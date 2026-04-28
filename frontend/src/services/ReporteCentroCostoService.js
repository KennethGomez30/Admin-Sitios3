import { ENV } from '../config/env'

const BASE_URL = ENV.REPORTE_CC_API_URL

async function handleResponse(res) {
    let data
    try {
        data = await res.json()
    } catch {
        throw new Error(`Error HTTP ${res.status}`)
    }

    return data
}

export const reporteCentroCostoService = {
    async listarCentrosCosto(accessToken) {
    const res = await fetch(`${BASE_URL}/centro-costo`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${accessToken}`,
        },
    })
    return handleResponse(res)
    },

    async obtenerMovimientos({
        centroCosto,
        fechaInicio,
        fechaFin,
        estado,
        pagina = 1,
        accessToken
    }) {
        const params = new URLSearchParams()

        if (centroCosto) params.append('centro_costo', centroCosto)
        if (fechaInicio) params.append('fecha_inicio', fechaInicio)
        if (fechaFin) params.append('fecha_fin', fechaFin)
        if (estado) params.append('estado', estado)

        params.append('pagina', pagina)

        const res = await fetch(`${BASE_URL}/movimientos?${params.toString()}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })

        return handleResponse(res)
    }
}