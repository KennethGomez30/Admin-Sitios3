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

// Tiempo de inactividad máximo: 5 minutos
const INACTIVIDAD_MS = 5 * 60 * 1000

// Delay que se muestra el modal antes de redirigir al login
const DELAY_MODAL_MS = 3000

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

// Modal de sesión expirada vive dentro del Provider encima de todo

function ModalSesionExpirada({ visible }) {
    const [mostrar, setMostrar] = useState(false)

    useEffect(() => {
        if (!visible) { setMostrar(false); return }
        const id = setTimeout(() => setMostrar(true), 50)
        return () => clearTimeout(id)
    }, [visible])

    if (!visible) return null

    return (
        <>
            {/* Backdrop */}
            <div
                aria-hidden="true"
                style={{
                    position: 'fixed',
                    inset: 0,
                    backgroundColor: 'rgba(0,0,0,0.5)',
                    zIndex: 1040,
                    transition: 'opacity 0.3s ease',
                    opacity: mostrar ? 1 : 0,
                }}
            />

            {/* Modal centrado */}
            <div
                role="dialog"
                aria-modal="true"
                aria-labelledby="modalSesionExpiradaLabel"
                style={{
                    position: 'fixed',
                    inset: 0,
                    zIndex: 1050,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    padding: '1rem',
                    transition: 'opacity 0.3s ease',
                    opacity: mostrar ? 1 : 0,
                }}
            >
                <div
                    className="modal-dialog modal-dialog-centered"
                    style={{ margin: 0, width: '100%', maxWidth: '500px' }}
                >
                    <div className="modal-content shadow-lg">

                        {/* Header rojo — igual que todos los modales del sistema */}
                        <div className="modal-header bg-danger text-white">
                            <h5 className="modal-title font-weight-bold" id="modalSesionExpiradaLabel">
                                <i className="fas fa-clock mr-2" aria-hidden="true" />
                                Sesión Expirada
                            </h5>
                            {/* Sin botón de cierre: el usuario no puede descartar este modal */}
                        </div>

                        {/* Body */}
                        <div className="modal-body">
                            <p className="mb-0">
                                Su sesión ha expirado por inactividad. Será redirigido al inicio de sesión.
                            </p>
                        </div>

                    </div>
                </div>
            </div>
        </>
    )
}

// Provider

export function AuthProvider({ children }) {
    const [user, setUser] = useState(null)
    const [loading, setLoading] = useState(() => {
        return !!(localStorage.getItem(KEYS.ACCESS) && localStorage.getItem(KEYS.REFRESH))
    })
    const [motivoCierre, setMotivoCierre] = useState(null)
    const [sesionExpirando, setSesionExpirando] = useState(false)

    const refreshTimerRef = useRef(null)
    const cerrarSesionRef = useRef(null)
    const programarRefreshRef = useRef(null)
    const userRef = useRef(null)
    const ultimaActividadRef = useRef(null)
    const actividadHandlerRef = useRef(null)
    const navegarRef = useRef(null)

    // Bandera en ref para evitar que cerrarSesion se llame más de una vez
    // (ej: múltiples clicks rápidos cuando ya está expirando)
    const cerrandoRef = useRef(false)

    userRef.current = user

    // cerrarSesion
    const cerrarSesion = useCallback((llamarApi = true, motivo = 'logout') => {
        if (refreshTimerRef.current) clearTimeout(refreshTimerRef.current)

        if (actividadHandlerRef.current) {
            window.removeEventListener('click', actividadHandlerRef.current, true)
            actividadHandlerRef.current = null
        }

        const accessToken = llamarApi ? localStorage.getItem(KEYS.ACCESS) : null

        if (motivo === 'expirada') {
            // Mostrar modal. ProtectedRoute devuelve children mientras
            // sesionExpirando === true, así la página actual permanece visible.
            setSesionExpirando(true)

            setTimeout(() => {
                limpiarSesion()
                setMotivoCierre('expirada')
                setUser(null)
                cerrandoRef.current = false
                setSesionExpirando(false)

                if (navegarRef.current) {
                    navegarRef.current('/login', { replace: true })
                }
            }, DELAY_MODAL_MS)

        } else {
            limpiarSesion()
            setMotivoCierre(motivo)
            setUser(null)
            cerrandoRef.current = false
        }

        if (accessToken) {
            authService.logout(accessToken).catch(() => { /* ignorar */ })
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

    // iniciarInactividad
    const iniciarInactividad = useCallback(() => {
        if (actividadHandlerRef.current) {
            window.removeEventListener('click', actividadHandlerRef.current, true)
            actividadHandlerRef.current = null
        }

        cerrandoRef.current = false
        ultimaActividadRef.current = Date.now()

        const handler = (e) => {
            // Si ya se está procesando el cierre, bloquear cualquier click adicional
            if (cerrandoRef.current) {
                e.preventDefault()
                e.stopPropagation()
                return
            }

            if (!userRef.current) return

            const inactivo = Date.now() - ultimaActividadRef.current >= INACTIVIDAD_MS

            if (inactivo) {
                // Marcar que estamos cerrando para ignorar clicks posteriores
                cerrandoRef.current = true
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
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    return (
        <AuthContext.Provider value={{ user, loading, motivoCierre, sesionExpirando, iniciarSesion, cerrarSesion, navegarRef }}>
            {children}
            {/* Modal global de sesión expirada: se renderiza encima de cualquier pantalla */}
            <ModalSesionExpirada visible={sesionExpirando} />
        </AuthContext.Provider>
    )
}