import { ENV } from '../config/env'

const BASE_URL = ENV.DIRECCIONES_API_URL

// Helper: arma headers con Authorization (X-Usuario ya no se usa)
function buildHeaders(accessToken) {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${accessToken}`,
    }
}

// Helper: parsea respuesta y normaliza errores
async function parseResponse(res) {
    // Sin body (DELETE)
    if (res.status === 204) return null

    let data = null
    try {
        data = await res.json()
    } catch {
        // Sin body parseable
    }
    return data
}

export const direccionService = {

    /** GET /api/TerceroDirecciones/tercero/{terceroId} → Array<Direccion> */
    async listarPorTercero(terceroId, accessToken) {
        const res = await fetch(`${BASE_URL}/tercero/${terceroId}`, {
            method: 'GET',
            headers: buildHeaders(accessToken),
        })

        if (res.status === 401) {
            return { ok: false, status: 401, message: 'No autorizado.' }
        }
        if (!res.ok) {
            const data = await parseResponse(res)
            return { ok: false, status: res.status, message: data?.message ?? 'Error al cargar direcciones.' }
        }

        const data = await parseResponse(res)
        return { ok: true, status: 200, data: data ?? [] }
    },

    /** GET /api/TerceroDirecciones/{id} → Direccion */
    async obtenerPorId(id, accessToken) {
        const res = await fetch(`${BASE_URL}/${id}`, {
            method: 'GET',
            headers: buildHeaders(accessToken),
        })

        if (res.status === 401) return { ok: false, status: 401, message: 'No autorizado.' }
        if (res.status === 404) return { ok: false, status: 404, message: 'La dirección no existe.' }
        if (!res.ok) {
            const data = await parseResponse(res)
            return { ok: false, status: res.status, message: data?.message ?? 'Error al obtener la dirección.' }
        }

        const data = await parseResponse(res)
        return { ok: true, status: 200, data }
    },

    /** POST /api/TerceroDirecciones → Direccion creada */
    async crear(payload, accessToken) {
        const res = await fetch(`${BASE_URL}`, {
            method: 'POST',
            headers: buildHeaders(accessToken),
            body: JSON.stringify(payload),
        })

        if (res.status === 401) return { ok: false, status: 401, message: 'No autorizado.' }
        if (res.status === 400) {
            const data = await parseResponse(res)
            return { ok: false, status: 400, message: data?.message ?? 'Datos inválidos.' }
        }
        if (!res.ok) {
            const data = await parseResponse(res)
            return { ok: false, status: res.status, message: data?.message ?? 'Error al crear la dirección.' }
        }

        const data = await parseResponse(res)
        return { ok: true, status: 201, data }
    },

    /** PUT /api/TerceroDirecciones/{id} → Direccion actualizada */
    async actualizar(id, payload, accessToken) {
        const res = await fetch(`${BASE_URL}/${id}`, {
            method: 'PUT',
            headers: buildHeaders(accessToken),
            body: JSON.stringify(payload),
        })

        if (res.status === 401) return { ok: false, status: 401, message: 'No autorizado.' }
        if (res.status === 404) return { ok: false, status: 404, message: 'La dirección no existe.' }
        if (res.status === 400) {
            const data = await parseResponse(res)
            return { ok: false, status: 400, message: data?.message ?? 'Datos inválidos.' }
        }
        if (!res.ok) {
            const data = await parseResponse(res)
            return { ok: false, status: res.status, message: data?.message ?? 'Error al actualizar la dirección.' }
        }

        const data = await parseResponse(res)
        return { ok: true, status: 200, data }
    },

    /** DELETE /api/TerceroDirecciones/{id} */
    async eliminar(id, accessToken) {
        const res = await fetch(`${BASE_URL}/${id}`, {
            method: 'DELETE',
            headers: buildHeaders(accessToken),
        })

        if (res.status === 401) return { ok: false, status: 401, message: 'No autorizado.' }
        if (res.status === 404) return { ok: false, status: 404, message: 'La dirección no existe.' }
        if (res.status === 409) {
            const data = await parseResponse(res)
            return { ok: false, status: 409, message: data?.message ?? 'No se puede eliminar un registro con datos relacionados.' }
        }
        if (!res.ok) {
            const data = await parseResponse(res)
            return { ok: false, status: res.status, message: data?.message ?? 'Error al eliminar la dirección.' }
        }

        return { ok: true, status: 204 }
    },
}