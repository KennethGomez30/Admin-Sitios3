import { createContext, useState, useEffect, useCallback, useRef } from 'react'
import { authService } from '../services/authService'
import { permisosService } from '../services/permisosService'

export const AuthContext = createContext(null)

// Claves de localStorage
const KEYS = {
    ACCESS: 'sc_access_token',
    REFRESH: 'sc_refresh_token',
    EXPIRES: 'sc_expires_in',
    USUARIO: 'sc_usuario_id',
    NOMBRE: 'sc_usuario_nombre',
    PANTALLAS: 'sc_pantallas',
}

// Tiempo de inactividad máximo 5 minutos
const INACTIVIDAD_MS = 5 * 60 * 1000

// Helpers de sesión

function guardarSesion({ accessToken, refreshToken, expiresIn, usuarioId, usuarioNombre, pantallas }) {
    localStorage.setItem(KEYS.ACCESS, accessToken)
    localStorage.setItem(KEYS.REFRESH, refreshToken)
    localStorage.setItem(KEYS.EXPIRES, expiresIn)
    localStorage.setItem(KEYS.USUARIO, usuarioId)
    localStorage.setItem(KEYS.NOMBRE, usuarioNombre ?? '')
    localStorage.setItem(KEYS.PANTALLAS, JSON.stringify(pantallas ?? []))
}

function limpiarSesion() {
    Object.values(KEYS).forEach((k) => localStorage.removeItem(k))
}

function cargarSesion() {
    const accessToken = localStorage.getItem(KEYS.ACCESS)
    const refreshToken = localStorage.getItem(KEYS.REFRESH)
    if (!accessToken || !refreshToken) return null

    let pantallas = []
    try {
        pantallas = JSON.parse(localStorage.getItem(KEYS.PANTALLAS) ?? '[]')
        if (!Array.isArray(pantallas)) pantallas = []
    } catch {
        pantallas = []
    }

    return {
        accessToken,
        refreshToken,
        expiresIn: localStorage.getItem(KEYS.EXPIRES),
        usuarioId: localStorage.getItem(KEYS.USUARIO),
        usuarioNombre: localStorage.getItem(KEYS.NOMBRE) ?? '',
        pantallas,
    }
}

function mapearTokensAux1(obj) {
    return {
        accessToken: obj.accessToken ?? obj.AccessToken ?? '',
        refreshToken: obj.refreshToken ?? obj.RefreshToken ?? '',
        expiresIn: obj.expiresIn ?? obj.ExpiresIn ?? '',
        usuarioId: obj.usuarioId ?? obj.UsuarioId ?? '',
    }
}

// Provider

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null)
    const [loading, setLoading] = useState(() => {
        return !!(localStorage.getItem(KEYS.ACCESS) && localStorage.getItem(KEYS.REFRESH))
    })
    const [motivoCierre, setMotivoCierre] = useState(null)

    const refreshTimerRef = useRef(null)
    const cerrarSesionRef = useRef(null)
    const programarRefreshRef = useRef(null)
    const userRef = useRef(null)
    const ultimaActividadRef = useRef(null)
    const actividadHandlerRef = useRef(null)

    userRef.current = user

    const cerrarSesion = useCallback((llamarApi = true, motivo = 'logout') => {
        if (refreshTimerRef.current) clearTimeout(refreshTimerRef.current)

        if (actividadHandlerRef.current) {
            window.removeEventListener('click', actividadHandlerRef.current, true)
            actividadHandlerRef.current = null
        }

        // Leer el token antes de limpiar para poder llamar al API después
        const accessToken = llamarApi ? localStorage.getItem(KEYS.ACCESS) : null

        // Limpiar localStorage y actualizar estado de forma síncrona
        // para que ProtectedRoute vea user=null y motivoCierre juntos en el mismo render
        limpiarSesion()
        setMotivoCierre(motivo)
        setUser(null)

        // Logout al API en segundo plano sin await
        if (accessToken) {
            authService.logout(accessToken).catch(() => { /* best-effort */ })
        }
    }, [])

    cerrarSesionRef.current = cerrarSesion

    // programarRefresh

    const programarRefresh = useCallback((expiresIn, refreshToken) => {
        if (refreshTimerRef.current) clearTimeout(refreshTimerRef.current)

        const expMs = new Date(expiresIn).getTime()
        const delay = isNaN(expMs)
            ? 4 * 60 * 1000
            : Math.max(expMs - Date.now() - 30_000, 0)

        refreshTimerRef.current = setTimeout(async () => {
            if (!userRef.current) return
            try {
                const data = await authService.refresh(refreshToken)
                if (data?.statusCode !== 200) throw new Error('Refresh rechazado')

                const tokens = mapearTokensAux1(data.responseObject)
                const sesionCompleta = {
                    ...tokens,
                    usuarioNombre: userRef.current.usuarioNombre,
                    pantallas: userRef.current.pantallas,
                }

                guardarSesion(sesionCompleta)
                setUser(sesionCompleta)
                programarRefreshRef.current(sesionCompleta.expiresIn, sesionCompleta.refreshToken)
            } catch {
                cerrarSesionRef.current(false, 'expirada')
            }
        }, delay)
    }, [])

    programarRefreshRef.current = programarRefresh

    const iniciarInactividad = useCallback(() => {
        if (actividadHandlerRef.current) {
            window.removeEventListener('click', actividadHandlerRef.current, true)
        }

        ultimaActividadRef.current = Date.now()

        const handler = (e) => {
            if (!userRef.current) return

            const inactivo = Date.now() - ultimaActividadRef.current > INACTIVIDAD_MS

            if (inactivo) {
                // Cancelar el click original para que no dispare navegaciones
                // ni acciones mientras se procesa el cierre de sesión
                e.preventDefault()
                e.stopPropagation()
                cerrarSesionRef.current(false, 'expirada')
                return
            }

            ultimaActividadRef.current = Date.now()
        }

        actividadHandlerRef.current = handler
        window.addEventListener('click', handler, true)
    }, [])

    // iniciarSesion

    const iniciarSesion = useCallback(async (identificacion, contrasena) => {
        const data = await authService.login(identificacion, contrasena)
        if (data.statusCode !== 200) throw new Error(data.message ?? 'Credenciales inválidas.')

        const tokens = mapearTokensAux1(data.responseObject)

        const perfil = await permisosService.obtenerPerfil(tokens.accessToken)

        const sesionCompleta = {
            ...tokens,
            usuarioNombre: perfil.usuarioNombre,
            pantallas: perfil.pantallas,
        }

        guardarSesion(sesionCompleta)
        setMotivoCierre(null)
        setUser(sesionCompleta)
        programarRefreshRef.current(sesionCompleta.expiresIn, sesionCompleta.refreshToken)
        iniciarInactividad()
    }, [iniciarInactividad])

    // Restaurar sesión al montar

    useEffect(() => {
        async function restaurar() {
            const sesion = cargarSesion()
            if (!sesion) {
                setLoading(false)
                return
            }

            const valido = await authService.validate(sesion.accessToken)

            if (valido) {
                const perfil = await permisosService.obtenerPerfil(sesion.accessToken)
                const sesionCompleta = {
                    ...sesion,
                    usuarioNombre: perfil.usuarioNombre,
                    pantallas: perfil.pantallas,
                }

                guardarSesion(sesionCompleta)
                setUser(sesionCompleta)
                programarRefreshRef.current(sesionCompleta.expiresIn, sesionCompleta.refreshToken)
                iniciarInactividad()
                setLoading(false)
                return
            }

            try {
                const data = await authService.refresh(sesion.refreshToken)
                if (data?.statusCode !== 200) throw new Error('Refresh rechazado')

                const tokens = mapearTokensAux1(data.responseObject)
                const perfil = await permisosService.obtenerPerfil(tokens.accessToken)
                const sesionCompleta = {
                    ...tokens,
                    usuarioNombre: perfil.usuarioNombre,
                    pantallas: perfil.pantallas,
                }

                guardarSesion(sesionCompleta)
                setUser(sesionCompleta)
                programarRefreshRef.current(sesionCompleta.expiresIn, sesionCompleta.refreshToken)
                iniciarInactividad()
            } catch {
                limpiarSesion()
            }

            setLoading(false)
        }

        restaurar()

        return () => {
            if (refreshTimerRef.current) clearTimeout(refreshTimerRef.current)
            if (actividadHandlerRef.current) {
                window.removeEventListener('click', actividadHandlerRef.current, true)
            }
        }
    }, [iniciarInactividad])

    return (
        <AuthContext.Provider value={{ user, loading, motivoCierre, iniciarSesion, cerrarSesion }}>
            {children}
        </AuthContext.Provider>
    )
}