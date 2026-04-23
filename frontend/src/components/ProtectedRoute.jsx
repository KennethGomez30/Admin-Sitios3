import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

export function ProtectedRoute({ children, ruta }) {
    const { user, loading, motivoCierre } = useAuth()
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

    // Sin sesión activa
    if (!user) {
        if (motivoCierre === 'expirada') return <Navigate to="/sesion-expirada" replace />
        if (motivoCierre === 'logout') return <Navigate to="/login?msg=logout" replace />
        return <Navigate to="/login?msg=nosesion" replace />
    }

    // Verificación de permiso por pantalla
    if (ruta) {
        const pantallas = user.pantallas ?? []
        const tienePantalla = pantallas.some((p) => p.ruta === ruta)

        if (!tienePantalla) {
            // Guardar la ruta que intentó acceder para mostrar mensaje informativo
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