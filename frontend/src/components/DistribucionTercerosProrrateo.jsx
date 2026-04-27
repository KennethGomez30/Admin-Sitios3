import { useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useMensajeFlash } from '../hooks/useMensajeFlash'
import { tercerosService } from '../services/DistribucionTercerosService'

function BadgeEstado({ codigo, nombre }) {
    const upper = (codigo ?? '').toUpperCase().trim()
    const clase =
        upper === 'EA2' ? 'badge-success'
        : upper === 'EA3' ? 'badge-secondary'
        : upper === 'EA4' ? 'badge-warning'
        : upper === 'EA5' ? 'badge-danger'
        : 'badge-dark'
    return <span className={`badge ${clase}`}>{nombre}</span>
}

function monedaCR(valor) {
    return Number(valor ?? 0).toLocaleString('es-CR', { minimumFractionDigits: 2 })
}

export default function DistribucionTercerosProrrateo() {
    const { detalleId } = useParams()
    const navigate = useNavigate()
    const { user } = useAuth()
    const { alerta, cerrar: cerrarAlerta } = useMensajeFlash()

    const [detalle, setDetalle] = useState(null)
    const [tercerosActivos, setTercerosActivos] = useState([])
    const [distribuciones, setDistribuciones] = useState([])
    const [cargando, setCargando] = useState(true)
    const [errorCarga, setErrorCarga] = useState('')

    const [terceroId, setTerceroId] = useState(0)
    const [monto, setMonto] = useState('')

    const montoDistribuido = useMemo(
        () => distribuciones.reduce((acc, d) => acc + Number(d.monto ?? 0), 0),
        [distribuciones]
    )

    const pendiente = useMemo(() => {
        const total = Number(detalle?.monto ?? 0)
        return total - montoDistribuido
    }, [detalle, montoDistribuido])

    const puedeModificar = useMemo(() => {
        const estado = (detalle?.estadoCodigo ?? '').toUpperCase().trim()
        return estado === 'EA3' || estado === 'EA4'
    }, [detalle])

    const estaBalanceado = useMemo(() => {
        return Math.abs(pendiente) < 0.009
    }, [pendiente])

    const puedeAgregar = puedeModificar && !estaBalanceado

    const validarFormulario = () => {
        const errores = []
        const montoNumero = Number(monto)

        if (!terceroId || Number(terceroId) <= 0) {
            errores.push('Debe seleccionar un tercero.')
        }

        if (!monto || Number.isNaN(montoNumero) || montoNumero <= 0) {
            errores.push('El monto debe ser mayor a cero.')
        }

        if (estaBalanceado) {
            errores.push('La línea ya está balanceada. Para agregar otra distribución, primero elimine una existente.')
        }

        if (montoNumero > 0 && montoNumero - pendiente > 0.009) {
            errores.push('La distribución excede el monto pendiente de la línea.')
        }

        return errores
    }

    const cargarDatos = async () => {
        setCargando(true)
        setErrorCarga('')
        try {
            const data = await tercerosService.obtenerProrrateo(Number(detalleId), user.accessToken)

            if (data.statusCode === 200) {
                const info = data.responseObject ?? {}
                setDetalle(info.detalle ?? null)
                setTercerosActivos(info.tercerosActivos ?? [])
                setDistribuciones(info.distribuciones ?? [])
                setMonto('')
                setTerceroId(0)
            } else {
                setErrorCarga(data.message ?? 'No se pudo cargar el detalle.')
            }
        } catch {
            setErrorCarga('No se pudo conectar con el servidor.')
        } finally {
            setCargando(false)
        }
    }

    useEffect(() => {
        if (detalleId) cargarDatos()
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [detalleId])

    const handleAgregar = async (e) => {
        e.preventDefault()
        setErrorCarga('')

        const errores = validarFormulario()
        if (errores.length > 0) {
            setErrorCarga(errores.join(' '))
            return
        }

        try {
            const payload = {
                detalleId: Number(detalleId),
                terceroId: Number(terceroId),
                monto: Number(monto),
            }

            const data = await tercerosService.agregarDistribucion(payload, user.accessToken)

            if (data.statusCode === 200) {
                await cargarDatos()
            } else {
                setErrorCarga(data.message ?? 'No fue posible registrar la distribución.')
            }
        } catch (err) {
            setErrorCarga(err.message || 'No se pudo conectar con el servidor.')
        }
    }

    const handleEliminar = async (id) => {
        try {
            const data = await tercerosService.eliminarDistribucion(id, Number(detalleId), user.accessToken)

            if (data.statusCode === 200) {
                await cargarDatos()
            } else {
                setErrorCarga(data.message ?? 'No fue posible eliminar la distribución.')
            }
        } catch (err) {
            setErrorCarga(err.message || 'No se pudo conectar con el servidor.')
        }
    }

    if (cargando) {
        return (
            <div className="text-center py-5">
                <span className="spinner-border text-primary" style={{ width: '2.5rem', height: '2.5rem' }} role="status">
                    <span className="sr-only">Cargando...</span>
                </span>
            </div>
        )
    }

    if (errorCarga && !detalle) {
        return (
            <>
                <div className="d-sm-flex align-items-center justify-content-between mb-4">
                    <h1 className="h3 mb-0 text-gray-800">
                        <i className="fas fa-sitemap text-primary" aria-hidden="true" /> Prorrateo por Tercero
                    </h1>
                </div>

                <div className="alert alert-danger" role="alert">
                    <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                    {errorCarga}
                    <button className="btn btn-sm btn-link ml-2" onClick={cargarDatos}>
                        Reintentar
                    </button>
                </div>
            </>
        )
    }

    return (
        <>
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800">
                    <i className="fas fa-sitemap text-primary" aria-hidden="true" /> Prorrateo por Tercero
                </h1>
                <button
                    type="button"
                    className="btn btn-sm btn-secondary shadow-sm"
                    onClick={() => navigate(`/distribucion-terceros`)}
                >
                    <i className="fas fa-arrow-left fa-sm text-white-50" aria-hidden="true" /> Volver
                </button>
            </div>

            {alerta && (
                <div className={`alert alert-${alerta.tipo} alert-dismissible fade show`} role="alert">
                    <i
                        className={`fas ${alerta.tipo === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle'} mr-2`}
                        aria-hidden="true"
                    />
                    {alerta.msg}
                    <button type="button" className="close" onClick={cerrarAlerta} aria-label="Cerrar">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
            )}

            {errorCarga && (
                <div className="alert alert-danger" role="alert">
                    <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                    {errorCarga}
                    <button className="btn btn-sm btn-link ml-2" onClick={cargarDatos}>
                        Reintentar
                    </button>
                </div>
            )}

            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Información de la Línea de Asiento</h6>
                </div>
                <div className="card-body">
                    <div className="row">
                        <div className="col-md-6">
                            <p><strong>Asiento ID:</strong> {detalle?.asientoId}</p>
                            <p><strong>Consecutivo:</strong> {detalle?.consecutivo}</p>
                            <p><strong>Fecha:</strong> {detalle?.fechaAsiento ? String(detalle.fechaAsiento).slice(0, 10) : '—'}</p>
                            <p><strong>Código asiento:</strong> {detalle?.codigo}</p>
                            <p><strong>Referencia:</strong> {detalle?.referencia}</p>
                        </div>
                        <div className="col-md-6">
                            <p><strong>Cuenta:</strong> {detalle?.cuentaCodigo} - {detalle?.cuentaNombre}</p>
                            <p><strong>Tipo movimiento:</strong> {detalle?.tipoMovimiento}</p>
                            <p><strong>Estado:</strong> <BadgeEstado codigo={detalle?.estadoCodigo} nombre={detalle?.estadoNombre} /></p>
                            <p><strong>Monto línea:</strong> ₡ {monedaCR(detalle?.monto)}</p>
                            <p><strong>Saldo pendiente:</strong> ₡ {monedaCR(pendiente)}</p>
                        </div>
                    </div>
                </div>
            </div>

            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Agregar distribución</h6>
                </div>
                <div className="card-body">
                    {puedeAgregar ? (
                        <form onSubmit={handleAgregar}>
                            <div className="form-row">
                                <div className="form-group col-md-6">
                                    <label htmlFor="tercero_id" className="font-weight-bold">Tercero</label>
                                    <select
                                        id="tercero_id"
                                        className="form-control"
                                        value={terceroId}
                                        onChange={(e) => setTerceroId(Number(e.target.value))}
                                    >
                                        <option value={0}>Seleccione un tercero</option>
                                        {tercerosActivos.map((t) => (
                                            <option key={t.id} value={t.id}>
                                                {t.nombreRazonSocial} - {t.identificacion}
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                <div className="form-group col-md-4">
                                    <label htmlFor="monto" className="font-weight-bold">Monto</label>
                                    <input
                                        id="monto"
                                        type="number"
                                        step="0.01"
                                        min="0.01"
                                        max={pendiente > 0 ? pendiente : 0}
                                        className="form-control"
                                        value={monto}
                                        onChange={(e) => setMonto(e.target.value)}
                                    />
                                </div>

                                <div className="form-group col-md-2 d-flex align-items-end">
                                    <button type="submit" className="btn btn-primary btn-block" disabled={!puedeAgregar}>
                                        <i className="fas fa-plus" aria-hidden="true" /> Agregar
                                    </button>
                                </div>
                            </div>
                        </form>
                    ) : (
                        <div className="alert alert-warning mb-0">
                            {!puedeModificar
                                ? 'Este asiento no permite modificar distribuciones.'
                                : 'La línea ya está balanceada. Para agregar otra distribución, elimine una existente primero.'}
                        </div>
                    )}
                </div>
            </div>

            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Distribuciones registradas</h6>
                </div>
                <div className="card-body">
                    <div className="table-responsive">
                        <table className="table table-bordered table-hover" width="100%" cellSpacing="0">
                            <thead className="thead-light">
                                <tr>
                                    <th>ID</th>
                                    <th>Tercero</th>
                                    <th>Identificación</th>
                                    <th className="text-right">Monto</th>
                                    <th className="text-center">Acción</th>
                                </tr>
                            </thead>
                            <tbody>
                                {distribuciones.length === 0 ? (
                                    <tr>
                                        <td colSpan={5} className="text-center text-muted">
                                            No hay distribuciones registradas.
                                        </td>
                                    </tr>
                                ) : (
                                    distribuciones.map((d) => (
                                        <tr key={d.id}>
                                            <td className="align-middle">{d.id}</td>
                                            <td className="align-middle">{d.nombre}</td>
                                            <td className="align-middle">{d.identificacion}</td>
                                            <td className="align-middle text-right">₡ {monedaCR(d.monto)}</td>
                                            <td className="align-middle text-center">
                                                {puedeModificar ? (
                                                    <button
                                                        type="button"
                                                        className="btn btn-sm btn-danger"
                                                        onClick={() => handleEliminar(d.id)}
                                                    >
                                                        <i className="fas fa-trash" aria-hidden="true" /> Eliminar
                                                    </button>
                                                ) : (
                                                    <span className="badge badge-light border text-muted">No permitido</span>
                                                )}
                                            </td>
                                        </tr>
                                    ))
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </>
    )
}