import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useMensajeFlash } from '../hooks/useMensajeFlash'
import { centroCostoService } from '../services/AdminCentroCostoService'
 
const ESTADO_OPCIONES = ['Activo', 'Inactivo']
 
export default function CentroCostoAdminCrear() {
    const { user } = useAuth()
    const navigate = useNavigate()
    const { guardar: guardarAlerta } = useMensajeFlash()
 
    const [form, setForm] = useState({
        codigo: '',
        nombre: '',
        descripcion: '',
        estado: 'Activo',
    })
    const [errores, setErrores] = useState({})
    const [guardando, setGuardando] = useState(false)
    const [errorServidor, setErrorServidor] = useState('')
 
    const handleChange = (e) => {
        const { name, value } = e.target
        setForm((prev) => ({ ...prev, [name]: value }))
        setErrores((prev) => ({ ...prev, [name]: '' }))
        setErrorServidor('')
    }
 
    const validar = () => {
        const nuevosErrores = {}
        if (!form.codigo.trim()) nuevosErrores.codigo = 'El código es requerido.'
        if (!form.nombre.trim()) nuevosErrores.nombre = 'El nombre es requerido.'
        if (!form.estado) nuevosErrores.estado = 'El estado es requerido.'
        return nuevosErrores
    }
 
    const handleSubmit = async (e) => {
        e.preventDefault()
        const nuevosErrores = validar()
        if (Object.keys(nuevosErrores).length > 0) {
            setErrores(nuevosErrores)
            return
        }
 
        setGuardando(true)
        setErrorServidor('')
        try {
            const payload = {
                codigo: form.codigo.trim(),
                nombre: form.nombre.trim(),
                descripcion: form.descripcion.trim() || null,
                estado: form.estado,
            }
            const data = await centroCostoService.crear(payload, user.accessToken)
 
            if (data.statusCode === 200) {
                guardarAlerta('success', 'Centro de costo creado exitosamente.')
                navigate('/centro-costo/admin')
            } else {
                setErrorServidor(data.message ?? 'Error al crear el centro de costo.')
            }
        } catch {
            setErrorServidor('No se pudo conectar con el servidor.')
        } finally {
            setGuardando(false)
        }
    }
 
    return (
        <>
            {/* Encabezado */}
            <div className="d-sm-flex align-items-center justify-content-between mb-4">
                <h1 className="h3 mb-0 text-gray-800">
                    <i className="fas fa-plus-circle text-primary" aria-hidden="true" /> Nuevo Centro de Costo
                </h1>
                <button
                    className="d-none d-sm-inline-block btn btn-sm btn-secondary shadow-sm"
                    onClick={() => navigate('/centro-costo/admin')}
                >
                    <i className="fas fa-arrow-left fa-sm mr-1" aria-hidden="true" /> Volver
                </button>
            </div>
 
            <div className="card shadow mb-4">
                <div className="card-header py-3">
                    <h6 className="m-0 font-weight-bold text-primary">Datos del Centro de Costo</h6>
                </div>
                <div className="card-body">
 
                    {/* Error servidor */}
                    {errorServidor && (
                        <div className="alert alert-danger" role="alert">
                            <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                            {errorServidor}
                        </div>
                    )}
 
                    <form onSubmit={handleSubmit} noValidate>
                        <div className="form-row">
 
                            {/* Código */}
                            <div className="form-group col-md-4">
                                <label htmlFor="codigo">
                                    Código <span className="text-danger">*</span>
                                </label>
                                <input
                                    type="text"
                                    id="codigo"
                                    name="codigo"
                                    className={`form-control${errores.codigo ? ' is-invalid' : ''}`}
                                    value={form.codigo}
                                    onChange={handleChange}
                                    maxLength={50}
                                    placeholder="Ej: CC001"
                                />
                                {errores.codigo && (
                                    <div className="invalid-feedback">{errores.codigo}</div>
                                )}
                            </div>
 
                            {/* Nombre */}
                            <div className="form-group col-md-5">
                                <label htmlFor="nombre">
                                    Nombre <span className="text-danger">*</span>
                                </label>
                                <input
                                    type="text"
                                    id="nombre"
                                    name="nombre"
                                    className={`form-control${errores.nombre ? ' is-invalid' : ''}`}
                                    value={form.nombre}
                                    onChange={handleChange}
                                    maxLength={100}
                                    placeholder="Nombre del centro de costo"
                                />
                                {errores.nombre && (
                                    <div className="invalid-feedback">{errores.nombre}</div>
                                )}
                            </div>
 
                            {/* Estado */}
                            <div className="form-group col-md-3">
                                <label htmlFor="estado">
                                    Estado <span className="text-danger">*</span>
                                </label>
                                <select
                                    id="estado"
                                    name="estado"
                                    className={`form-control${errores.estado ? ' is-invalid' : ''}`}
                                    value={form.estado}
                                    onChange={handleChange}
                                >
                                    {ESTADO_OPCIONES.map((op) => (
                                        <option key={op} value={op}>{op}</option>
                                    ))}
                                </select>
                                {errores.estado && (
                                    <div className="invalid-feedback">{errores.estado}</div>
                                )}
                            </div>
 
                        </div>
 
                        {/* Descripción */}
                        <div className="form-group">
                            <label htmlFor="descripcion">Descripción <span className="text-muted">(opcional)</span></label>
                            <textarea
                                id="descripcion"
                                name="descripcion"
                                className="form-control"
                                value={form.descripcion}
                                onChange={handleChange}
                                rows={3}
                                maxLength={255}
                                placeholder="Descripción del centro de costo"
                            />
                        </div>
 
                        {/* Botones */}
                        <div className="d-flex justify-content-end">
                            <button
                                type="button"
                                className="btn btn-secondary mr-2"
                                onClick={() => navigate('/centro-costo/admin')}
                                disabled={guardando}
                            >
                                Cancelar
                            </button>
                            <button
                                type="submit"
                                className="btn btn-primary"
                                disabled={guardando}
                            >
                                {guardando ? (
                                    <>
                                        <span className="spinner-border spinner-border-sm mr-2" role="status" aria-hidden="true" />
                                        Guardando...
                                    </>
                                ) : (
                                    <>
                                        <i className="fas fa-save mr-1" aria-hidden="true" /> Guardar
                                    </>
                                )}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </>
    )
}