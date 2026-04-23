import { useState, useEffect, useRef } from 'react'
import { useNavigate, useSearchParams, Navigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import '../styles/login.css'

// Mensajes por código de query param
const MENSAJES_INFO = {
    nosesion: 'Por favor inicie sesión para utilizar el sistema.',
    logout: 'Ha cerrado sesión correctamente.',
}

export default function Login() {
    const { user, iniciarSesion } = useAuth()
    const navigate = useNavigate()
    const [searchParams] = useSearchParams()

    const [identificacion, setIdentificacion] = useState('')
    const [contrasena, setContrasena] = useState('')
    const [error, setError] = useState('')
    const [info, setInfo] = useState('')
    const [cargando, setCargando] = useState(false)
    const [mostrarError, setMostrarError] = useState(false)
    const [mostrarInfo, setMostrarInfo] = useState(false)

    const timerErrorRef = useRef(null)
    const timerInfoRef = useRef(null)

    // Leer ?msg= y limpiar la URL para que al refrescar no persista
    useEffect(() => {
        const msg = searchParams.get('msg')
        const texto = MENSAJES_INFO[msg] ?? null

        if (texto) {
            setInfo(texto)
            setMostrarInfo(true)
            window.history.replaceState({}, '', '/login')
        }
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [])

    // Auto-cierre alerta ERROR 5 s
    useEffect(() => {
        if (!mostrarError) return
        const id = setTimeout(() => setMostrarError(false), 5000)
        timerErrorRef.current = id
        return () => clearTimeout(id)
    }, [mostrarError])

    // Auto-cierre alerta INFO 5 s
    useEffect(() => {
        if (!mostrarInfo) return
        const id = setTimeout(() => setMostrarInfo(false), 5000)
        timerInfoRef.current = id
        return () => clearTimeout(id)
    }, [mostrarInfo])

    // Limpieza al desmontar
    useEffect(() => {
        return () => {
            clearTimeout(timerErrorRef.current)
            clearTimeout(timerInfoRef.current)
        }
    }, [])

    // Si ya hay sesión activa ir directo al dashboard
    if (user) return <Navigate to="/dashboard" replace />

    // Submit
    async function handleSubmit(e) {
        e.preventDefault()

        if (!identificacion.trim() || !contrasena.trim()) {
            setError('Usuario y/o contraseña incorrectos.')
            setMostrarError(true)
            return
        }

        setError('')
        setMostrarError(false)
        setCargando(true)

        try {
            await iniciarSesion(identificacion.trim(), contrasena)
            navigate('/dashboard', { replace: true })
        } catch (err) {
            setError(err.message ?? 'Usuario y/o contraseña incorrectos.')
            setMostrarError(true)
        } finally {
            setCargando(false)
        }
    }

    // Render
    return (
        <div className="login-page">
            <div className="login-box">

                {/* Header azul */}
                <div className="login-header">
                    <div className="login-logo" aria-hidden="true">
                        <i className="fas fa-calculator rotate-n-15" />
                    </div>
                    <h1 className="h2 mb-0">Sistema Contable</h1>
                    <p className="mb-0 mt-2 login-company">Desarrollos Ordenados S.A.</p>
                </div>

                {/* Body blanco */}
                <div className="login-body">

                    {/* Alerta de error */}
                    {mostrarError && (
                        <div className="alert alert-danger alert-dismissible fade show" role="alert">
                            <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                            {error}
                            <button
                                type="button"
                                className="close"
                                onClick={() => {
                                    clearTimeout(timerErrorRef.current)
                                    setMostrarError(false)
                                }}
                                aria-label="Cerrar alerta de error"
                            >
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                    )}

                    {/* Alerta informativa (logout, nosesion, expirada) */}
                    {mostrarInfo && (
                        <div className="alert alert-warning alert-dismissible fade show" role="alert">
                            <i className="fas fa-info-circle mr-2" aria-hidden="true" />
                            {info}
                            <button
                                type="button"
                                className="close"
                                onClick={() => {
                                    clearTimeout(timerInfoRef.current)
                                    setMostrarInfo(false)
                                }}
                                aria-label="Cerrar alerta informativa"
                            >
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                    )}

                    {/* Formulario */}
                    <form onSubmit={handleSubmit} noValidate>

                        <div className="form-group">
                            <label htmlFor="identificacion" className="font-weight-bold text-gray-700">
                                <i className="fas fa-user mr-1" aria-hidden="true" /> Usuario
                            </label>
                            <input
                                type="text"
                                id="identificacion"
                                name="identificacion"
                                className="form-control form-control-lg"
                                placeholder="Ingrese su usuario"
                                value={identificacion}
                                onChange={(e) => setIdentificacion(e.target.value)}
                                autoComplete="username"
                                disabled={cargando}
                                autoFocus
                            />
                        </div>

                        <div className="form-group mb-4">
                            <label htmlFor="contrasena" className="font-weight-bold text-gray-700">
                                <i className="fas fa-lock mr-1" aria-hidden="true" /> Contraseña
                            </label>
                            <input
                                type="password"
                                id="contrasena"
                                name="contrasena"
                                className="form-control form-control-lg"
                                placeholder="Ingrese su contraseña"
                                value={contrasena}
                                onChange={(e) => setContrasena(e.target.value)}
                                autoComplete="current-password"
                                disabled={cargando}
                            />
                        </div>

                        <button
                            type="submit"
                            className="btn btn-login btn-lg btn-block"
                            disabled={cargando}
                        >
                            {cargando ? (
                                <>
                                    <span
                                        className="spinner-border spinner-border-sm"
                                        role="status"
                                        aria-hidden="true"
                                    />
                                    Ingresando...
                                </>
                            ) : (
                                <>
                                    <i className="fas fa-sign-in-alt mr-2" aria-hidden="true" />
                                    Ingresar
                                </>
                            )}
                        </button>

                    </form>
                </div>
            </div>
        </div>
    )
}