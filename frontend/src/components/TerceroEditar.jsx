import { useState, useEffect, useRef } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { terceroService } from '../services/terceroService'
import '../styles/terceros.css'

const TIPOS_VALIDOS = ['Cliente', 'Proveedor', 'Empleado', 'Otro']
const ESTADOS_VALIDOS = ['Activo', 'Inactivo']

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

// Validación sin identificación, que es readonly
function validar(datos) {
    const errores = []

    if (!datos.nombreRazonSocial.trim()) {
        errores.push('El nombre / razón social es obligatorio.')
    } else if (datos.nombreRazonSocial.trim().length > 200) {
        errores.push('El nombre / razón social no puede superar 200 caracteres.')
    }

    if (!TIPOS_VALIDOS.includes(datos.tipo)) {
        errores.push('El tipo seleccionado no es válido.')
    }

    if (datos.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(datos.email)) {
        errores.push('El formato del correo electrónico no es válido.')
    }

    if (datos.telefono && !/^[\d\s+\-()]{7,20}$/.test(datos.telefono)) {
        errores.push('El formato del teléfono no es válido.')
    }

    if (!ESTADOS_VALIDOS.includes(datos.estado)) {
        errores.push('El estado seleccionado no es válido.')
    }

    return errores
}

//  Componente principal
export default function TerceroEditar() {
    const { id } = useParams()
    const { user } = useAuth()
    const navigate = useNavigate()

    const [identificacion, setIdentificacion] = useState('')  // solo lectura
    const [datos, setDatos] = useState({
        nombreRazonSocial: '',
        tipo: '',
        email: '',
        telefono: '',
        estado: 'Activo',
    })
    const [cargando, setCargando] = useState(true)
    const [errorCarga, setErrorCarga] = useState('')
    const [mensajeError, setMensajeError] = useState('')
    const [guardando, setGuardando] = useState(false)

    // Cargar tercero al montar
    useEffect(() => {
        if (!id || isNaN(Number(id)) || Number(id) <= 0) {
            navigate('/terceros')
            return
        }

        async function cargar() {
            setCargando(true)
            setErrorCarga('')
            try {
                const data = await terceroService.obtenerPorId(id, user.accessToken)
                if (data.statusCode === 200 && data.responseObject) {
                    const t = data.responseObject
                    setIdentificacion(t.identificacion ?? '')
                    setDatos({
                        nombreRazonSocial: t.nombreRazonSocial ?? '',
                        tipo: t.tipo ?? '',
                        email: t.email ?? '',
                        telefono: t.telefono ?? '',
                        estado: t.estado ?? 'Activo',
                    })
                } else if (data.statusCode === 404) {
                    navigate('/terceros')
                } else {
                    setErrorCarga(data.message ?? 'Error al cargar el tercero.')
                }
            } catch {
                setErrorCarga('No se pudo conectar con el servidor.')
            } finally {
                setCargando(false)
            }
        }

        cargar()
    }, [id, user.accessToken, navigate])

    const cambiar = (e) => {
        const { name, value } = e.target
        setDatos((prev) => ({ ...prev, [name]: value }))
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
                NombreRazonSocial: datos.nombreRazonSocial.trim(),
                Tipo: datos.tipo,
                Email: datos.email.trim() || null,
                Telefono: datos.telefono.trim() || null,
                Estado: datos.estado,
            }

            const data = await terceroService.actualizar(id, payload, user.accessToken)

            if (data.statusCode === 200) {
                sessionStorage.setItem('alerta_tipo', 'success')
                sessionStorage.setItem('alerta_msg', 'Tercero actualizado exitosamente.')
                navigate('/terceros')
            } else if (data.statusCode === 404) {
                navigate('/terceros')
            } else if (data.statusCode === 400) {
                setMensajeError(data.message ?? 'Datos inválidos.')
            } else {
                setMensajeError(data.message ?? 'Error al actualizar el tercero. Intente nuevamente.')
            }
        } catch {
            setMensajeError('Error de conexión al actualizar el tercero. Intente nuevamente.')
        } finally {
            setGuardando(false)
        }
    }

    // Estados de carga / error
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
                    onClick={() => navigate('/terceros')}
                >
                    Volver al listado
                </button>
            </div>
        )
    }

    // Render principal
    return (
        <>
            {/* Encabezado de página */}
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800 page-header-title">
                    <i className="fas fa-user-edit text-primary" aria-hidden="true" /> Editar Tercero
                </h1>
                <button
                    className="d-none d-sm-inline-block btn btn-sm btn-secondary shadow-sm"
                    onClick={() => navigate('/terceros')}
                >
                    <i className="fas fa-arrow-left fa-sm text-white-50" aria-hidden="true" /> Volver al Listado
                </button>
            </div>

            {/* Formulario */}
            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Información del Tercero</h6>
                </div>
                <div className="card-body">

                    {/* Identificación — solo lectura, idéntico al alert-light de PHP */}
                    <div className="alert-identificacion mb-3">
                        <i className="fas fa-info-circle text-primary" aria-hidden="true" />
                        <strong> Identificación:</strong> {identificacion}
                    </div>

                    <form onSubmit={handleSubmit} noValidate>

                        <div className="row">
                            <div className="col-md-6">
                                <div className="form-group">
                                    <label htmlFor="tipo" className="font-weight-bold">
                                        Tipo <span className="text-danger">*</span>
                                    </label>
                                    <select
                                        className="form-control"
                                        id="tipo"
                                        name="tipo"
                                        required
                                        value={datos.tipo}
                                        onChange={cambiar}
                                        disabled={guardando}
                                    >
                                        <option value="">— Seleccione —</option>
                                        {TIPOS_VALIDOS.map((op) => (
                                            <option key={op} value={op}>{op}</option>
                                        ))}
                                    </select>
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
                                    <label htmlFor="nombreRazonSocial" className="font-weight-bold">
                                        Nombre / Razón Social <span className="text-danger">*</span>
                                    </label>
                                    <input
                                        type="text"
                                        className="form-control"
                                        id="nombreRazonSocial"
                                        name="nombreRazonSocial"
                                        maxLength={200}
                                        required
                                        value={datos.nombreRazonSocial}
                                        onChange={cambiar}
                                        disabled={guardando}
                                    />
                                </div>
                            </div>
                        </div>

                        <div className="row">
                            <div className="col-md-6">
                                <div className="form-group">
                                    <label htmlFor="email" className="font-weight-bold">
                                        Correo Electrónico
                                    </label>
                                    <input
                                        type="email"
                                        className="form-control"
                                        id="email"
                                        name="email"
                                        maxLength={100}
                                        value={datos.email}
                                        onChange={cambiar}
                                        disabled={guardando}
                                    />
                                </div>
                            </div>
                            <div className="col-md-6">
                                <div className="form-group">
                                    <label htmlFor="telefono" className="font-weight-bold">
                                        Teléfono
                                    </label>
                                    <input
                                        type="text"
                                        className="form-control"
                                        id="telefono"
                                        name="telefono"
                                        maxLength={20}
                                        value={datos.telefono}
                                        onChange={cambiar}
                                        disabled={guardando}
                                    />
                                </div>
                            </div>
                        </div>

                        <hr className="my-4" />

                        <div className="form-group text-right mb-0 form-actions">
                            <button
                                type="button"
                                className="btn btn-secondary"
                                onClick={() => navigate('/terceros')}
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

            {/* Modal de error*/}
            <ModalError
                mensaje={mensajeError}
                onCerrar={() => setMensajeError('')}
            />
        </>
    )
}