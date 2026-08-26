import { useEffect, useState } from 'react'
import { useSearchParams, Link } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { submitScan } from '../services/api'
import LoadingSpinner from '../components/LoadingSpinner'
import './Scan.css'

// Where a student lands right after their phone camera scans the kiosk's QR
// code. Deliberately does NOT use the generic ProtectedRoute — that would
// redirect an unauthenticated visitor to the landing page and lose the
// `token` query param in the process. Instead this page handles "not logged
// in yet" itself, with a login form that keeps the token in hand and
// auto-submits the scan the moment login succeeds.
export default function Scan() {
  const [searchParams] = useSearchParams()
  const token = searchParams.get('token')
  const { isAuthenticated, isAdmin, isStudent, login } = useAuth()

  const [result, setResult] = useState(null)
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)
  const [attempted, setAttempted] = useState(false)

  const [identifier, setIdentifier] = useState('')
  const [password, setPassword] = useState('')
  const [loginError, setLoginError] = useState(null)
  const [loginSubmitting, setLoginSubmitting] = useState(false)

  const doScan = async () => {
    setSubmitting(true)
    setError(null)
    try {
      const data = await submitScan(token)
      setResult(data)
    } catch (err) {
      setError(err.message)
    } finally {
      setSubmitting(false)
      setAttempted(true)
    }
  }

  // Auto-submit the instant we know we're looking at a logged-in student —
  // no extra tap needed, matching the "tap and go" feel of a real reader.
  useEffect(() => {
    if (isStudent && token && !attempted && !submitting) {
      doScan()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isStudent, token])

  const handleLogin = async (e) => {
    e.preventDefault()
    setLoginError(null)
    setLoginSubmitting(true)
    try {
      await login(identifier.trim(), password)
      // doScan fires automatically via the effect above once isStudent flips true
    } catch (err) {
      setLoginError(err.message)
    } finally {
      setLoginSubmitting(false)
    }
  }

  if (!token) {
    return (
      <div className="scan">
        <div className="card scan__card">
          <p className="scan__status-icon">?</p>
          <h1 className="scan__title">No code found</h1>
          <p className="scan__message">This page is meant to be opened by scanning the QR code on the library kiosk screen.</p>
        </div>
      </div>
    )
  }

  if (isAdmin) {
    return (
      <div className="scan">
        <div className="card scan__card">
          <p className="scan__status-icon">!</p>
          <h1 className="scan__title">Wrong account</h1>
          <p className="scan__message">You're logged in as the librarian. Scan this with your own student account instead.</p>
        </div>
      </div>
    )
  }

  if (!isAuthenticated) {
    return (
      <div className="scan">
        <div className="card scan__card scan__card--form">
          <p className="form-page__eyebrow">Almost there</p>
          <h1 className="scan__title">Log in to check in</h1>
          <p className="scan__message">Your code is ready — just sign in to your student account to complete it.</p>

          <form onSubmit={handleLogin}>
            <div className="form-field">
              <label htmlFor="identifier">Index number</label>
              <input id="identifier" type="text" className="mono" value={identifier} onChange={(e) => setIdentifier(e.target.value)} autoComplete="username" />
            </div>
            <div className="form-field">
              <label htmlFor="password">Password</label>
              <input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} autoComplete="current-password" />
            </div>
            {loginError && <div className="alert alert-error" style={{ marginBottom: 'var(--space-4)' }}>{loginError}</div>}
            <button type="submit" className="btn btn-accent" disabled={loginSubmitting} style={{ width: '100%' }}>
              {loginSubmitting ? 'Signing in…' : 'Sign in & check in'}
            </button>
          </form>

          <p className="form-page__footer-link">
            No account yet? <Link to={`/register?next=${encodeURIComponent(`/scan?token=${token}`)}`}>Register first</Link>
          </p>
        </div>
      </div>
    )
  }

  if (submitting) {
    return (
      <div className="scan">
        <LoadingSpinner label="Checking you in…" />
      </div>
    )
  }

  if (error) {
    return (
      <div className="scan">
        <div className="card scan__card">
          <p className="scan__status-icon scan__status-icon--error">!</p>
          <h1 className="scan__title">That didn't work</h1>
          <p className="scan__message">{error}</p>
          {error.includes('expired') && (
            <p className="scan__hint">Ask the librarian to check the kiosk screen shows a fresh code, then scan again.</p>
          )}
        </div>
      </div>
    )
  }

  if (result) {
    const isCheckIn = result.action === 'CheckedIn'
    return (
      <div className="scan">
        <div className={`card scan__card scan__card--${isCheckIn ? 'in' : 'out'}`}>
          <p className="scan__status-icon">{isCheckIn ? '✓' : '👋'}</p>
          <h1 className="scan__title">{isCheckIn ? `Welcome in, ${result.fullName.split(' ')[0]}` : `See you later, ${result.fullName.split(' ')[0]}`}</h1>
          <p className="scan__message">
            {isCheckIn
              ? `Signed in at ${new Date(result.timestamp).toLocaleTimeString()}.`
              : `You were here for ${Math.round(result.durationMinutes ?? 0)} minutes.`}
          </p>
          <Link to="/dashboard" className="btn btn-primary" style={{ marginTop: 'var(--space-4)' }}>View library occupancy</Link>
        </div>
      </div>
    )
  }

  return null
}
