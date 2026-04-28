import { useState, useEffect, useCallback, useRef, useMemo } from 'react'
import { useNavigate, useLocation } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import '../styles/sistema.css'

const PROFILE_IMG = `${import.meta.env.BASE_URL}undraw_profile.svg`

// Determina si la ruta es interna (React Router)
// Todas las pantallas 'RT' tienen rutas que empiezan con '/'
// y no contienen extensión .php, .html
function esRutaInterna(ruta) {
    if (typeof ruta !== 'string' || !ruta.startsWith('/')) return false
    // Excluir rutas con extensión de archivo
    return !/\.\w{2,4}$/.test(ruta)
}

// Componente principal
export default function Dashboard({ children }) {
    const { user, cerrarSesion } = useAuth()
    const navigate = useNavigate()
    const location = useLocation()

    const [cargandoLogout, setCargandoLogout] = useState(false)
    const [toggled, setToggled] = useState(() => window.innerWidth < 480)
    const [dropdownOpen, setDropdownOpen] = useState(false)
    const [alertaSinPermiso, setAlertaSinPermiso] = useState(
        location.state?.sinPermiso === true
    )

    const dropdownRef = useRef(null)

    // Limpiar el state de navegación para que no persista al refrescar
    useEffect(() => {
        if (location.state?.sinPermiso) {
            window.history.replaceState({}, '', location.pathname)
        }
    }, [location.state, location.pathname])

    // Auto-cierre de la alerta de sin permiso 5 s
    useEffect(() => {
        if (!alertaSinPermiso) return
        const id = setTimeout(() => setAlertaSinPermiso(false), 5000)
        return () => clearTimeout(id)
    }, [alertaSinPermiso])

    // Colapsar sidebar en pantallas pequeñas
    useEffect(() => {
        function onResize() {
            if (window.innerWidth < 480) setToggled(true)
        }
        window.addEventListener('resize', onResize)
        return () => window.removeEventListener('resize', onResize)
    }, [])

    // Clase sidebar-toggled en <body>
    useEffect(() => {
        document.body.classList.toggle('sidebar-toggled', toggled)
        return () => document.body.classList.remove('sidebar-toggled')
    }, [toggled])

    // ─Cerrar dropdown al hacer clic fuera
    useEffect(() => {
        if (!dropdownOpen) return
        function handleClickOutside(e) {
            if (dropdownRef.current && !dropdownRef.current.contains(e.target))
                setDropdownOpen(false)
        }
        document.addEventListener('mousedown', handleClickOutside)
        return () => document.removeEventListener('mousedown', handleClickOutside)
    }, [dropdownOpen])

    // Logout
    const handleLogout = useCallback(async () => {
        setCargandoLogout(true)
        try {
            await cerrarSesion(true, 'logout')
        } finally {
            setCargandoLogout(false)
        }
    }, [cerrarSesion])

    // Datos del usuario
    const usuarioNombre = user?.usuarioNombre?.trim()
        ? user.usuarioNombre
        : (user?.usuarioId ?? 'Usuario')

    // Secciones del menú generadas dinámicamente desde user.pantallas
    const secciones = useMemo(() => {
        const pantallas = user?.pantallas ?? []
        const map = new Map()

        pantallas.forEach((p) => {
            const sec = p.menuSeccion?.trim() || 'Módulos'
            if (!map.has(sec)) map.set(sec, [])
            map.get(sec).push(p)
        })

        return Array.from(map.entries()).map(([seccion, items]) => ({ seccion, items }))
    }, [user?.pantallas])

    // Helper de ruta activa
    const estaActivo = useCallback((ruta) => {
        if (!esRutaInterna(ruta)) return false
        return location.pathname === ruta || location.pathname.startsWith(ruta + '/')
    }, [location.pathname])

    // Manejador de navegación (interno o externo)
    const irA = useCallback((e, ruta) => {
        e.preventDefault()
        if (!ruta) return
        if (esRutaInterna(ruta)) {
            navigate(ruta)
        } else {
            window.location.href = ruta
        }
    }, [navigate])

    // Render
    return (
        <div id="wrapper">

            {/* Sidebar*/}
            <ul
                className={`navbar-nav bg-gradient-primary sidebar sidebar-dark accordion${toggled ? ' toggled' : ''}`}
                id="accordionSidebar"
            >
                {/* Brand */}
                <a
                    className="sidebar-brand d-flex align-items-center justify-content-center"
                    href="#brand"
                    onClick={(e) => { e.preventDefault(); navigate('/dashboard') }}
                    aria-label="Sistema Contable - Inicio"
                >
                    <div className="sidebar-brand-icon rotate-n-15" aria-hidden="true">
                        <i className="fas fa-calculator" />
                    </div>
                    <div className="sidebar-brand-text mx-3">Sistema Contable</div>
                </a>

                <hr className="sidebar-divider my-0" />

                {/* Ítem fijo: Inicio (siempre visible para cualquier usuario autenticado) */}
                <li className={`nav-item${estaActivo('/dashboard') && !secciones.some(s => s.items.some(p => estaActivo(p.ruta))) ? ' active' : ''}`}>
                    <a
                        className="nav-link"
                        href="#inicio"
                        onClick={(e) => { e.preventDefault(); navigate('/dashboard') }}
                    >
                        <i className="fas fa-fw fa-home" aria-hidden="true" />
                        <span>Inicio</span>
                    </a>
                </li>

                {/* Secciones dinámicas desde la BD*/}
                {secciones.map(({ seccion, items }) => (
                    <div key={seccion}>
                        <hr className="sidebar-divider" />
                        <div className="sidebar-heading">{seccion}</div>

                        {items.map((pantalla) => (
                            <li
                                key={pantalla.ruta}
                                className={`nav-item${estaActivo(pantalla.ruta) ? ' active' : ''}`}
                            >
                                <a
                                    className="nav-link"
                                    href={pantalla.ruta}
                                    onClick={(e) => irA(e, pantalla.ruta)}
                                    title={pantalla.nombre}
                                >
                                    <span className="nav-arrow" aria-hidden="true">›</span>
                                    <span>{pantalla.nombre}</span>
                                </a>
                            </li>
                        ))}
                    </div>
                ))}

                {/* Estado vacíO usuario sin pantallas asignadas */}
                {secciones.length === 0 && (
                    <>
                        <hr className="sidebar-divider" />
                        <div className="sidebar-heading text-center">
                            <small className="text-white-50">Sin módulos asignados</small>
                        </div>
                    </>
                )}

                <hr className="sidebar-divider d-none d-md-block" />

                {/* Botón colapsar */}
                <div className="text-center d-none d-md-inline">
                    <button
                        className="rounded-circle border-0"
                        id="sidebarToggle"
                        onClick={() => setToggled((t) => !t)}
                        aria-label={toggled ? 'Expandir sidebar' : 'Colapsar sidebar'}
                        title={toggled ? 'Expandir' : 'Colapsar'}
                    />
                </div>
            </ul>

            {/* Contenido*/}
            <div id="content-wrapper" className="d-flex flex-column">
                <div id="content">

                    {/* Topbar */}
                    <nav className="navbar navbar-expand navbar-light bg-white topbar mb-4 static-top shadow">

                        <button
                            id="sidebarToggleTop"
                            className="btn btn-link d-md-none rounded-circle mr-3"
                            onClick={() => setToggled((t) => !t)}
                            aria-label="Abrir/cerrar sidebar"
                        >
                            <i className="fa fa-bars" aria-hidden="true" />
                        </button>

                        <ul className="navbar-nav ml-auto">
                            <div className="topbar-divider d-none d-sm-block" />

                            <li
                                className={`nav-item dropdown no-arrow${dropdownOpen ? ' show' : ''}`}
                                ref={dropdownRef}
                            >
                                <button
                                    className="nav-link dropdown-toggle btn btn-link"
                                    id="userDropdown"
                                    aria-haspopup="true"
                                    aria-expanded={dropdownOpen}
                                    onClick={() => setDropdownOpen((o) => !o)}
                                    style={{ background: 'none', border: 'none' }}
                                >
                                    <span className="mr-2 d-none d-lg-inline text-gray-600 small">
                                        {usuarioNombre}
                                    </span>
                                    <img
                                        className="img-profile rounded-circle"
                                        src={PROFILE_IMG}
                                        alt={`Avatar de ${usuarioNombre}`}
                                    />
                                </button>

                                <div
                                    className={`dropdown-menu dropdown-menu-right shadow animated--grow-in${dropdownOpen ? ' show' : ''}`}
                                    aria-labelledby="userDropdown"
                                >
                                    <button
                                        className="dropdown-item"
                                        onClick={handleLogout}
                                        disabled={cargandoLogout}
                                    >
                                        {cargandoLogout ? (
                                            <>
                                                <span
                                                    className="spinner-border spinner-border-sm mr-2"
                                                    role="status"
                                                    aria-hidden="true"
                                                />
                                                Cerrando sesión...
                                            </>
                                        ) : (
                                            <>
                                                <i className="fas fa-sign-out-alt fa-sm fa-fw mr-2 text-gray-400" aria-hidden="true" />
                                                Cerrar Sesión
                                            </>
                                        )}
                                    </button>
                                </div>
                            </li>
                        </ul>
                    </nav>

                    {/* Contenido de la página */}
                    <div className="container-fluid">

                        {/* Alerta de sin permiso (llega desde ProtectedRoute via navigate state) */}
                        {alertaSinPermiso && (
                            <div className="alert alert-warning alert-dismissible fade show" role="alert">
                                <i className="fas fa-exclamation-triangle mr-2" aria-hidden="true" />
                                No tiene permiso para acceder a esa página.
                                <button
                                    type="button"
                                    className="close"
                                    onClick={() => setAlertaSinPermiso(false)}
                                    aria-label="Cerrar alerta"
                                >
                                    <span aria-hidden="true">&times;</span>
                                </button>
                            </div>
                        )}

                        {/* Si hay children (sub-página como TerceroListado), renderizarlos */}
                        {children ?? (
                            <div className="row justify-content-center">
                                <div className="col-xl-8 col-lg-10">
                                    <div className="card shadow mb-4">
                                        <div className="card-body text-center p-5">

                                            <div className="mb-4" aria-hidden="true">
                                                <div
                                                    className="rounded-circle bg-primary d-inline-flex align-items-center justify-content-center"
                                                    style={{ width: 100, height: 100 }}
                                                >
                                                    <i className="fas fa-calculator fa-3x text-white rotate-n-15" />
                                                </div>
                                            </div>

                                            <h2 className="h3 text-gray-800 font-weight-bold mb-3">
                                                Bienvenido, {usuarioNombre}
                                            </h2>

                                        </div>
                                    </div>
                                </div>
                            </div>
                        )}

                    </div>
                </div>

                {/* Footer */}
                <footer className="sticky-footer bg-white">
                    <div className="container my-auto">
                        <div className="copyright text-center my-auto">
                            <span>
                                &copy; {new Date().getFullYear()} — Sistema Contable — Desarrollos Ordenados S.A.
                            </span>
                        </div>
                    </div>
                </footer>
            </div>
        </div>
    )
}