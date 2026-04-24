import { useState, useEffect, useCallback, useRef } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { centroCostoService } from '../services/centroCostoService'

// Modal de confirmación de eliminación de distribución
function ModalEliminar({ distribucion, onConfirmar, onCancelar, eliminando }) {
    const modalRef = useRef(null)

    useEffect(() => {
        const $m = window.$(modalRef.current)
        if (distribucion) {
            $m.modal({ backdrop: 'static', keyboard: false })
            $m.modal('show')
        } else {
            $m.modal('hide')
        }
    }, [distribucion])

    return (
        <div className="modal fade" id="modalEliminarDist" tabIndex="-1" role="dialog" ref={modalRef}>
            <div className="modal-dialog modal-dialog-centered" role="document">
                <div className="modal-content">
                    <div className="modal-header bg-danger text-white">
                        <h5 className="modal-title">
                            <i className="fas fa-exclamation-triangle" aria-hidden="true" /> Confirmar Eliminación
                        </h5>
                        <button
                            type="button"
                            className="close text-white"
                            onClick={onCancelar}
                            disabled={eliminando}
                            aria-label="Cerrar"
                        >
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>¿Desea eliminar esta distribución?</p>
                        {distribucion && (
                            <p>
                                <strong>Centro de costo:</strong> {distribucion.codigo} - {distribucion.nombre}
                            </p>
                        )}
                    </div>
                    <div className="modal-footer">
                        <button
                            type="button"
                            className="btn btn-secondary"
                            onClick={onCancelar}
                            disabled={eliminando}
                        >
                            No
                        </button>
                        <button
                            type="button"
                            className="btn btn-danger"
                            onClick={onConfirmar}
                            disabled={eliminando}
                        >
                            {eliminando ? (
                                <>
                                    <span className="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true" />
                                    Eliminando...
                                </>
                            ) : 'Sí'}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    )
}

// Componente principal
export default function CentroCostoProrrateo() {
    const { detalleId } = useParams()
    const { user } = useAuth()
    const navigate = useNavigate()

    const [datos, setDatos] = useState(null)          // { detalle, distribuciones, centrosActivos, montoDistribuido, pendiente }
    const [cargando, setCargando] = useState(true)
    const [errorCarga, setErrorCarga] = useState('')

    // Formulario agregar
    const [centroCostoId, setCentroCostoId] = useState('')
    const [monto, setMonto] = useState('')
    const [guardando, setGuardando] = useState(false)
    const [errorForm, setErrorForm] = useState('')
    const [exitoForm, setExitoForm] = useState('')

    // Eliminar
    const [paraEliminar, setParaEliminar] = useState(null)
    const [eliminando, setEliminando] = useState(false)

    const cargarProrrateo = useCallback(async () => {
        setCargando(true)
        setErrorCarga('')
        try {
            const data = await centroCostoService.obtenerProrrateo(detalleId, user.accessToken)
            if (data.statusCode === 200) {
                setDatos(data.responseObject)
            } else {
                setErrorCarga(data.message ?? 'Error al cargar datos del prorrateo.')
            }
        } catch {
            setErrorCarga('No se pudo conectar con el servidor.')
        } finally {
            setCargando(false)
        }
    }, [detalleId, user.accessToken])

    useEffect(() => {
        cargarProrrateo()
    }, [cargarProrrateo])

    // Agregar distribución
    const handleAgregar = async (e) => {
        e.preventDefault()
        setErrorForm('')
        setExitoForm('')

        if (!centroCostoId) { setErrorForm('Debe seleccionar un centro de costo.'); return }
        if (!monto || Number(monto) <= 0) { setErrorForm('El monto debe ser mayor a cero.'); return }

        setGuardando(true)
        try {
            const data = await centroCostoService.agregarDistribucion(
                { DetalleId: Number(detalleId), CentroCostoId: Number(centroCostoId), Monto: Number(monto) },
                user.accessToken
            )
            if (data.statusCode === 200) {
                setExitoForm('Distribución registrada exitosamente.')
                setCentroCostoId('')
                setMonto('')
                await cargarProrrateo()
            } else {
                setErrorForm(data.message ?? 'Error al registrar la distribución.')
            }
        } catch {
            setErrorForm('No se pudo conectar con el servidor.')
        } finally {
            setGuardando(false)
        }
    }

    // Eliminar distribución
    const confirmarEliminar = async () => {
        if (!paraEliminar) return
        setEliminando(true)
        try {
            const data = await centroCostoService.eliminarDistribucion(
                { Id: paraEliminar.id, DetalleId: Number(detalleId) },
                user.accessToken
            )
            if (data.statusCode === 200) {
                setParaEliminar(null)
                setExitoForm('Distribución eliminada exitosamente.')
                await cargarProrrateo()
            } else {
                setParaEliminar(null)
                setErrorForm(data.message ?? 'Error al eliminar la distribución.')
            }
        } catch {
            setParaEliminar(null)
            setErrorForm('No se pudo conectar con el servidor.')
        } finally {
            setEliminando(false)
        }
    }

    const detalle = datos?.detalle
    const distribuciones = datos?.distribuciones ?? []
    const centrosActivos = datos?.centrosActivos ?? []
    const montoDistribuido = datos?.montoDistribuido ?? 0
    const pendiente = datos?.pendiente ?? 0
    const pendienteCero = Math.abs(pendiente) < 0.009

    return (
        <>
            {/* Encabezado */}
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800">
                    <i className="fas fa-sitemap text-primary" aria-hidden="true" /> Prorrateo por Centro de Costo
                </h1>
                <button
                    className="d-none d-sm-inline-block btn btn-sm btn-secondary shadow-sm"
                    onClick={() => navigate('/centro-costo')}
                >
                    <i className="fas fa-arrow-left fa-sm text-white-50" aria-hidden="true" /> Volver
                </button>
            </div>

            {/* Alertas de formulario */}
            {errorForm && (
                <div className="alert alert-danger alert-dismissible fade show" role="alert">
                    <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                    {errorForm}
                    <button type="button" className="close" onClick={() => setErrorForm('')} aria-label="Cerrar">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
            )}
            {exitoForm && (
                <div className="alert alert-success alert-dismissible fade show" role="alert">
                    <i className="fas fa-check-circle mr-2" aria-hidden="true" />
                    {exitoForm}
                    <button type="button" className="close" onClick={() => setExitoForm('')} aria-label="Cerrar">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
            )}

            {/* Cargando */}
            {cargando && (
                <div className="text-center py-4">
                    <span className="spinner-border text-primary" style={{ width: '2rem', height: '2rem' }} role="status">
                        <span className="sr-only">Cargando...</span>
                    </span>
                </div>
            )}

            {/* Error de carga */}
            {!cargando && errorCarga && (
                <div className="alert alert-danger" role="alert">
                    <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                    {errorCarga}
                </div>
            )}

            {/* Contenido principal */}
            {!cargando && !errorCarga && detalle && (
                <>
                    {/* Info del asiento */}
                    <div className="card shadow mb-4">
                        <div className="card-header py-3">
                            <h6 className="m-0 font-weight-bold text-primary">Información de la Línea de Asiento</h6>
                        </div>
                        <div className="card-body">
                            <div className="row">
                                <div className="col-md-6">
                                    <p><strong>Asiento ID:</strong> {detalle.asientoId}</p>
                                    <p><strong>Consecutivo:</strong> {detalle.consecutivo}</p>
                                    <p><strong>Fecha:</strong> {detalle.fechaAsiento}</p>
                                    <p><strong>Código asiento:</strong> {detalle.codigo}</p>
                                    <p><strong>Referencia:</strong> {detalle.referencia}</p>
                                </div>
                                <div className="col-md-6">
                                    <p><strong>Detalle ID:</strong> {detalle.detalleId}</p>
                                    <p><strong>Cuenta:</strong> {detalle.cuentaCodigo} - {detalle.cuentaNombre}</p>
                                    <p><strong>Tipo movimiento:</strong>{' '}
                                        {detalle.tipoMovimiento
                                            ? detalle.tipoMovimiento.charAt(0).toUpperCase() + detalle.tipoMovimiento.slice(1)
                                            : '—'}
                                    </p>
                                    <p><strong>Monto línea:</strong>{' '}
                                        ₡ {Number(detalle.monto).toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                                    </p>
                                    <p><strong>Estado asiento:</strong> {detalle.estadoNombre}</p>
                                    <p><strong>Período:</strong>{' '}
                                        {detalle.anio}-{String(detalle.mes).padStart(2, '0')}
                                    </p>
                                </div>
                            </div>
                            {detalle.descripcion && (
                                <>
                                    <hr />
                                    <p className="mb-0"><strong>Descripción:</strong> {detalle.descripcion}</p>
                                </>
                            )}
                        </div>
                    </div>

                    <div className="row">
                        {/* Formulario agregar */}
                        <div className="col-lg-5">
                            <div className="card shadow mb-4">
                                <div className="card-header py-3">
                                    <h6 className="m-0 font-weight-bold text-primary">Agregar Distribución</h6>
                                </div>
                                <div className="card-body">
                                    <form onSubmit={handleAgregar}>
                                        <div className="form-group">
                                            <label htmlFor="centroCostoId" className="font-weight-bold">
                                                Centro de costo <span className="text-danger">*</span>
                                            </label>
                                            <select
                                                id="centroCostoId"
                                                className="form-control"
                                                value={centroCostoId}
                                                onChange={(e) => setCentroCostoId(e.target.value)}
                                                required
                                            >
                                                <option value="">— Seleccione —</option>
                                                {centrosActivos.map((c) => (
                                                    <option key={c.centroCostoId} value={c.centroCostoId}>
                                                        {c.codigo} - {c.nombre}
                                                    </option>
                                                ))}
                                            </select>
                                        </div>
                                        <div className="form-group">
                                            <label htmlFor="monto" className="font-weight-bold">
                                                Monto <span className="text-danger">*</span>
                                            </label>
                                            <input
                                                type="number"
                                                step="0.01"
                                                min="0.01"
                                                id="monto"
                                                className="form-control"
                                                value={monto}
                                                onChange={(e) => setMonto(e.target.value)}
                                                required
                                            />
                                        </div>
                                        <div className="text-right">
                                            <button type="submit" className="btn btn-primary" disabled={guardando}>
                                                {guardando ? (
                                                    <>
                                                        <span className="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true" />
                                                        Guardando...
                                                    </>
                                                ) : (
                                                    <><i className="fas fa-plus" aria-hidden="true" /> Agregar Distribución</>
                                                )}
                                            </button>
                                        </div>
                                    </form>
                                </div>
                            </div>
                        </div>

                        {/* Tabla distribuciones registradas */}
                        <div className="col-lg-7">
                            <div className="card shadow mb-4">
                                <div className="card-header py-3 d-flex justify-content-between align-items-center">
                                    <h6 className="m-0 font-weight-bold text-primary">Distribuciones Registradas</h6>
                                    <span className="badge badge-info">
                                        Total distribuido: ₡ {Number(montoDistribuido).toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                                    </span>
                                </div>
                                <div className="card-body">
                                    <div className={`alert ${pendienteCero ? 'alert-success' : 'alert-warning'} mb-3`}>
                                        <strong>Pendiente por distribuir:</strong>{' '}
                                        ₡ {Number(pendiente).toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                                    </div>
                                    <div className="table-responsive">
                                        <table className="table table-bordered table-hover" width="100%" cellSpacing="0">
                                            <thead className="thead-light">
                                                <tr>
                                                    <th>Código</th>
                                                    <th>Centro de costo</th>
                                                    <th className="text-right">Monto</th>
                                                    <th className="text-center">Acción</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                {distribuciones.length === 0 ? (
                                                    <tr>
                                                        <td colSpan={4} className="text-center text-muted">
                                                            No hay distribuciones registradas.
                                                        </td>
                                                    </tr>
                                                ) : (
                                                    distribuciones.map((d) => (
                                                        <tr key={d.id}>
                                                            <td>{d.codigo}</td>
                                                            <td>{d.nombre}</td>
                                                            <td className="text-right">
                                                                ₡ {Number(d.monto).toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                                                            </td>
                                                            <td className="text-center">
                                                                <button
                                                                    className="btn btn-sm btn-danger"
                                                                    title="Eliminar distribución"
                                                                    onClick={() => setParaEliminar(d)}
                                                                >
                                                                    <i className="fas fa-trash" aria-hidden="true" />
                                                                </button>
                                                            </td>
                                                        </tr>
                                                    ))
                                                )}
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </>
            )}

            {/* Modal eliminar distribución */}
            <ModalEliminar
                distribucion={paraEliminar}
                onConfirmar={confirmarEliminar}
                onCancelar={() => setParaEliminar(null)}
                eliminando={eliminando}
            />
        </>
    )
}