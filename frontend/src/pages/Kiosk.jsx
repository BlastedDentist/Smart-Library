import { useEffect, useRef, useState } from 'react'
import { QRCodeSVG } from 'qrcode.react'
import { getKioskToken } from '../services/api'
import LoadingSpinner from '../components/LoadingSpinner'
import './Kiosk.css'

// Meant to run full-screen on a tablet/monitor at the library entrance,
// left open all day. Fetches a fresh token from the backend on a timer —
// the token itself is short-lived (see QrTokenService on the backend), so
// this page's only job is "always be showing whatever the CURRENT valid
// code is," not to do any validation itself.
export default function Kiosk() {
  const [tokenData, setTokenData] = useState(null)
  const [error, setError] = useState(null)
  const [secondsLeft, setSecondsLeft] = useState(null)
  const intervalRef = useRef(null)
  const countdownRef = useRef(null)

  const scanUrl = tokenData
    ? `${window.location.origin}/scan?token=${encodeURIComponent(tokenData.token)}`
    : null

  const fetchToken = async () => {
    try {
      const result = await getKioskToken()
      setTokenData(result)
      setError(null)
    } catch (err) {
      setError(err.message)
    }
  }

  useEffect(() => {
    fetchToken()
    // Refresh well before the current window actually expires, so the
    // screen is never caught displaying a dead code.
    intervalRef.current = setInterval(fetchToken, 5000)
    return () => clearInterval(intervalRef.current)
  }, [])

  useEffect(() => {
    if (!tokenData) return
    const update = () => {
      const remaining = tokenData.expiresAtUnix - Math.floor(Date.now() / 1000)
      setSecondsLeft(Math.max(remaining, 0))
    }
    update()
    countdownRef.current = setInterval(update, 1000)
    return () => clearInterval(countdownRef.current)
  }, [tokenData])

  if (error) return <div className="alert alert-error">{error}</div>
  if (!tokenData) return <LoadingSpinner label="Starting kiosk…" />

  const progress = secondsLeft !== null ? (secondsLeft / tokenData.windowSeconds) * 100 : 100

  return (
    <div className="kiosk">
      <p className="kiosk__eyebrow">Library check-in</p>
      <h1 className="kiosk__title">Scan to sign in or out</h1>
      <p className="kiosk__subtitle">Open your phone's camera and point it at the code below</p>

      <div className="kiosk__qr-frame">
        <QRCodeSVG value={scanUrl} size={320} bgColor="#FFFFFF" fgColor="#16241C" level="M" />
      </div>

      <div className="kiosk__progress-track">
        <div className="kiosk__progress-fill" style={{ width: `${progress}%` }} />
      </div>
      <p className="kiosk__rotate-note">Code refreshes automatically — no need to do anything</p>
    </div>
  )
}
