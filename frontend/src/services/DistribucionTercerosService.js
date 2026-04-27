import { ENV } from '../config/env'

const BASE_URL = ENV.AUX8_API_URL

async function handleResponse(res) {
    let data
    try {
        data = await res.json()
    } catch {
        throw new Error(`Error de servidor (HTTP ${res.status}). Intente de nuevo.`)
    }
    return data
}

export const distribucionTercerosService = {
    async listarPeriodos(accessToken) {
        const res = await fetch(`${BASE_URL}/periodos`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    async listarLineas(periodoId, accessToken) {
        const res = await fetch(`${BASE_URL}/lineas/${periodoId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    async obtenerProrrateo(detalleId, accessToken) {
        const res = await fetch(`${BASE_URL}/prorrateo/${detalleId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    async listarActivos(accessToken) {
        const res = await fetch(`${BASE_URL}/activos`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    async agregarDistribucion(payload, accessToken) {
        const res = await fetch(`${BASE_URL}/distribuciones`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
            body: JSON.stringify(payload),
        })
        return handleResponse(res)
    },

    async eliminarDistribucion(id, detalleId, accessToken) {
        const res = await fetch(`${BASE_URL}/distribuciones/${id}?detalleId=${detalleId}`, {
            method: 'DELETE',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },
}