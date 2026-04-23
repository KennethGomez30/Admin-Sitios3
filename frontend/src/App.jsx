import { BrowserRouter, Routes, Route, Navigate, useNavigate } from 'react-router-dom'
import { useEffect } from 'react'
import { AuthProvider } from './context/AuthContext'
import { useAuth } from './hooks/useAuth'
import { ProtectedRoute } from './components/ProtectedRoute'
import Login from './components/Login'
import Dashboard from './components/Dashboard'
import TerceroListado from './components/TerceroListado'
import TerceroCrear from './components/TerceroCrear'
import TerceroEditar from './components/TerceroEditar'

function NavegadorConectado() {
  const navigate = useNavigate()
  const { navegarRef } = useAuth()

  useEffect(() => {
    navegarRef.current = navigate
  }, [navigate, navegarRef])

  return null
}

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        {/* Conecta useNavigate al AuthContext sin romper la jerarquía */}
        <NavegadorConectado />

        <Routes>

          {/* Raíz */}
          <Route path="/" element={<Navigate to="/login" replace />} />

          {/* Públicas */}
          <Route path="/login" element={<Login />} />

          {/* Dashboard accesible a todo usuario autenticado */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            }
          />

          {/* Módulo Terceros */}
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