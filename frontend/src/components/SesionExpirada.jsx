import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'

const DELAY_REDIRECT_MS = 3000   // 3 s

export default function SesionExpirada() {
    const navigate = useNavigate()
    const timerRef = useRef(null)
    const [visible, setVisible] = useState(false)

    useEffect(() => {
        // Pequeño delay para que el fade-in del modal se vea
        const showId = setTimeout(() => setVisible(true), 50)

        timerRef.current = setTimeout(() => {
            navigate('/login?msg=expirada', { replace: true })
        }, DELAY_REDIRECT_MS)

        return () => {
            clearTimeout(showId)
            clearTimeout(timerRef.current)
        }
    }, [navigate])

    return (
        <>
            {/* Backdrop*/}
            <div
                style={{
                    position: 'fixed',
                    inset: 0,
                    backgroundColor: 'rgba(0,0,0,0.5)',
                    zIndex: 1040,
                    transition: 'opacity 0.3s ease',
                    opacity: visible ? 1 : 0,
                }}
                aria-hidden="true"
            />

            {/* Modal */}
            <div
                role="dialog"
                aria-modal="true"
                aria-labelledby="modalSesionExpiradaLabel"
                style={{
                    position: 'fixed',
                    inset: 0,
                    zIndex: 1050,
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    padding: '1rem',
                    transition: 'opacity 0.3s ease',
                    opacity: visible ? 1 : 0,
                }}
            >
                <div
                    className="modal-dialog modal-dialog-centered"
                    style={{ margin: 0, width: '100%', maxWidth: '500px' }}
                >
                    <div className="modal-content">

                        {/* Header amarillo — idéntico al PHP */}
                        <div className="modal-header bg-warning">
                            <h5 className="modal-title" id="modalSesionExpiradaLabel">
                                <i className="fas fa-clock mr-2" aria-hidden="true" />
                                Sesión Expirada
                            </h5>
                            {/* Sin botón de cierre: data-backdrop="static" en PHP */}
                        </div>

                        {/* Body */}
                        <div className="modal-body">
                            <p className="mb-0">
                                Su sesión ha expirado por inactividad. Será redirigido al inicio de sesión.
                            </p>
                        </div>

                    </div>
                </div>
            </div>
        </>
    )
}