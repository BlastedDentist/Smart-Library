import { Navigate } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'

// Guards routes client-side. This is a UX convenience, not the real
// security boundary — the API's [Authorize] / [Authorize(Roles = "Admin")]
// attributes are what actually enforce access; this just avoids showing a
// page shell to someone who isn't allowed to see it.
//
// - No `role` prop: any logged-in user (Admin or Student) may pass.
// - `role="Admin"`: only the librarian may pass; a logged-in student gets
//   bounced to their own dashboard instead of the generic login screen.
export default function ProtectedRoute({ role, children }) {
  const { isAuthenticated, role: currentRole } = useAuth()

  if (!isAuthenticated) {
    return <Navigate to="/" replace />
  }

  if (role && currentRole !== role) {
    return <Navigate to="/dashboard" replace />
  }

  return children
}
