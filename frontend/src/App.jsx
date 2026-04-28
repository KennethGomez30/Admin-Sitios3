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
import CentroCostoListado from './components/CentroCostoListado'
import CentroCostoProrrateo from './components/CentroCostoProrrateo'
import ReporteTerceros from './components/ReporteTerceros'
import CentroCostoAdminListado from './components/ListaCentrosCostos'
import CentroCostoAdminCrear from './components/CentroCostosCrear'
import CentroCostoEditar from './components/CentroCostoEditar'
import ReporteCentroCosto from './components/ReporteCentroCosto'

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

          {/* ── Módulo Terceros ── */}
          <Route
            path="/terceros"
            element={
              <ProtectedRoute ruta="/terceros">
                <Dashboard><TerceroListado /></Dashboard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/terceros/crear"
            element={
              <ProtectedRoute ruta="/terceros">
                <Dashboard><TerceroCrear /></Dashboard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/terceros/editar/:id"
            element={
              <ProtectedRoute ruta="/terceros">
                <Dashboard><TerceroEditar /></Dashboard>
              </ProtectedRoute>
            }
          />

          {/* ── AUX7: Distribución por Centro de Costo ── */}
          <Route
            path="/centro-costo"
            element={
              <ProtectedRoute ruta="/centro-costo">
                <Dashboard><CentroCostoListado /></Dashboard>
              </ProtectedRoute>
            }
          />
          <Route
            path="/centro-costo/prorrateo/:detalleId"
            element={
              <ProtectedRoute ruta="/centro-costo">
                <Dashboard><CentroCostoProrrateo /></Dashboard>
              </ProtectedRoute>
            }
          />

          {/* ── AUX9: Reporte de Movimientos por Tercero ── */}
          <Route
            path="/reporte-terceros"
            element={
              <ProtectedRoute ruta="/reporte-terceros">
                <Dashboard><ReporteTerceros /></Dashboard>
              </ProtectedRoute>
            }
          />
       
      {/* ── AUX6: Administración de Centros de Costo ── */}
           <Route
             path="/centro-costo/admin"
             element={
              <ProtectedRoute ruta="/centro-costo/admin">
                <Dashboard><CentroCostoAdminListado /></Dashboard>
              </ProtectedRoute>
              } 
           />
            <Route
              path="/centro-costo/crear"
              element={
                <ProtectedRoute ruta="/centro-costo/admin">
                  <Dashboard><CentroCostoAdminCrear /></Dashboard>
                </ProtectedRoute>
                }
            />
          <Route
            path="/centro-costo/editar/:codigo"
            element={
            <ProtectedRoute ruta="/centro-costo/admin">
              <Dashboard><CentroCostoEditar /></Dashboard>
            </ProtectedRoute>
            }
          /> 

          <Route
            path="/reporte-centro-costo"
            element={
              <ProtectedRoute ruta="/reporte-centro-costo">
                <Dashboard><ReporteCentroCosto /></Dashboard>
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