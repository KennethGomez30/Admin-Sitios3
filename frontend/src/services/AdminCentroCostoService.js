import { ENV } from '../config/env'

const BASE_URL = ENV.ADMIN_CENTRO_COSTO_API_URL

async function handleResponse(res) {
    if (res.status === 204) return { statusCode: 204 }

    let data
    try {
        data = await res.json()
    } catch {
        throw new Error(`Error de servidor (HTTP ${res.status}). Intente de nuevo.`)
    }

    return data
}

export const centroCostoService = {

    /**
     * GET /api/CentroCosto?pagina=1
     * @returns {Promise<{ statusCode, message, responseObject: PagedCentroCostoResponse }>}
     */
    async listar(pagina = 1, accessToken) {
        const res = await fetch(`${BASE_URL}?pagina=${pagina}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    /**
     * GET /api/CentroCosto/{codigo}
     * @returns {Promise<{ statusCode, message, responseObject: CentroCostoDto }>}
     */
    async obtenerPorCodigo(codigo, accessToken) {
        const res = await fetch(`${BASE_URL}/${encodeURIComponent(codigo)}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    /**
     * POST /api/CentroCosto
     * body: { codigo, nombre, descripcion?, estado }
     * @returns {Promise<{ statusCode, message, responseObject }>}
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
     * PUT /api/CentroCosto/{codigo}
     * body: { codigo, nombre, descripcion?, estado }
     * @returns {Promise<{ statusCode, message, responseObject }>}
     */
    async actualizar(codigo, payload, accessToken) {
        const res = await fetch(`${BASE_URL}/${encodeURIComponent(codigo)}`, {
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
     * DELETE /api/CentroCosto/{codigo}
     * @returns {Promise<{ statusCode, message, responseObject }>}
     */
    async eliminar(codigo, accessToken) {
        const res = await fetch(`${BASE_URL}/${encodeURIComponent(codigo)}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },
}