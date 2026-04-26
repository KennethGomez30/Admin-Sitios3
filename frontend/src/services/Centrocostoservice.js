import { ENV } from '../config/env'

const BASE_URL = ENV.CENTRO_COSTO_API_URL

async function handleResponse(res) {
    let data
    try {
        data = await res.json()
    } catch {
        throw new Error(`Error de servidor (HTTP ${res.status}). Intente de nuevo.`)
    }
    return data
}

export const centroCostoService = {

    /** GET /api/CentroCosto/periodos */
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

    /** GET /api/CentroCosto/lineas?periodo_id={id} */
    async listarLineas(periodoId, accessToken) {
        const res = await fetch(`${BASE_URL}/lineas?periodo_id=${periodoId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    /** GET /api/CentroCosto/prorrateo/{detalleId} */
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

    /** POST /api/CentroCosto/prorrateo/agregar */
    async agregarDistribucion(payload, accessToken) {
        const res = await fetch(`${BASE_URL}/prorrateo/agregar`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
            body: JSON.stringify(payload),
        })
        return handleResponse(res)
    },

    /** POST /api/CentroCosto/prorrateo/eliminar */
    async eliminarDistribucion(payload, accessToken) {
        const res = await fetch(`${BASE_URL}/prorrateo/eliminar`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
            body: JSON.stringify(payload),
        })
        return handleResponse(res)
    },
}