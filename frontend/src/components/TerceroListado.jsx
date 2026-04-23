import { useState, useEffect, useCallback, useRef } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useMensajeFlash } from '../hooks/useMensajeFlash'
import { terceroService } from '../services/terceroService'
import '../styles/terceros.css'

const POR_PAGINA = 10

// Modal de confirmación de eliminación
function ModalEliminar({ tercero, onConfirmar, onCancelar, eliminando }) {
    const modalRef = useRef(null)

    // Activar/desactivar Bootstrap modal vía jQuery ya cargado en index.html
    useEffect(() => {
        const $m = window.$(modalRef.current)
        if (tercero) {
            $m.modal({ backdrop: 'static', keyboard: false })
            $m.modal('show')
        } else {
            $m.modal('hide')
        }
    }, [tercero])

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
                            <strong>Tercero:</strong>{' '}
                            <span>{tercero?.nombreRazonSocial}</span>
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
export default function TerceroListado() {
    const { user } = useAuth()
    const navigate = useNavigate()
    const { alerta, cerrar: cerrarAlerta, mostrar: mostrarAlerta, guardar: guardarAlerta } = useMensajeFlash()

    const [terceros, setTerceros] = useState([])
    const [cargando, setCargando] = useState(true)
    const [errorCarga, setErrorCarga] = useState('')
    const [paginaAct, setPaginaAct] = useState(1)
    const [paraEliminar, setParaEliminar] = useState(null)   // TerceroEntity | null
    const [eliminando, setEliminando] = useState(false)

    // Cargar listado completo la paginación es client-side
    const cargarTerceros = useCallback(async () => {
        setCargando(true)
        setErrorCarga('')
        try {
            const data = await terceroService.listar(user.accessToken)
            if (data.statusCode === 200) {
                setTerceros(data.responseObject ?? [])
            } else {
                setErrorCarga(data.message ?? 'Error al cargar terceros.')
            }
        } catch (err) {
            setErrorCarga('No se pudo conectar con el servidor.')
        } finally {
            setCargando(false)
        }
    }, [user.accessToken])

    useEffect(() => {
        cargarTerceros()
    }, [cargarTerceros])

    // Paginación client-side
    const totalPags = Math.max(1, Math.ceil(terceros.length / POR_PAGINA))
    const paginaReal = Math.min(paginaAct, totalPags)
    const slice = terceros.slice((paginaReal - 1) * POR_PAGINA, paginaReal * POR_PAGINA)

    // Eliminar
    const confirmarEliminar = useCallback(async () => {
        if (!paraEliminar) return
        setEliminando(true)
        try {
            const data = await terceroService.eliminar(paraEliminar.id, user.accessToken)

            if (data.statusCode === 204) {
                setParaEliminar(null)
                const lista = await terceroService.listar(user.accessToken)
                if (lista.statusCode === 200) setTerceros(lista.responseObject ?? [])
                mostrarAlerta('success', 'Tercero eliminado exitosamente.')
            } else if (data.statusCode === 409) {
                setParaEliminar(null)
                mostrarAlerta('danger', 'No se puede eliminar un registro con datos relacionados.')
            } else {
                setParaEliminar(null)
                mostrarAlerta('danger', data.message ?? 'Error al eliminar el tercero. Intente nuevamente.')
            }
        } catch {
            setParaEliminar(null)
            mostrarAlerta('danger', 'Error de conexión al intentar eliminar.')
        } finally {
            setEliminando(false)
        }
    }, [paraEliminar, user.accessToken, mostrarAlerta])

    // Render
    return (
        <>
            {/* Encabezado de página */}
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800">
                    <i className="fas fa-users text-primary" aria-hidden="true" /> Administración de Terceros
                </h1>
                <button
                    className="d-none d-sm-inline-block btn btn-sm btn-primary shadow-sm"
                    onClick={() => navigate('/terceros/crear')}
                >
                    <i className="fas fa-plus fa-sm text-white-50" aria-hidden="true" /> Nuevo Tercero
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
                    <h6 className="m-0 font-weight-bold text-primary">Listado de Terceros</h6>
                </div>
                <div className="card-body">

                    {/* Estado cargando */}
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

                    {/* Estado error de carga */}
                    {!cargando && errorCarga && (
                        <div className="alert alert-danger" role="alert">
                            <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                            {errorCarga}
                            <button
                                className="btn btn-sm btn-link ml-2"
                                onClick={cargarTerceros}
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
                                            <th>Identificación</th>
                                            <th>Nombre / Razón Social</th>
                                            <th>Tipo</th>
                                            <th>Email</th>
                                            <th>Teléfono</th>
                                            <th>Estado</th>
                                            <th>Acciones</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {slice.length === 0 ? (
                                            <tr>
                                                <td
                                                    colSpan={7}
                                                    className="text-center text-muted tabla-vacia"
                                                >
                                                    <i
                                                        className="fas fa-inbox fa-2x"
                                                        aria-hidden="true"
                                                    />
                                                    <p className="mb-0">No hay terceros registrados</p>
                                                </td>
                                            </tr>
                                        ) : (
                                            slice.map((t) => (
                                                <tr key={t.id}>
                                                    <td>{t.identificacion}</td>
                                                    <td>{t.nombreRazonSocial}</td>
                                                    <td>{t.tipo}</td>
                                                    <td>{t.email ?? '—'}</td>
                                                    <td>{t.telefono ?? '—'}</td>
                                                    <td>
                                                        {t.estado === 'Activo' ? (
                                                            <span className="badge badge-primary">
                                                                {t.estado}
                                                            </span>
                                                        ) : (
                                                            <span className="badge badge-secondary">
                                                                {t.estado}
                                                            </span>
                                                        )}
                                                    </td>
                                                    <td className="text-nowrap td-acciones">
                                                        <button
                                                            className="btn btn-sm btn-primary"
                                                            title="Editar"
                                                            onClick={() =>
                                                                navigate(`/terceros/editar/${t.id}`)
                                                            }
                                                        >
                                                            <i className="fas fa-edit" aria-hidden="true" />
                                                        </button>
                                                        <a
                                                            href={`../AUX11/index.php?tercero_id=${t.id}`}
                                                            className="btn btn-sm btn-warning"
                                                            title="Direcciones"
                                                        >
                                                            <i className="fas fa-map-marker-alt" aria-hidden="true" />
                                                        </a>
                                                        <a
                                                            href={`../AUX12/index.php?tercero_id=${t.id}`}
                                                            className="btn btn-sm btn-info"
                                                            title="Contactos"
                                                        >
                                                            <i className="fas fa-address-book" aria-hidden="true" />
                                                        </a>
                                                        <button
                                                            className="btn btn-sm btn-danger btn-eliminar"
                                                            title="Eliminar"
                                                            onClick={() => setParaEliminar(t)}
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
                tercero={paraEliminar}
                onConfirmar={confirmarEliminar}
                onCancelar={() => setParaEliminar(null)}
                eliminando={eliminando}
            />
        </>
    )
}