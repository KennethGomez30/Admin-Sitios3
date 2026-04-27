import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
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

function FilaLinea({ linea, onDistribuir }) {
    const saldo = (linea.monto ?? 0) - (linea.montoTerceros ?? 0)
    const saldoCero = Math.abs(saldo) < 0.009
    const estadoCodigo = (linea.estadoCodigo ?? '').toUpperCase().trim()
    const puedeDistribuir = estadoCodigo === 'EA3' || estadoCodigo === 'EA4'

    return (
        <tr>
            <td className="align-middle">{linea.asientoId}</td>
            <td className="align-middle">{linea.detalleId}</td>
            <td className="align-middle">
                {linea.cuentaCodigo} - {linea.cuentaNombre}
            </td>
            <td className="align-middle">
                {linea.tipoMovimiento
                    ? linea.tipoMovimiento.charAt(0).toUpperCase() + linea.tipoMovimiento.slice(1)
                    : '—'}
            </td>
            <td className="align-middle">
                <BadgeEstado codigo={linea.estadoCodigo} nombre={linea.estadoNombre} />
            </td>
            <td className="align-middle text-right">
                ₡ {Number(linea.monto ?? 0).toLocaleString('es-CR', { minimumFractionDigits: 2 })}
            </td>
            <td className="align-middle text-right">
                ₡ {Number(linea.montoTerceros ?? 0).toLocaleString('es-CR', { minimumFractionDigits: 2 })}
            </td>
            <td className="align-middle text-right">
                <span className={`badge ${saldoCero ? 'badge-success' : 'badge-warning'}`}>
                    ₡ {Number(saldo).toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                </span>
            </td>
            <td className="align-middle text-center">{linea.cantidadTerceros}</td>
            <td className="align-middle text-center">
                {puedeDistribuir ? (
                    <button
                        className="btn btn-sm btn-primary"
                        onClick={() => onDistribuir(linea.detalleId)}
                        title="Distribuir por tercero"
                    >
                        <i className="fas fa-user-plus" aria-hidden="true" /> Asignar
                    </button>
                ) : (
                    <span className="badge badge-light border text-muted">No permitido</span>
                )}
            </td>
        </tr>
    )
}

export default function DistribucionTercerosListado() {
    const { user } = useAuth()
    const navigate = useNavigate()
    const { alerta, cerrar: cerrarAlerta } = useMensajeFlash()

    const [periodos, setPeriodos] = useState([])
    const [periodoId, setPeriodoId] = useState(0)
    const [lineas, setLineas] = useState([])
    const [cargando, setCargando] = useState(true)
    const [errorCarga, setErrorCarga] = useState('')

    useEffect(() => {
        async function cargarPeriodos() {
            try {
                const data = await tercerosService.listarPeriodos(user.accessToken)
                if (data.statusCode === 200) {
                    const lista = data.responseObject ?? []
                    setPeriodos(lista)

                    const activo = lista.find((p) => Number(p.activo) === 1)
                    const seleccionado = activo ? activo.periodoId : lista[0]?.periodoId ?? 0
                    setPeriodoId(seleccionado)
                } else {
                    setErrorCarga(data.message ?? 'No se pudieron cargar los períodos.')
                    setCargando(false)
                }
            } catch {
                setErrorCarga('No se pudo conectar con el servidor.')
                setCargando(false)
            }
        }

        cargarPeriodos()
    }, [user.accessToken])

    const cargarLineas = useCallback(async (id) => {
        if (!id) return
        setCargando(true)
        setErrorCarga('')
        try {
            const data = await tercerosService.listarLineas(id, user.accessToken)
            if (data.statusCode === 200) {
                setLineas(data.responseObject ?? [])
            } else {
                setErrorCarga(data.message ?? 'Error al cargar líneas de asiento.')
            }
        } catch {
            setErrorCarga('No se pudo conectar con el servidor.')
        } finally {
            setCargando(false)
        }
    }, [user.accessToken])

    useEffect(() => {
        if (periodoId) cargarLineas(periodoId)
    }, [periodoId, cargarLineas])

    const handleConsultar = (e) => {
        e.preventDefault()
        cargarLineas(periodoId)
    }

    return (
        <>
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800">
                    <i className="fas fa-sitemap text-primary" aria-hidden="true" /> Distribución por Terceros
                </h1>
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

            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Filtro de Período</h6>
                </div>
                <div className="card-body">
                    <form className="form-inline" onSubmit={handleConsultar}>
                        <label htmlFor="periodo_id" className="mr-2 font-weight-bold">
                            Período contable:
                        </label>
                        <select
                            id="periodo_id"
                            className="form-control mr-2"
                            value={periodoId}
                            onChange={(e) => setPeriodoId(Number(e.target.value))}
                        >
                            {periodos.map((p) => (
                                <option key={p.periodoId} value={p.periodoId}>
                                    {p.anio} - {String(p.mes).padStart(2, '0')} ({p.estado})
                                </option>
                            ))}
                        </select>
                        <button type="submit" className="btn btn-primary">
                            <i className="fas fa-search" aria-hidden="true" /> Consultar
                        </button>
                    </form>
                </div>
            </div>

            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Líneas de Asiento del Período</h6>
                </div>
                <div className="card-body">
                    {cargando && (
                        <div className="text-center py-4">
                            <span
                                className="spinner-border text-primary"
                                style={{ width: '2rem', height: '2rem' }}
                                role="status"
                            >
                                <span className="sr-only">Cargando...</span>
                            </span>
                        </div>
                    )}

                    {!cargando && errorCarga && (
                        <div className="alert alert-danger" role="alert">
                            <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                            {errorCarga}
                            <button className="btn btn-sm btn-link ml-2" onClick={() => cargarLineas(periodoId)}>
                                Reintentar
                            </button>
                        </div>
                    )}

                    {!cargando && !errorCarga && (
                        <div className="table-responsive">
                            <table className="table table-bordered table-hover" width="100%" cellSpacing="0">
                                <thead className="thead-light">
                                    <tr>
                                        <th>Asiento</th>
                                        <th>Detalle</th>
                                        <th>Cuenta</th>
                                        <th>Tipo Movimiento</th>
                                        <th>Estado Asiento</th>
                                        <th className="text-right">Monto Línea</th>
                                        <th className="text-right">Monto Distribuido</th>
                                        <th className="text-right">Pendiente</th>
                                        <th className="text-center">Terceros</th>
                                        <th className="text-center">Acción</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {lineas.length === 0 ? (
                                        <tr>
                                            <td colSpan={10} className="text-center text-muted">
                                                <i className="fas fa-inbox fa-2x" aria-hidden="true" />
                                                <p className="mb-0">No hay líneas de asiento para el período seleccionado.</p>
                                            </td>
                                        </tr>
                                    ) : (
                                        lineas.map((linea) => (
                                            <FilaLinea
                                                key={linea.detalleId}
                                                linea={linea}
                                                onDistribuir={(detalleId) =>
                                                    navigate(`/distribucion-terceros/prorrateo/${detalleId}`)
                                                }
                                            />
                                        ))
                                    )}
                                </tbody>
                            </table>
                        </div>
                    )}
                </div>
            </div>
        </>
    )
}