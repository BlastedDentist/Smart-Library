import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import './FormPage.css'

export default function LoginPage() {
  const [identifier, setIdentifier] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState(null)
  const [submitting, setSubmitting] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError(null)
    setSubmitting(true)

    try {
      const result = await login(identifier.trim(), password)
      navigate(result.role === 'Admin' ? '/admin' : '/dashboard')
    } catch (err) {
      setError(err.message)
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="form-page">
      <p className="form-page__eyebrow">Welcome back</p>
      <h1 className="form-page__title">Sign in</h1>
      <p className="form-page__subtitle">
        Use your librarian username or student index number to sign in.
      </p>

      <form className="card form-page__card" onSubmit={handleSubmit}>
        <div className="form-field">
          <label htmlFor="identifier">Username or index number</label>
          <input
            id="identifier"
            type="text"
            className="mono"
            value={identifier}
            onChange={(e) => setIdentifier(e.target.value)}
            autoComplete="username"
            placeholder="e.g. admin or 202300123"
          />
        </div>

        <div className="form-field">
          <label htmlFor="password">Password</label>
          <input
            id="password"
            type="password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
          />
        </div>

        {error && <div className="alert alert-error" style={{ marginBottom: 'var(--space-4)' }}>{error}</div>}

        <button type="submit" className="btn btn-accent" disabled={submitting} style={{ width: '100%' }}>
          {submitting ? 'Signing in…' : 'Sign in'}
        </button>
      </form>

      <p className="form-page__footer-link">
        New student? <a href="/register">Create an account</a>
      </p>
    </div>
  )
}
