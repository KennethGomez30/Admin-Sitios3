import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export function ProtectedRoute({ children, ruta }) {
    const { user, loading, motivoCierre, sesionExpirando } = useAuth()
    const location = useLocation()

    // Sesión aún cargando
    if (loading) {
        return (
            <div
                style={{
                    minHeight: '100vh',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    background: 'linear-gradient(135deg, #4e73df 0%, #224abe 100%)',
                }}
            >
                <div
                    className="spinner-border text-light"
                    style={{ width: '3rem', height: '3rem' }}
                    role="status"
                >
                    <span className="sr-only">Cargando...</span>
                </div>
            </div>
        )
    }

    // Sesión en proceso de expirar: el modal ya está visible encima de la página.
    // No redirigir aquí; la navegación la maneja AuthContext tras el delay del modal.
    if (sesionExpirando) {
        return children
    }

    // Sin sesión activa
    if (!user) {
        // Cuando la sesión expiró por inactividad, AuthContext ya se encarga
        // de navegar a /login después del modal. Si por alguna razón llegamos
        // aquí con motivoCierre === 'expirada' y el modal ya terminó, dejamos
        // pasar la redirección a /login sin query param (no mostrar msg=logout).
        if (motivoCierre === 'logout') return <Navigate to="/login?msg=logout" replace />
        return <Navigate to="/login?msg=nosesion" replace />
    }

    // Verificación de permiso por pantalla
    if (ruta) {
        const pantallas = user.pantallas ?? []
        const tienePantalla = pantallas.some((p) => p.ruta === ruta)

        if (!tienePantalla) {
            return (
                <Navigate
                    to="/dashboard"
                    replace
                    state={{ sinPermiso: true, rutaIntentada: location.pathname }}
                />
            )
        }
    }

    // Acceso permitido
    return children
}