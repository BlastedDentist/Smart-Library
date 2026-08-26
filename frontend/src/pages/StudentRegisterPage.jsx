import { useState } from 'react'
import { Link, useNavigate, useSearchParams } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import './FormPage.css'

export default function StudentRegisterPage() {
  const [fullName, setFullName] = useState('')
  const [indexNumber, setIndexNumber] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)
  const { registerAsStudent } = useAuth()
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  // Set by the /scan page's "Register first" link, so a student who scans
  // the kiosk QR without an account yet lands back on /scan (with their
  // token still attached) right after creating one, instead of being
  // dropped on the generic dashboard.
  const nextPath = searchParams.get('next')

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError(null)

    if (!fullName.trim() || !indexNumber.trim() || !password) {
      setError('All fields are required.')
      return
    }
    if (password.length < 6) {
      setError('Password must be at least 6 characters.')
      return
    }
    if (password !== confirmPassword) {
      setError('Passwords do not match.')
      return
    }

    setSubmitting(true)
    try {
      await registerAsStudent(fullName.trim(), indexNumber.trim(), password)
      navigate(nextPath || '/dashboard')
    } catch (err) {
      setError(err.message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="form-page">
      <p className="form-page__eyebrow">Student</p>
      <h1 className="form-page__title">Create your account</h1>
      <p className="form-page__subtitle">
        This is a website account for checking seat availability — it's separate from being signed in/out of the
        library itself, which the librarian handles at the desk.
      </p>

      <form className="card form-page__card" onSubmit={handleSubmit}>
        <div className="form-field">
          <label htmlFor="fullName">Full name</label>
          <input id="fullName" type="text" value={fullName} onChange={(e) => setFullName(e.target.value)} autoComplete="name" />
        </div>

        <div className="form-field">
          <label htmlFor="indexNumber">Index number</label>
          <input
            id="indexNumber"
            type="text"
            className="mono"
            value={indexNumber}
            onChange={(e) => setIndexNumber(e.target.value)}
          />
        </div>

        <div className="form-field">
          <label htmlFor="password">Password</label>
          <input id="password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} autoComplete="new-password" />
        </div>

        <div className="form-field">
          <label htmlFor="confirmPassword">Confirm password</label>
          <input
            id="confirmPassword"
            type="password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            autoComplete="new-password"
          />
        </div>

        {error && <div className="alert alert-error" style={{ marginBottom: 'var(--space-4)' }}>{error}</div>}

        <button type="submit" className="btn btn-accent" disabled={submitting} style={{ width: '100%' }}>
          {submitting ? 'Creating account…' : 'Create account'}
        </button>
      </form>

      <p className="form-page__footer-link">
        Already have an account? <Link to="/login">Log in</Link>
      </p>
    </div>
  )
}
