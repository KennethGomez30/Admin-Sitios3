import { ENV } from '../config/env'

const BASE_URL = ENV.AUTH_API_URL

// Helper interno

async function handleResponse(res) {
    // 204 No Content da logout o validate exitoso, sin body
    if (res.status === 204) return null

    // Proteger el JSON.parse si el servidor devuelve HTML 502, 503, proxy error, no explotar
    let data
    try {
        data = await res.json()
    } catch {
        throw new Error(`Error de servidor (HTTP ${res.status}). Intente de nuevo.`)
    }

    if (!res.ok) {
        // El backend devuelve { statusCode, message, responseObject }
        throw new Error(data?.message ?? `Error HTTP ${res.status}`)
    }

    return data
}

// Servicio exportado

export const authService = {
    /**
     * POST /login
     * @returns {Promise<{ statusCode: number, message: string, responseObject: LoginResponse }>}
     * LoginResponse: { accessToken, refreshToken, expiresIn, usuarioId, usuarioNombre }
     */
    async login(identificacion, contrasena) {
        const res = await fetch(`${BASE_URL}/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ Identificacion: identificacion, Contrasena: contrasena }),
        })
        return handleResponse(res)
    },

    /**
     * POST /refresh
     * @returns {Promise<{ statusCode: number, message: string, responseObject: RefreshResponse }>}
     * RefreshResponse: { accessToken, refreshToken, expiresIn, usuarioId, usuarioNombre }
     */
    async refresh(refreshToken) {
        const res = await fetch(`${BASE_URL}/refresh`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ RefreshToken: refreshToken }),
        })
        return handleResponse(res)
    },

    /**
     * POST /validate
     * @returns {Promise<boolean>} true si el token es válido  204, false en cualquier otro caso
     */
    async validate(token) {
        try {
            const res = await fetch(`${BASE_URL}/validate`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Token: token }),
            })
            return res.status === 204
        } catch {
            return false
        }
    },

    /**
     * POST /logout
     * Requiere cabecera Authorization: Bearer <accessToken>
     * @returns {Promise<null>} 204  logout exitoso
     */
    async logout(accessToken) {
        const res = await fetch(`${BASE_URL}/logout`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },
}