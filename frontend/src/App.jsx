import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './context/AuthContext'
import { ProtectedRoute } from './components/ProtectedRoute'
import Login from './components/Login'
import Dashboard from './components/Dashboard'
import SesionExpirada from './components/SesionExpirada'
import TerceroListado from './components/TerceroListado'
import TerceroCrear from './components/TerceroCrear'
import TerceroEditar from './components/TerceroEditar'

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>

          {/* Raíz */}
          <Route path="/" element={<Navigate to="/login" replace />} />

          {/* Públicas */}
          <Route path="/login" element={<Login />} />
          <Route path="/sesion-expirada" element={<SesionExpirada />} />

          {/* ── Dashboard accesible a todo usuario autenticado ─ */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />

          {/* Módulo Terceros */}

          {/* Listado verifica permiso '/terceros' directamente */}
          <Route
            path="/terceros"
            element={
              <ProtectedRoute ruta="/terceros">
                <Dashboard>
                  <TerceroListado />
                </Dashboard>
              </ProtectedRoute>
            }
          />

          {/*
            * Crear y Editar usan ruta="/terceros" osea la pantalla padre
            * porque las pantallas con mostrar_en_menu = 0 no aparecen
            * en user.pantallas. El permiso sobre el módulo principal
            * cubre implícitamente sus sub-acciones.
          */}
          <Route
            path="/terceros/crear"
            element={
              <ProtectedRoute ruta="/terceros">
                <Dashboard>
                  <TerceroCrear />
                </Dashboard>
              </ProtectedRoute>
            }
          />

          <Route
            path="/terceros/editar/:id"
            element={
              <ProtectedRoute ruta="/terceros">
                <Dashboard>
                  <TerceroEditar />
                </Dashboard>
              </ProtectedRoute>
            }
          />

          {/* Comodín */}
          <Route path="*" element={<Navigate to="/login" replace />} />

        </Routes>
      </BrowserRouter>
    </AuthProvider>
  )
}