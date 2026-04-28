import { useState, useEffect, useCallback } from 'react'
import { useAuth } from '../hooks/useAuth'
import { reporteCentroCostoService } from '../services/reporteCentroCostoService'
import { useMensajeFlash } from '../hooks/useMensajeFlash'

const ESTADOS = [
    { value: '', label: 'Todos' },
    { value: 'EA2', label: 'Aprobado' },
    { value: 'EA3', label: 'Borrador' },
    { value: 'EA4', label: 'Pendiente de aprobación' },
    { value: 'EA5', label: 'Anulado' },
]

// Badge de estado
function BadgeEstado({ codigo, nombre }) {
    const upper = (codigo ?? '').toUpperCase().trim()
    const clase =
        upper === 'EA2' ? 'badge-success'
        : upper === 'EA3' ? 'badge-secondary'
        : upper === 'EA4' ? 'badge-warning'
        : upper === 'EA5' ? 'badge-danger'
        : 'badge-dark'
    return <span className={`badge ${clase}`}>{nombre ?? codigo}</span>
}

// Paginación
function Paginacion({ paginaAct, totalPags, onChange }) {
    if (totalPags <= 1) return null
    return (
        <nav aria-label="Paginación">
            <ul className="pagination justify-content-center">
                <li className={`page-item${paginaAct === 1 ? ' disabled' : ''}`}>
                    <button className="page-link" onClick={() => onChange(paginaAct - 1)} disabled={paginaAct === 1}>
                        Anterior
                    </button>
                </li>
                {Array.from({ length: totalPags }, (_, i) => i + 1).map((p) => (
                    <li key={p} className={`page-item${p === paginaAct ? ' active' : ''}`}>
                        <button className="page-link" onClick={() => onChange(p)}>{p}</button>
                    </li>
                ))}
                <li className={`page-item${paginaAct === totalPags ? ' disabled' : ''}`}>
                    <button className="page-link" onClick={() => onChange(paginaAct + 1)} disabled={paginaAct === totalPags}>
                        Siguiente
                    </button>
                </li>
            </ul>
        </nav>
    )
}

// Componente principal
export default function ReporteCentroCosto() {
    const { user } = useAuth()
    const { alerta, cerrar: cerrarAlerta, mostrar } = useMensajeFlash()

    // Dropdown de centros de costo
    const [centrosCosto, setCentrosCosto] = useState([])
    const [cargandoCentros, setCargandoCentros] = useState(true)

    // Filtros
    const [centroCostoId, setCentroCostoId] = useState('')
    const [fechaInicio, setFechaInicio] = useState('')
    const [fechaFin, setFechaFin] = useState('')
    const [estado, setEstado] = useState('')
    const [errores, setErrores] = useState({})

    // Resultados
    const [reporte, setReporte] = useState(null)
    const [pagina, setPagina] = useState(1)
    const [cargando, setCargando] = useState(false)
    const [errorCarga, setErrorCarga] = useState('')
    const [buscado, setBuscado] = useState(false)

    // Cargar centros de costo al montar
    useEffect(() => {
        async function cargar() {
            try {
                const data = await reporteCentroCostoService.listarCentrosCosto(user.accessToken)
                if (data.statusCode === 200) {
                    setCentrosCosto(data.responseObject ?? [])
                }
            } catch {
                 console.error(err)
            } finally {
                setCargandoCentros(false)
            }
        }
        cargar()
    }, [user.accessToken])

    // Ejecutar reporte
    const ejecutarReporte = useCallback(async (paginaActual = 1) => {
        setCargando(true)
        setErrorCarga('')
        setBuscado(true)

        try {
            const data = await reporteCentroCostoService.obtenerMovimientos({
                centroCosto: centroCostoId,
                fechaInicio: fechaInicio || null,
                fechaFin: fechaFin || null,
                estado: estado || null,
                pagina: paginaActual,
                accessToken: user.accessToken,
            })

            if (data.statusCode === 200) {
                setReporte(data.responseObject)
                setPagina(paginaActual)
            } else {
                setErrorCarga(data.message ?? 'Error al generar el reporte.')
                setReporte(null)
            }
        } catch {
            setErrorCarga('No se pudo conectar con el servidor.')
            setReporte(null)
        } finally {
            setCargando(false)
        }
    }, [centroCostoId, fechaInicio, fechaFin, estado, user.accessToken])

    const handleBuscar = (e) => {
        e.preventDefault()
        const nuevosErrores = {}
        if (!centroCostoId) nuevosErrores.centroCosto = 'Debe seleccionar un centro de costo.'
        if (Object.keys(nuevosErrores).length > 0) {
            setErrores(nuevosErrores)
            return
        }
        setErrores({})
        ejecutarReporte(1)
    }

    const movimientos = reporte?.movimientos ?? []
    const total = reporte?.total ?? 0
    const totalPaginas = reporte?.totalPaginas ?? 1
    const totalRegistros = reporte?.totalRegistros ?? 0

    return (
        <>
            {/* Encabezado */}
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800">
                    <i className="fas fa-chart-line text-primary" aria-hidden="true" />{' '}
                    Reporte de Movimientos por Centro de Costo
                </h1>
            </div>

            {/* Alerta flash */}
            {alerta && (
                <div className={`alert alert-${alerta.tipo} alert-dismissible fade show`} role="alert">
                    <i className={`fas ${alerta.tipo === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle'} mr-2`} aria-hidden="true" />
                    {alerta.msg}
                    <button type="button" className="close" onClick={cerrarAlerta} aria-label="Cerrar">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
            )}

            {/* Filtros */}
            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Filtros del Reporte</h6>
                </div>
                <div className="card-body">
                    <form onSubmit={handleBuscar} noValidate>
                        <div className="row">

                            {/* Centro de Costo */}
                            <div className="col-md-4 form-group">
                                <label htmlFor="centroCosto" className="font-weight-bold">
                                    Centro de Costo <span className="text-danger">*</span>
                                </label>
                                <select
                                    id="centroCosto"
                                    className={`form-control${errores.centroCosto ? ' is-invalid' : ''}`}
                                    value={centroCostoId}
                                    onChange={(e) => {
                                        setCentroCostoId(e.target.value)
                                        setErrores((prev) => ({ ...prev, centroCosto: '' }))
                                    }}
                                    disabled={cargandoCentros}
                                >
                                    <option value="">
                                        {cargandoCentros ? 'Cargando...' : '— Seleccione —'}
                                    </option>
                                    {centrosCosto.map((cc) => (
                                        <option key={cc.centroCostoId} value={cc.centroCostoId}>
                                            {cc.nombre} ({cc.codigo})
                                        </option>
                                    ))}
                                </select>
                                {errores.centroCosto && (
                                    <div className="invalid-feedback">{errores.centroCosto}</div>
                                )}
                            </div>

                            {/* Fecha inicio */}
                            <div className="col-md-2 form-group">
                                <label htmlFor="fechaInicio" className="font-weight-bold">Fecha inicio</label>
                                <input
                                    type="date"
                                    id="fechaInicio"
                                    className="form-control"
                                    value={fechaInicio}
                                    onChange={(e) => setFechaInicio(e.target.value)}
                                />
                            </div>

                            {/* Fecha fin */}
                            <div className="col-md-2 form-group">
                                <label htmlFor="fechaFin" className="font-weight-bold">Fecha fin</label>
                                <input
                                    type="date"
                                    id="fechaFin"
                                    className="form-control"
                                    value={fechaFin}
                                    onChange={(e) => setFechaFin(e.target.value)}
                                />
                            </div>

                            {/* Estado */}
                            <div className="col-md-2 form-group">
                                <label htmlFor="estado" className="font-weight-bold">Estado asiento</label>
                                <select
                                    id="estado"
                                    className="form-control"
                                    value={estado}
                                    onChange={(e) => setEstado(e.target.value)}
                                >
                                    {ESTADOS.map((op) => (
                                        <option key={op.value} value={op.value}>{op.label}</option>
                                    ))}
                                </select>
                            </div>

                            {/* Botón */}
                            <div className="col-md-2 form-group d-flex align-items-end">
                                <button
                                    type="submit"
                                    className="btn btn-primary w-100"
                                    disabled={cargando || cargandoCentros}
                                >
                                    {cargando ? (
                                        <>
                                            <span className="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true" />
                                            Consultando...
                                        </>
                                    ) : (
                                        <><i className="fas fa-search mr-1" aria-hidden="true" /> Consultar</>
                                    )}
                                </button>
                            </div>

                        </div>
                    </form>
                </div>
            </div>

            {/* Error */}
            {errorCarga && (
                <div className="alert alert-danger" role="alert">
                    <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                    {errorCarga}
                </div>
            )}

            {/* Resultados */}
            {buscado && !cargando && !errorCarga && (
                <div className="card shadow mb-4">
                    <div className="card-header py-3 d-flex justify-content-between align-items-center">
                        <h6 className="m-0 font-weight-bold text-primary">
                            Resultados
                            {totalRegistros > 0 && (
                                <span className="ml-2 text-muted font-weight-normal" style={{ fontSize: '0.85rem' }}>
                                    ({totalRegistros} registro{totalRegistros !== 1 ? 's' : ''})
                                </span>
                            )}
                        </h6>
                        {movimientos.length > 0 && (
                            <span className="badge badge-primary" style={{ fontSize: '0.9rem' }}>
                                Total: ₡ {Number(total).toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                            </span>
                        )}
                    </div>
                    <div className="card-body">
                        <div className="table-responsive">
                            <table className="table table-bordered table-hover" width="100%" cellSpacing="0">
                                <thead className="thead-light">
    <tr>
        <th>Asiento</th>
        <th>Fecha</th>
        <th>Código</th>
        <th>Referencia</th>
        <th>Cuenta</th>
        <th>Tipo Mov.</th>
        <th className="text-right">Monto</th>
        <th>Estado</th>
    </tr>
</thead>
<tbody>
    {movimientos.length === 0 ? (
        <tr>
            <td colSpan={8} className="text-center text-muted">
                <i className="fas fa-inbox fa-2x" aria-hidden="true" />
                <p className="mb-0">
                    No se encontraron movimientos para los filtros seleccionados.
                </p>
            </td>
        </tr>
    ) : (
        movimientos.map((m, idx) => (
            <tr key={`${m.asientoId}-${m.detalleId}-${idx}`}>
                <td>{m.consecutivo ?? m.asientoId}</td>
                <td>{m.fechaAsiento}</td>
                <td>{m.codigoAsiento ?? '—'}</td>
                <td>{m.referencia ?? '—'}</td>
                <td>{m.cuentaCodigo} - {m.cuentaNombre}</td>

                <td>
                    {m.tipoMovimiento
                        ? m.tipoMovimiento.charAt(0).toUpperCase() + m.tipoMovimiento.slice(1)
                        : '—'}
                </td>

                <td className="text-right">
                    ₡ {Number(m.monto).toLocaleString('es-CR', { minimumFractionDigits: 2 })}
                </td>

                <td>
                    <BadgeEstado codigo={m.estadoCodigo} nombre={m.estadoNombre} />
                </td>
            </tr>
        ))
    )}
</tbody>
                            </table>
                        </div>

                        <Paginacion
                            paginaAct={pagina}
                            totalPags={totalPaginas}
                            onChange={(p) => ejecutarReporte(p)}
                        />
                    </div>
                </div>
            )}
        </>
    )
}