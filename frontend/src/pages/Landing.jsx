import { Link, Navigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import './Landing.css'

// The very first thing anyone sees. Two distinct paths on purpose — this is
// the "signing into the website" fork the spec calls for, separate from the
// physical library sign-in/out that only happens at the librarian's desk.
export default function Landing() {
  const { isAuthenticated, isAdmin } = useAuth()

  if (isAuthenticated) {
    return <Navigate to={isAdmin ? '/admin' : '/dashboard'} replace />
  }

  return (
    <div className="landing">
      <p className="form-page__eyebrow">Welcome</p>
      <h1 className="landing__title">Who's signing in?</h1>
      <p className="landing__subtitle">
        Choose your role to continue. Only librarians can check students in or out of the library —
        students use their account to check seat availability from anywhere.
      </p>

      <div className="landing__cards">
        <Link to="/login" className="card landing__card">
          <span className="landing__card-eyebrow">Sign in</span>
          <h2 className="landing__card-title">One login for everyone</h2>
          <p className="landing__card-copy">
            Use your student index number or librarian username to sign in.
          </p>
          <span className="landing__card-cta">Continue to login →</span>
        </Link>
      </div>

      <p className="landing__register">
        New student? <Link to="/register">Create an account</Link>
      </p>
    </div>
  )
}
