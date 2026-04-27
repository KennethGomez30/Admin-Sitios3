import { useState, useEffect, useRef } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { direccionService } from '../services/direccionService'
import '../styles/terceros.css'

const ESTADOS_VALIDOS = ['Activa', 'Inactiva']

// Modal de error
function ModalError({ mensaje, onCerrar }) {
    const modalRef = useRef(null)

    useEffect(() => {
        const $m = window.$(modalRef.current)
        if (mensaje) {
            $m.modal({ backdrop: true, keyboard: true })
            $m.modal('show')
            $m.on('hidden.bs.modal', onCerrar)
        } else {
            $m.modal('hide')
        }
        return () => $m.off('hidden.bs.modal', onCerrar)
    }, [mensaje, onCerrar])

    return (
        <div className="modal fade" id="modalMensaje" tabIndex="-1" role="dialog" ref={modalRef}>
            <div className="modal-dialog modal-dialog-centered" role="document">
                <div className="modal-content">
                    <div className="modal-header bg-danger text-white">
                        <h5 className="modal-title">
                            <i className="fas fa-exclamation-triangle" aria-hidden="true" /> Error
                        </h5>
                        <button
                            type="button"
                            className="close text-white"
                            onClick={onCerrar}
                            aria-label="Cerrar"
                        >
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">{mensaje}</div>
                    <div className="modal-footer">
                        <button
                            type="button"
                            className="btn btn-danger"
                            onClick={onCerrar}
                        >
                            Aceptar
                        </button>
                    </div>
                </div>
            </div>
        </div>
    )
}

// Validaciones
function validar(datos) {
    const errores = []

    if (!datos.alias.trim()) {
        errores.push('El alias es obligatorio.')
    } else if (datos.alias.trim().length > 100) {
        errores.push('El alias no puede superar 100 caracteres.')
    }

    if (datos.ubicacion && datos.ubicacion.length > 255) {
        errores.push('La ubicación no puede superar 255 caracteres.')
    }

    if (!datos.direccionExacta.trim()) {
        errores.push('La dirección exacta es obligatoria.')
    }

    if (!ESTADOS_VALIDOS.includes(datos.estado)) {
        errores.push('El estado seleccionado no es válido.')
    }

    return errores
}

// Componente principal
export default function DireccionEditar() {
    const { terceroId, id } = useParams()
    const { user } = useAuth()
    const navigate = useNavigate()

    const [datos, setDatos] = useState({
        alias: '',
        ubicacion: '',
        direccionExacta: '',
        esPrincipal: false,
        estado: 'Activa',
    })
    const [cargando, setCargando] = useState(true)
    const [errorCarga, setErrorCarga] = useState('')
    const [mensajeError, setMensajeError] = useState('')
    const [guardando, setGuardando] = useState(false)

    // Cargar al montar
    useEffect(() => {
        async function cargar() {
            setCargando(true)
            setErrorCarga('')
            try {
                const result = await direccionService.obtenerPorId(id, user.accessToken)

                if (result.ok && result.data) {
                    const d = result.data
                    setDatos({
                        alias: d.alias ?? '',
                        ubicacion: d.ubicacion ?? '',
                        direccionExacta: d.direccionExacta ?? '',
                        esPrincipal: d.esPrincipal ?? false,
                        estado: d.estado ?? 'Activa',
                    })
                } else if (result.status === 404) {
                    navigate(`/terceros/${terceroId}/direcciones`)
                } else {
                    setErrorCarga(result.message ?? 'Error al cargar la dirección.')
                }
            } catch {
                setErrorCarga('No se pudo conectar con el servidor.')
            } finally {
                setCargando(false)
            }
        }

        cargar()
    }, [id, terceroId, user.accessToken, navigate])

    const cambiar = (e) => {
        const { name, value, type, checked } = e.target
        setDatos((prev) => ({
            ...prev,
            [name]: type === 'checkbox' ? checked : value,
        }))
    }

    const handleSubmit = async (e) => {
        e.preventDefault()

        const errores = validar(datos)
        if (errores.length > 0) {
            setMensajeError(errores.join(' '))
            return
        }

        setGuardando(true)
        try {
            const payload = {
                Id: Number(id),
                Alias: datos.alias.trim(),
                Ubicacion: datos.ubicacion.trim() || null,
                DireccionExacta: datos.direccionExacta.trim(),
                EsPrincipal: datos.esPrincipal,
                Estado: datos.estado,
            }

            const result = await direccionService.actualizar(id, payload, user.accessToken)

            if (result.ok) {
                sessionStorage.setItem('alerta_tipo', 'success')
                sessionStorage.setItem('alerta_msg', 'Dirección actualizada exitosamente.')
                navigate(`/terceros/${terceroId}/direcciones`)
            } else if (result.status === 404) {
                navigate(`/terceros/${terceroId}/direcciones`)
            } else {
                setMensajeError(result.message)
            }
        } catch {
            setMensajeError('Error de conexión al actualizar la dirección. Intente nuevamente.')
        } finally {
            setGuardando(false)
        }
    }

    // Validar id y terceroId — redirigir antes de renderizar
    const idNum = Number(id)
    const terceroIdNum = Number(terceroId)
    if (!id || isNaN(idNum) || idNum <= 0 ||
        !terceroId || isNaN(terceroIdNum) || terceroIdNum <= 0) {
        navigate('/terceros', { replace: true })
        return null
    }

    if (cargando) {
        return (
            <div className="text-center py-5">
                <span
                    className="spinner-border text-primary"
                    style={{ width: '2.5rem', height: '2.5rem' }}
                    role="status"
                >
                    <span className="sr-only">Cargando...</span>
                </span>
            </div>
        )
    }

    if (errorCarga) {
        return (
            <div className="alert alert-danger" role="alert">
                <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                {errorCarga}
                <button
                    className="btn btn-sm btn-link ml-2"
                    onClick={() => navigate(`/terceros/${terceroId}/direcciones`)}
                >
                    Volver al listado
                </button>
            </div>
        )
    }

    return (
        <>
            {/* Encabezado */}
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800 page-header-title">
                    <i className="fas fa-map-marker-alt text-primary" aria-hidden="true" /> Editar Dirección
                </h1>
                <button
                    className="d-none d-sm-inline-block btn btn-sm btn-secondary shadow-sm"
                    onClick={() => navigate(`/terceros/${terceroId}/direcciones`)}
                >
                    <i className="fas fa-arrow-left fa-sm text-white-50" aria-hidden="true" /> Volver al Listado
                </button>
            </div>

            {/* Formulario */}
            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Información de la Dirección</h6>
                </div>
                <div className="card-body">
                    <form onSubmit={handleSubmit} noValidate>

                        <div className="row">
                            <div className="col-md-6">
                                <div className="form-group">
                                    <label htmlFor="alias" className="font-weight-bold">
                                        Alias <span className="text-danger">*</span>
                                    </label>
                                    <input
                                        type="text"
                                        className="form-control"
                                        id="alias"
                                        name="alias"
                                        maxLength={100}
                                        required
                                        value={datos.alias}
                                        onChange={cambiar}
                                        disabled={guardando}
                                    />
                                </div>
                            </div>
                            <div className="col-md-6">
                                <div className="form-group">
                                    <label htmlFor="estado" className="font-weight-bold">
                                        Estado <span className="text-danger">*</span>
                                    </label>
                                    <select
                                        className="form-control"
                                        id="estado"
                                        name="estado"
                                        required
                                        value={datos.estado}
                                        onChange={cambiar}
                                        disabled={guardando}
                                    >
                                        {ESTADOS_VALIDOS.map((op) => (
                                            <option key={op} value={op}>{op}</option>
                                        ))}
                                    </select>
                                </div>
                            </div>
                        </div>

                        <div className="row">
                            <div className="col-md-12">
                                <div className="form-group">
                                    <label htmlFor="ubicacion" className="font-weight-bold">
                                        Ubicación
                                    </label>
                                    <input
                                        type="text"
                                        className="form-control"
                                        id="ubicacion"
                                        name="ubicacion"
                                        maxLength={255}
                                        value={datos.ubicacion}
                                        onChange={cambiar}
                                        disabled={guardando}
                                    />
                                </div>
                            </div>
                        </div>

                        <div className="row">
                            <div className="col-md-12">
                                <div className="form-group">
                                    <label htmlFor="direccionExacta" className="font-weight-bold">
                                        Dirección Exacta <span className="text-danger">*</span>
                                    </label>
                                    <textarea
                                        className="form-control"
                                        id="direccionExacta"
                                        name="direccionExacta"
                                        rows={3}
                                        required
                                        value={datos.direccionExacta}
                                        onChange={cambiar}
                                        disabled={guardando}
                                    />
                                </div>
                            </div>
                        </div>

                        <div className="row">
                            <div className="col-md-12">
                                <div className="form-group">
                                    <div className="custom-control custom-checkbox">
                                        <input
                                            type="checkbox"
                                            className="custom-control-input"
                                            id="esPrincipal"
                                            name="esPrincipal"
                                            checked={datos.esPrincipal}
                                            onChange={cambiar}
                                            disabled={guardando}
                                        />
                                        <label className="custom-control-label" htmlFor="esPrincipal">
                                            <strong>Marcar como dirección principal</strong>
                                            <small className="text-muted d-block">
                                                Si ya existía una principal, se desmarcará automáticamente.
                                            </small>
                                        </label>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <hr className="my-4" />

                        <div className="form-group text-right mb-0 form-actions">
                            <button
                                type="button"
                                className="btn btn-secondary"
                                onClick={() => navigate(`/terceros/${terceroId}/direcciones`)}
                                disabled={guardando}
                            >
                                <i className="fas fa-times" aria-hidden="true" /> Cancelar
                            </button>
                            <button
                                type="submit"
                                className="btn btn-primary"
                                disabled={guardando}
                            >
                                {guardando ? (
                                    <>
                                        <span
                                            className="spinner-border spinner-border-sm mr-2"
                                            role="status"
                                            aria-hidden="true"
                                        />
                                        Guardando...
                                    </>
                                ) : (
                                    <>
                                        <i className="fas fa-save" aria-hidden="true" /> Guardar Cambios
                                    </>
                                )}
                            </button>
                        </div>

                    </form>
                </div>
            </div>

            <ModalError
                mensaje={mensajeError}
                onCerrar={() => setMensajeError('')}
            />
        </>
    )
}