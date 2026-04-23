import { useState, useEffect, useCallback, useRef } from 'react'

export function useMensajeFlash() {
    const [alerta, setAlerta] = useState(() => {
        const tipo = sessionStorage.getItem('alerta_tipo')
        const msg = sessionStorage.getItem('alerta_msg')
        // Limpiar siempre en el mount, haya datos o no,
        // para evitar residuos de ejecuciones anteriores
        sessionStorage.removeItem('alerta_tipo')
        sessionStorage.removeItem('alerta_msg')
        return tipo && msg ? { tipo, msg } : null
    })

    const timerRef = useRef(null)

    useEffect(() => {
        clearTimeout(timerRef.current)
        if (!alerta) return
        timerRef.current = setTimeout(() => setAlerta(null), 5000)
        return () => clearTimeout(timerRef.current)
    }, [alerta])

    const cerrar = useCallback(() => {
        clearTimeout(timerRef.current)
        setAlerta(null)
    }, [])

    const mostrar = useCallback((tipo, msg) => {
        setAlerta({ tipo, msg })
    }, [])

    const guardar = useCallback((tipo, msg) => {
        sessionStorage.setItem('alerta_tipo', tipo)
        sessionStorage.setItem('alerta_msg', msg)
    }, [])

    return { alerta, cerrar, mostrar, guardar }
}