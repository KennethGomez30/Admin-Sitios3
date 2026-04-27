import { ENV } from '../config/env'

const BASE_URL = ENV.CONTACTOS_API_URL

function buildHeaders(accessToken) {
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${accessToken}`,
    }
}

async function parseResponse(res) {
    if (res.status === 204) return null
    let data = null
    try { data = await res.json() } catch { /* sin body */ }
    return data
}

export const contactoService = {

    /** GET /api/TerceroContactos/tercero/{terceroId} → Array<Contacto> */
    async listarPorTercero(terceroId, accessToken) {
        const res = await fetch(`${BASE_URL}/tercero/${terceroId}`, {
            method: 'GET',
            headers: buildHeaders(accessToken),
        })
        if (res.status === 401) return { ok: false, status: 401, message: 'No autorizado.' }
        if (!res.ok) {
            const data = await parseResponse(res)
            return { ok: false, status: res.status, message: data?.message ?? 'Error al cargar contactos.' }
        }
        const data = await parseResponse(res)
        return { ok: true, status: 200, data: data ?? [] }
    },

    /** GET /api/TerceroContactos/{id} → Contacto */
    async obtenerPorId(id, accessToken) {
        const res = await fetch(`${BASE_URL}/${id}`, {
            method: 'GET',
            headers: buildHeaders(accessToken),
        })
        if (res.status === 401) return { ok: false, status: 401, message: 'No autorizado.' }
        if (res.status === 404) return { ok: false, status: 404, message: 'El contacto no existe.' }
        if (!res.ok) {
            const data = await parseResponse(res)
            return { ok: false, status: res.status, message: data?.message ?? 'Error al obtener el contacto.' }
        }
        const data = await parseResponse(res)
        return { ok: true, status: 200, data }
    },

    /** POST /api/TerceroContactos */
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
            return { ok: false, status: res.status, message: data?.message ?? 'Error al crear el contacto.' }
        }
        const data = await parseResponse(res)
        return { ok: true, status: 201, data }
    },

    /** PUT /api/TerceroContactos/{id} */
    async actualizar(id, payload, accessToken) {
        const res = await fetch(`${BASE_URL}/${id}`, {
            method: 'PUT',
            headers: buildHeaders(accessToken),
            body: JSON.stringify(payload),
        })
        if (res.status === 401) return { ok: false, status: 401, message: 'No autorizado.' }
        if (res.status === 404) return { ok: false, status: 404, message: 'El contacto no existe.' }
        if (res.status === 400) {
            const data = await parseResponse(res)
            return { ok: false, status: 400, message: data?.message ?? 'Datos inválidos.' }
        }
        if (!res.ok) {
            const data = await parseResponse(res)
            return { ok: false, status: res.status, message: data?.message ?? 'Error al actualizar el contacto.' }
        }
        const data = await parseResponse(res)
        return { ok: true, status: 200, data }
    },

    /** DELETE /api/TerceroContactos/{id} */
    async eliminar(id, accessToken) {
        const res = await fetch(`${BASE_URL}/${id}`, {
            method: 'DELETE',
            headers: buildHeaders(accessToken),
        })
        if (res.status === 401) return { ok: false, status: 401, message: 'No autorizado.' }
        if (res.status === 404) return { ok: false, status: 404, message: 'El contacto no existe.' }
        if (res.status === 409) {
            const data = await parseResponse(res)
            return { ok: false, status: 409, message: data?.message ?? 'No se puede eliminar un registro con datos relacionados.' }
        }
        if (!res.ok) {
            const data = await parseResponse(res)
            return { ok: false, status: res.status, message: data?.message ?? 'Error al eliminar el contacto.' }
        }
        return { ok: true, status: 204 }
    },
}