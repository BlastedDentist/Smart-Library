import { NavLink, useNavigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import './Navbar.css'

export default function Navbar() {
  const { isAuthenticated, isAdmin, displayName, logout } = useAuth()
  const navigate = useNavigate()

  const handleLogout = () => {
    logout()
    navigate('/')
  }

  return (
    <header className="navbar">
      <div className="container navbar__inner">
        <NavLink to={isAuthenticated ? (isAdmin ? '/admin' : '/dashboard') : '/'} className="navbar__brand">
          <span className="navbar__brand-mark" aria-hidden="true" />
          UMaT Smart Library
        </NavLink>

        {isAuthenticated ? (
          <nav className="navbar__links">
            {!isAdmin && (
              <>
                <NavLink to="/dashboard" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
                  Dashboard
                </NavLink>
                <NavLink to="/analytics" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
                  Best Times
                </NavLink>
              </>
            )}
            {isAdmin && (
              <>
                <NavLink to="/admin" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
                  Attendance
                </NavLink>
                <NavLink to="/admin/books" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
                  Books
                </NavLink>
                <NavLink to="/kiosk" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
                  Kiosk
                </NavLink>
                <NavLink to="/dashboard" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
                  Dashboard
                </NavLink>
                <NavLink to="/analytics" className={({ isActive }) => `navbar__link ${isActive ? 'navbar__link--active' : ''}`}>
                  Analytics
                </NavLink>
              </>
            )}
            <span className="navbar__username">{displayName}</span>
            <button className="navbar__logout" onClick={handleLogout}>Log out</button>
          </nav>
        ) : (
          <nav className="navbar__links">
            <NavLink to="/login" className="navbar__link">Sign in</NavLink>
          </nav>
        )}
      </div>
    </header>
  )
}
