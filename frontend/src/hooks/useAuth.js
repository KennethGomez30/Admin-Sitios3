import { useContext } from 'react'
import { AuthContext } from '../context/AuthContext'

/**
 * Hook para consumir el AuthContext.
 *
 * Expone:
 *  - user          {object|null}    Sesión activa o null
 *  - loading       {boolean}        true mientras se restaura la sesión al arrancar
 *  - motivoCierre  {string|null}    'expirada' si la sesión cerró por timeout, null en otro caso
 *  - iniciarSesion {function}       (identificacion, contrasena) => Promise<void>
 *  - cerrarSesion  {function}       (llamarApi?, motivo?) => Promise<void>
 *
 * @throws {Error} si se usa fuera de <AuthProvider>
 */
export function useAuth() {
    const ctx = useContext(AuthContext)
    if (!ctx) throw new Error('useAuth debe usarse dentro de <AuthProvider>')
    return ctx
}