import { useState, useEffect, useCallback, useRef } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useMensajeFlash } from '../hooks/useMensajeFlash'
import { contactoService } from '../services/contactoService'
import '../styles/terceros.css'

const POR_PAGINA = 10

function ModalEliminar({ contacto, onConfirmar, onCancelar, eliminando }) {
    const modalRef = useRef(null)

    useEffect(() => {
        const $m = window.$(modalRef.current)
        if (contacto) {
            $m.modal({ backdrop: 'static', keyboard: false })
            $m.modal('show')
        } else {
            $m.modal('hide')
        }
    }, [contacto])

    return (
        <div className="modal fade" id="modalEliminarContacto" tabIndex="-1" role="dialog" ref={modalRef}>
            <div className="modal-dialog modal-dialog-centered" role="document">
                <div className="modal-content">
                    <div className="modal-header bg-danger text-white">
                        <h5 className="modal-title">
                            <i className="fas fa-exclamation-triangle" aria-hidden="true" /> Confirmar Eliminación
                        </h5>
                        <button type="button" className="close text-white" onClick={onCancelar} disabled={eliminando} aria-label="Cerrar">
                            <span aria-hidden="true">&times;</span>
                        </button>
                    </div>
                    <div className="modal-body">
                        <p>¿Realmente desea eliminar el elemento seleccionado?</p>
                        <p><strong>Contacto:</strong> <span>{contacto?.nombre}</span></p>
                    </div>
                    <div className="modal-footer">
                        <button type="button" className="btn btn-secondary" onClick={onCancelar} disabled={eliminando}>No</button>
                        <button type="button" className="btn btn-danger" onClick={onConfirmar} disabled={eliminando}>
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

function Paginacion({ paginaAct, totalPags, onChange }) {
    if (totalPags <= 1) return null
    return (
        <nav aria-label="Paginación">
            <ul className="pagination justify-content-center">
                <li className={`page-item${paginaAct === 1 ? ' disabled' : ''}`}>
                    <button className="page-link" onClick={() => onChange(paginaAct - 1)} disabled={paginaAct === 1}>Anterior</button>
                </li>
                {Array.from({ length: totalPags }, (_, i) => i + 1).map((p) => (
                    <li key={p} className={`page-item${p === paginaAct ? ' active' : ''}`}>
                        <button className="page-link" onClick={() => onChange(p)}>{p}</button>
                    </li>
                ))}
                <li className={`page-item${paginaAct === totalPags ? ' disabled' : ''}`}>
                    <button className="page-link" onClick={() => onChange(paginaAct + 1)} disabled={paginaAct === totalPags}>Siguiente</button>
                </li>
            </ul>
        </nav>
    )
}

// Color del badge según el tipo
function badgeTipo(tipo) {
    switch (tipo) {
        case 'Principal':   return 'badge-success'
        case 'Facturación': return 'badge-info'
        case 'Cobros':      return 'badge-warning'
        case 'Soporte':     return 'badge-primary'
        default:            return 'badge-secondary'
    }
}

export default function ContactoListado() {
    const { terceroId } = useParams()
    const { user } = useAuth()
    const navigate = useNavigate()
    const { alerta, cerrar: cerrarAlerta, mostrar: mostrarAlerta } = useMensajeFlash()

    const [contactos, setContactos] = useState([])
    const [cargando, setCargando] = useState(true)
    const [errorCarga, setErrorCarga] = useState('')
    const [paginaAct, setPaginaAct] = useState(1)
    const [paraEliminar, setParaEliminar] = useState(null)
    const [eliminando, setEliminando] = useState(false)

    const cargar = useCallback(async () => {
        setCargando(true)
        setErrorCarga('')
        try {
            const result = await contactoService.listarPorTercero(terceroId, user.accessToken)
            if (result.ok) setContactos(result.data ?? [])
            else setErrorCarga(result.message)
        } catch {
            setErrorCarga('No se pudo conectar con el servidor.')
        } finally {
            setCargando(false)
        }
    }, [terceroId, user.accessToken])

    useEffect(() => { cargar() }, [cargar])

    const totalPags = Math.max(1, Math.ceil(contactos.length / POR_PAGINA))
    const paginaReal = Math.min(paginaAct, totalPags)
    const slice = contactos.slice((paginaReal - 1) * POR_PAGINA, paginaReal * POR_PAGINA)

    const confirmarEliminar = useCallback(async () => {
        if (!paraEliminar) return
        setEliminando(true)
        try {
            const result = await contactoService.eliminar(paraEliminar.id, user.accessToken)
            if (result.ok) {
                setParaEliminar(null)
                await cargar()
                mostrarAlerta('success', 'Contacto eliminado exitosamente.')
            } else if (result.status === 409) {
                setParaEliminar(null)
                mostrarAlerta('danger', result.message)
            } else {
                setParaEliminar(null)
                mostrarAlerta('danger', result.message ?? 'Error al eliminar el contacto.')
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
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800">
                    <i className="fas fa-address-book text-primary" aria-hidden="true" /> Contactos del Tercero
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
                        onClick={() => navigate(`/terceros/${terceroId}/contactos/crear`)}
                    >
                        <i className="fas fa-plus fa-sm text-white-50" aria-hidden="true" /> Nuevo Contacto
                    </button>
                </div>
            </div>

            {alerta && (
                <div className={`alert alert-${alerta.tipo} alert-dismissible fade show`} role="alert">
                    <i className={`fas ${alerta.tipo === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle'} mr-2`} aria-hidden="true" />
                    {alerta.msg}
                    <button type="button" className="close" onClick={cerrarAlerta} aria-label="Cerrar">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
            )}

            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Listado de Contactos</h6>
                </div>
                <div className="card-body">

                    {cargando && (
                        <div className="text-center py-4">
                            <span className="spinner-border text-primary" style={{ width: '2rem', height: '2rem' }} role="status">
                                <span className="sr-only">Cargando...</span>
                            </span>
                        </div>
                    )}

                    {!cargando && errorCarga && (
                        <div className="alert alert-danger" role="alert">
                            <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                            {errorCarga}
                            <button className="btn btn-sm btn-link ml-2" onClick={cargar}>Reintentar</button>
                        </div>
                    )}

                    {!cargando && !errorCarga && (
                        <>
                            <div className="table-responsive">
                                <table className="table table-bordered table-hover" width="100%" cellSpacing="0">
                                    <thead className="thead-light">
                                        <tr>
                                            <th>Nombre</th>
                                            <th>Cargo</th>
                                            <th>Email</th>
                                            <th>Teléfono</th>
                                            <th>Tipo</th>
                                            <th>Estado</th>
                                            <th>Acciones</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {slice.length === 0 ? (
                                            <tr>
                                                <td colSpan={7} className="text-center text-muted tabla-vacia">
                                                    <i className="fas fa-inbox fa-2x" aria-hidden="true" />
                                                    <p className="mb-0">No hay contactos registrados</p>
                                                </td>
                                            </tr>
                                        ) : (
                                            slice.map((c) => (
                                                <tr key={c.id}>
                                                    <td>{c.nombre}</td>
                                                    <td>{c.cargo ?? '—'}</td>
                                                    <td>{c.email ?? '—'}</td>
                                                    <td>{c.telefono ?? '—'}</td>
                                                    <td>
                                                        <span className={`badge ${badgeTipo(c.tipo)}`}>{c.tipo}</span>
                                                    </td>
                                                    <td>
                                                        {c.estado === 'Activo' ? (
                                                            <span className="badge badge-primary">{c.estado}</span>
                                                        ) : (
                                                            <span className="badge badge-secondary">{c.estado}</span>
                                                        )}
                                                    </td>
                                                    <td className="text-nowrap td-acciones">
                                                        <button
                                                            className="btn btn-sm btn-primary"
                                                            title="Editar"
                                                            onClick={() => navigate(`/terceros/${terceroId}/contactos/editar/${c.id}`)}
                                                        >
                                                            <i className="fas fa-edit" aria-hidden="true" />
                                                        </button>
                                                        <button
                                                            className="btn btn-sm btn-danger btn-eliminar"
                                                            title="Eliminar"
                                                            onClick={() => setParaEliminar(c)}
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
                            <Paginacion paginaAct={paginaReal} totalPags={totalPags} onChange={setPaginaAct} />
                        </>
                    )}
                </div>
            </div>

            <ModalEliminar
                contacto={paraEliminar}
                onConfirmar={confirmarEliminar}
                onCancelar={() => setParaEliminar(null)}
                eliminando={eliminando}
            />
        </>
    )
}