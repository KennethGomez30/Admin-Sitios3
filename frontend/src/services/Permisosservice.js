import { ENV } from '../config/env'

const BASE_URL = ENV.PERMISOS_API_URL

export const permisosService = {

    /**
     * GET /api/Permisos/perfil
     * Requiere Authorization Bearer <accessToken de AUX1>
     *
     * PERMISOS valida el token contra AUX1 extrae LA identificacion
     * y devuelve el nombre completo del usuario y sus pantallas según roles
     *
     * @returns {Promise<{ usuarioNombre: string, pantallas: PantallaDto[] }>}
     *   Siempre devuelve un objeto con valores por defecto, nunca lanza.
     */
    async obtenerPerfil(accessToken) {
        try {
            const res = await fetch(`${BASE_URL}/perfil`, {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${accessToken}`,
                },
            })

            if (!res.ok) return { usuarioNombre: '', pantallas: [] }

            // PERMISOS devuelve { usuarioNombre, pantallas: [{ nombre, ruta, menuSeccion }] }
            return await res.json()
        } catch {
            return { usuarioNombre: '', pantallas: [] }
        }
    },
}