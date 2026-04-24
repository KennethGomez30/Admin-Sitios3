import { ENV } from '../config/env'

const BASE_URL = ENV.REPORTE_TERCEROS_API_URL

async function handleResponse(res) {
    let data
    try {
        data = await res.json()
    } catch {
        throw new Error(`Error de servidor (HTTP ${res.status}). Intente de nuevo.`)
    }
    return data
}

export const reporteTercerosService = {

    /** GET /api/ReporteTerceros/terceros */
    async listarTerceros(accessToken) {
        const res = await fetch(`${BASE_URL}/terceros`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },

    /**
     * GET /api/ReporteTerceros/movimientos
     * @param {Object} filtros - { terceroId, fechaInicio, fechaFin, periodoId, estado, pagina }
     */
    async obtenerMovimientos(filtros, accessToken) {
        const params = new URLSearchParams()
        params.append('tercero_id', filtros.terceroId)
        if (filtros.fechaInicio) params.append('fecha_inicio', filtros.fechaInicio)
        if (filtros.fechaFin)    params.append('fecha_fin',    filtros.fechaFin)
        if (filtros.periodoId)   params.append('periodo_id',   filtros.periodoId)
        if (filtros.estado)      params.append('estado',       filtros.estado)
        params.append('pagina', filtros.pagina ?? 1)

        const res = await fetch(`${BASE_URL}/movimientos?${params.toString()}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${accessToken}`,
            },
        })
        return handleResponse(res)
    },
}