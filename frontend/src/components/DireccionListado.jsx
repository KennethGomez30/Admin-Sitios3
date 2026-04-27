import { useState, useEffect, useCallback, useRef } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useMensajeFlash } from '../hooks/useMensajeFlash'
import { direccionService } from '../services/direccionService'
import '../styles/terceros.css'

const POR_PAGINA = 10

// Modal de confirmación de eliminación
function ModalEliminar({ direccion, onConfirmar, onCancelar, eliminando }) {
    const modalRef = useRef(null)

    useEffect(() => {
        const $m = window.$(modalRef.current)
        if (direccion) {
            $m.modal({ backdrop: 'static', keyboard: false })
            $m.modal('show')
        } else {
            $m.modal('hide')
        }
    }, [direccion])

    return (
        <div className="modal fade" id="modalEliminarDir" tabIndex="-1" role="dialog" ref={modalRef}>
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
                            <strong>Dirección:</strong>{' '}
                            <span>{direccion?.alias}</span>
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

// Paginación
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
export default function DireccionListado() {
    const { terceroId } = useParams()
    const { user } = useAuth()
    const navigate = useNavigate()
    const { alerta, cerrar: cerrarAlerta, mostrar: mostrarAlerta } = useMensajeFlash()

    const [direcciones, setDirecciones] = useState([])
    const [cargando, setCargando] = useState(true)
    const [errorCarga, setErrorCarga] = useState('')
    const [paginaAct, setPaginaAct] = useState(1)
    const [paraEliminar, setParaEliminar] = useState(null)
    const [eliminando, setEliminando] = useState(false)

    // Cargar listado
    const cargar = useCallback(async () => {
        setCargando(true)
        setErrorCarga('')
        try {
            const result = await direccionService.listarPorTercero(
                terceroId,
                user.accessToken
            )
            if (result.ok) {
                setDirecciones(result.data ?? [])
            } else {
                setErrorCarga(result.message)
            }
        } catch {
            setErrorCarga('No se pudo conectar con el servidor.')
        } finally {
            setCargando(false)
        }
    }, [terceroId, user.accessToken])

    useEffect(() => {
        cargar()
    }, [cargar])

    // Paginación client-side
    const totalPags = Math.max(1, Math.ceil(direcciones.length / POR_PAGINA))
    const paginaReal = Math.min(paginaAct, totalPags)
    const slice = direcciones.slice((paginaReal - 1) * POR_PAGINA, paginaReal * POR_PAGINA)

    // Eliminar
    const confirmarEliminar = useCallback(async () => {
        if (!paraEliminar) return
        setEliminando(true)
        try {
            const result = await direccionService.eliminar(
                paraEliminar.id,
                user.accessToken
            )

            if (result.ok) {
                setParaEliminar(null)
                await cargar()
                mostrarAlerta('success', 'Dirección eliminada exitosamente.')
            } else if (result.status === 409) {
                setParaEliminar(null)
                mostrarAlerta('danger', result.message)
            } else {
                setParaEliminar(null)
                mostrarAlerta('danger', result.message ?? 'Error al eliminar la dirección.')
            }
        } catch {
            setParaEliminar(null)
            mostrarAlerta('danger', 'Error de conexión al intentar eliminar.')
        } finally {
            setEliminando(false)
        }
    }, [paraEliminar, user.accessToken, cargar, mostrarAlerta])

    // Validar terceroId — redirigir antes de renderizar
    const terceroIdNum = Number(terceroId)
    if (!terceroId || isNaN(terceroIdNum) || terceroIdNum <= 0) {
        navigate('/terceros', { replace: true })
        return null
    }

    return (
        <>
            {/* Encabezado */}
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800">
                    <i className="fas fa-map-marker-alt text-primary" aria-hidden="true" /> Direcciones del Tercero
                </h1>
                <div>
                    <button
                        className="d-none d-sm-inline-block btn btn-sm btn-secondary shadow-sm mr-2"
                        onClick={() => navigate('/terceros')}
                    >
                        <i className="fas fa-arrow-left fa-sm text-white-50" aria-hidden="true" /> Volver al Listado
                    </button>
                    <button
                        className="d-none d-sm-inline-block btn btn-sm btn-primary shadow-sm"
                        onClick={() => navigate(`/terceros/${terceroId}/direcciones/crear`)}
                    >
                        <i className="fas fa-plus fa-sm text-white-50" aria-hidden="true" /> Nueva Dirección
                    </button>
                </div>
            </div>

            {/* Alerta flash */}
            {alerta && (
                <div className={`alert alert-${alerta.tipo} alert-dismissible fade show`} role="alert">
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
                    <h6 className="m-0 font-weight-bold text-primary">Listado de Direcciones</h6>
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

                    {/* Error de carga */}
                    {!cargando && errorCarga && (
                        <div className="alert alert-danger" role="alert">
                            <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                            {errorCarga}
                            <button
                                className="btn btn-sm btn-link ml-2"
                                onClick={cargar}
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
                                            <th>Alias</th>
                                            <th>Ubicación</th>
                                            <th>Dirección Exacta</th>
                                            <th>Principal</th>
                                            <th>Estado</th>
                                            <th>Acciones</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {slice.length === 0 ? (
                                            <tr>
                                                <td
                                                    colSpan={6}
                                                    className="text-center text-muted tabla-vacia"
                                                >
                                                    <i className="fas fa-inbox fa-2x" aria-hidden="true" />
                                                    <p className="mb-0">No hay direcciones registradas</p>
                                                </td>
                                            </tr>
                                        ) : (
                                            slice.map((d) => (
                                                <tr key={d.id}>
                                                    <td>{d.alias}</td>
                                                    <td>{d.ubicacion ?? '—'}</td>
                                                    <td>{d.direccionExacta}</td>
                                                    <td>
                                                        {d.esPrincipal ? (
                                                            <span className="badge badge-success">
                                                                <i className="fas fa-star" aria-hidden="true" /> Sí
                                                            </span>
                                                        ) : (
                                                            <span className="text-muted">—</span>
                                                        )}
                                                    </td>
                                                    <td>
                                                        {d.estado === 'Activa' ? (
                                                            <span className="badge badge-primary">
                                                                {d.estado}
                                                            </span>
                                                        ) : (
                                                            <span className="badge badge-secondary">
                                                                {d.estado}
                                                            </span>
                                                        )}
                                                    </td>
                                                    <td className="text-nowrap td-acciones">
                                                        <button
                                                            className="btn btn-sm btn-primary"
                                                            title="Editar"
                                                            onClick={() =>
                                                                navigate(`/terceros/${terceroId}/direcciones/editar/${d.id}`)
                                                            }
                                                        >
                                                            <i className="fas fa-edit" aria-hidden="true" />
                                                        </button>
                                                        <button
                                                            className="btn btn-sm btn-danger btn-eliminar"
                                                            title="Eliminar"
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

                            <Paginacion
                                paginaAct={paginaReal}
                                totalPags={totalPags}
                                onChange={setPaginaAct}
                            />
                        </>
                    )}
                </div>
            </div>

            {/* Modal de confirmación de eliminación */}
            <ModalEliminar
                direccion={paraEliminar}
                onConfirmar={confirmarEliminar}
                onCancelar={() => setParaEliminar(null)}
                eliminando={eliminando}
            />
        </>
    )
}