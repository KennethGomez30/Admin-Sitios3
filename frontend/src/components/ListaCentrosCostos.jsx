import { useState, useEffect, useCallback, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useMensajeFlash } from '../hooks/useMensajeFlash'
import { centroCostoService } from '../services/AdminCentroCostoService'

// Modal de confirmación de eliminación
function ModalEliminar({ centroCosto, onConfirmar, onCancelar, eliminando }) {
    const modalRef = useRef(null)

    useEffect(() => {
        const $m = window.$(modalRef.current)
        if (centroCosto) {
            $m.modal({ backdrop: 'static', keyboard: false })
            $m.modal('show')
        } else {
            $m.modal('hide')
        }
    }, [centroCosto])

    return (
        <div className="modal fade" id="modalEliminar" tabIndex="-1" role="dialog" ref={modalRef}>
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
                        <p>¿Realmente desea eliminar el elemento seleccionado?</p>
                        <p>
                            <strong>Centro de Costo:</strong>{' '}
                            <span>{centroCosto?.nombre} ({centroCosto?.codigo})</span>
                        </p>
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
                                    <span
                                        className="spinner-border spinner-border-sm mr-2"
                                        role="status"
                                        aria-hidden="true"
                                    />
                                    Eliminando...
                                </>
                            ) : (
                                'Sí'
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    )
}

// Paginación — server-side
function Paginacion({ paginaAct, totalPags, onChange }) {
    if (totalPags <= 1) return null
    return (
        <nav aria-label="Paginación">
            <ul className="pagination justify-content-center">
                <li className={`page-item${paginaAct === 1 ? ' disabled' : ''}`}>
                    <button
                        className="page-link"
                        onClick={() => onChange(paginaAct - 1)}
                        disabled={paginaAct === 1}
                    >
                        Anterior
                    </button>
                </li>
                {Array.from({ length: totalPags }, (_, i) => i + 1).map((p) => (
                    <li key={p} className={`page-item${p === paginaAct ? ' active' : ''}`}>
                        <button className="page-link" onClick={() => onChange(p)}>
                            {p}
                        </button>
                    </li>
                ))}
                <li className={`page-item${paginaAct === totalPags ? ' disabled' : ''}`}>
                    <button
                        className="page-link"
                        onClick={() => onChange(paginaAct + 1)}
                        disabled={paginaAct === totalPags}
                    >
                        Siguiente
                    </button>
                </li>
            </ul>
        </nav>
    )
}

// Componente principal
export default function CentroCostoAdminListado() {
    const { user } = useAuth()
    const navigate = useNavigate()
    const { alerta, cerrar: cerrarAlerta, mostrar: mostrarAlerta } = useMensajeFlash()

    const [centrosCosto, setCentrosCosto] = useState([])
    const [cargando, setCargando] = useState(true)
    const [errorCarga, setErrorCarga] = useState('')
    const [paginaAct, setPaginaAct] = useState(1)
    const [totalPags, setTotalPags] = useState(1)
    const [paraEliminar, setParaEliminar] = useState(null)
    const [eliminando, setEliminando] = useState(false)

    const cargarCentrosCosto = useCallback(async (pagina = 1) => {
        setCargando(true)
        setErrorCarga('')
        try {
            const data = await centroCostoService.listar(pagina, user.accessToken)
            if (data.statusCode === 200) {
                const obj = data.responseObject
                setCentrosCosto(obj.data ?? [])
                setTotalPags(obj.totalPaginas ?? 1)
                setPaginaAct(obj.pagina ?? pagina)
            } else {
                setErrorCarga(data.message ?? 'Error al cargar centros de costo.')
            }
        } catch {
            setErrorCarga('No se pudo conectar con el servidor.')
        } finally {
            setCargando(false)
        }
    }, [user.accessToken])

    useEffect(() => {
        cargarCentrosCosto(1)
    }, [cargarCentrosCosto])

    const cambiarPagina = (nuevaPagina) => {
        cargarCentrosCosto(nuevaPagina)
    }

    const confirmarEliminar = useCallback(async () => {
        if (!paraEliminar) return
        setEliminando(true)
        try {
            const data = await centroCostoService.eliminar(paraEliminar.codigo, user.accessToken)
            setParaEliminar(null)

            if (data.statusCode === 200) {
                mostrarAlerta('success', 'Centro de costo eliminado exitosamente.')
                cargarCentrosCosto(paginaAct)
            } else if (data.statusCode === 400) {
                mostrarAlerta('danger', 'No se puede eliminar un registro con datos relacionados.')
            } else {
                mostrarAlerta('danger', data.message ?? 'Error al eliminar. Intente nuevamente.')
            }
        } catch {
            setParaEliminar(null)
            mostrarAlerta('danger', 'Error de conexión al intentar eliminar.')
        } finally {
            setEliminando(false)
        }
    }, [paraEliminar, user.accessToken, mostrarAlerta, cargarCentrosCosto, paginaAct])

    return (
        <>
            {/* Encabezado */}
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800">
                    <i className="fas fa-sitemap text-primary" aria-hidden="true" /> Administración de Centros de Costo
                </h1>
                <button
                    className="d-none d-sm-inline-block btn btn-sm btn-primary shadow-sm"
                    onClick={() => navigate('/centro-costo/crear')}
                >
                    <i className="fas fa-plus fa-sm text-white-50" aria-hidden="true" /> Nuevo Centro de Costo
                </button>
            </div>

            {/* Alerta flash */}
            {alerta && (
                <div
                    className={`alert alert-${alerta.tipo} alert-dismissible fade show`}
                    role="alert"
                >
                    <i
                        className={`fas ${alerta.tipo === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle'} mr-2`}
                        aria-hidden="true"
                    />
                    {alerta.msg}
                    <button
                        type="button"
                        className="close"
                        onClick={cerrarAlerta}
                        aria-label="Cerrar"
                    >
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
            )}

            {/* Tarjeta con tabla */}
            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Listado de Centros de Costo</h6>
                </div>
                <div className="card-body">

                    {/* Cargando */}
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

                    {/* Error */}
                    {!cargando && errorCarga && (
                        <div className="alert alert-danger" role="alert">
                            <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                            {errorCarga}
                            <button
                                className="btn btn-sm btn-link ml-2"
                                onClick={() => cargarCentrosCosto(paginaAct)}
                            >
                                Reintentar
                            </button>
                        </div>
                    )}

                    {/* Tabla */}
                    {!cargando && !errorCarga && (
                        <>
                            <div className="table-responsive">
                                <table
                                    className="table table-bordered table-hover"
                                    width="100%"
                                    cellSpacing="0"
                                >
                                    <thead className="thead-light">
                                        <tr>
                                            <th>Código</th>
                                            <th>Nombre</th>
                                            <th>Descripción</th>
                                            <th>Estado</th>
                                            <th>Acciones</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {centrosCosto.length === 0 ? (
                                            <tr>
                                                <td colSpan={5} className="text-center text-muted">
                                                    <i className="fas fa-inbox fa-2x" aria-hidden="true" />
                                                    <p className="mb-0">No hay centros de costo registrados</p>
                                                </td>
                                            </tr>
                                        ) : (
                                            centrosCosto.map((cc) => (
                                                <tr key={cc.codigo}>
                                                    <td>{cc.codigo}</td>
                                                    <td>{cc.nombre}</td>
                                                    <td>{cc.descripcion ?? '—'}</td>
                                                    <td>
                                                        {cc.estado === 'Activo' ? (
                                                            <span className="badge badge-primary">{cc.estado}</span>
                                                        ) : (
                                                            <span className="badge badge-secondary">{cc.estado}</span>
                                                        )}
                                                    </td>
                                                    <td className="text-nowrap">
                                                        <button
                                                            className="btn btn-sm btn-primary mr-1"
                                                            title="Editar"
                                                            onClick={() => navigate(`/centro-costo/editar/${cc.codigo}`)}
                                                        >
                                                            <i className="fas fa-edit" aria-hidden="true" />
                                                        </button>
                                                        <button
                                                            className="btn btn-sm btn-danger"
                                                            title="Eliminar"
                                                            onClick={() => setParaEliminar(cc)}
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

                            <Paginacion
                                paginaAct={paginaAct}
                                totalPags={totalPags}
                                onChange={cambiarPagina}
                            />
                        </>
                    )}
                </div>
            </div>

            <ModalEliminar
                centroCosto={paraEliminar}
                onConfirmar={confirmarEliminar}
                onCancelar={() => setParaEliminar(null)}
                eliminando={eliminando}
            />
        </>
    )
}