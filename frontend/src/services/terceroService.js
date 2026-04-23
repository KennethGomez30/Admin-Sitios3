import { ENV } from '../config/env'

const BASE_URL = ENV.TERCEROS_API_URL

async function handleResponse(res) {
    // 204 No Content — DELETE exitoso sin body
    if (res.status === 204) return { statusCode: 204 }

    let data
    try {
        data = await res.json()
    } catch {
        throw new Error(`Error de servidor (HTTP ${res.status}). Intente de nuevo.`)
    }

    // El backend siempre retorna BusinessLogicResponse: { statusCode, message, responseObject }
    return data
}

// Servicio exportado
export const terceroService = {

    /**
     * GET /api/Terceros
     * @returns {Promise<{ statusCode, message, responseObject: TerceroEntity[] }>}
     */
    async listar(accessToken) {
        const res = await fetch(`${BASE_URL}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    /**
     * GET /api/Terceros/{id}
     * @returns {Promise<{ statusCode, message, responseObject: TerceroEntity }>}
     */
    async obtenerPorId(id, accessToken) {
        const res = await fetch(`${BASE_URL}/${id}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    /**
     * POST /api/Terceros
     * body: { Identificacion, NombreRazonSocial, Tipo, Email?, Telefono?, Estado }
     * @returns {Promise<{ statusCode, message, responseObject: TerceroEntity }>}
     */
    async crear(payload, accessToken) {
        const res = await fetch(`${BASE_URL}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
            body: JSON.stringify(payload),
        })
        return handleResponse(res)
    },

    /**
     * PUT /api/Terceros/{id}
     * body: { NombreRazonSocial, Tipo, Email?, Telefono?, Estado }
     * @returns {Promise<{ statusCode, message, responseObject: TerceroEntity }>}
     */
    async actualizar(id, payload, accessToken) {
        const res = await fetch(`${BASE_URL}/${id}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
            body: JSON.stringify(payload),
        })
        return handleResponse(res)
    },

    /**
     * DELETE /api/Terceros/{id}
     * @returns {Promise<{ statusCode: 204 }>}
     */
    async eliminar(id, accessToken) {
        const res = await fetch(`${BASE_URL}/${id}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },
}