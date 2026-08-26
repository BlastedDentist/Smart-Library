import { Routes, Route } from 'react-router-dom'
import MainLayout from './layouts/MainLayout'
import Landing from './pages/Landing'
import LoginPage from './pages/LoginPage'
import StudentRegisterPage from './pages/StudentRegisterPage'
import Dashboard from './pages/Dashboard'
import Analytics from './pages/Analytics'
import AdminPanel from './pages/AdminPanel'
import BookManagement from './pages/BookManagement'
import Kiosk from './pages/Kiosk'
import Scan from './pages/Scan'
import ProtectedRoute from './components/ProtectedRoute'

export default function App() {
  return (
    <MainLayout>
      <Routes>
        {/* Public: choose a role, then log in */}
        <Route path="/" element={<Landing />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<StudentRegisterPage />} />

        {/* Public: reached only by scanning the kiosk's QR code — handles
            its own login state internally rather than using ProtectedRoute,
            so the ?token= param survives an in-page login/register. */}
        <Route path="/scan" element={<Scan />} />

        {/* Any logged-in user (Admin or Student) — view-only occupancy info */}
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          }
        />
        <Route
          path="/analytics"
          element={
            <ProtectedRoute>
              <Analytics />
            </ProtectedRoute>
          }
        />

        {/* Librarian only — physically checks students in/out, manages capacity */}
        <Route
          path="/admin"
          element={
            <ProtectedRoute role="Admin">
              <AdminPanel />
            </ProtectedRoute>
          }
        />
        <Route
          path="/admin/books"
          element={
            <ProtectedRoute role="Admin">
              <BookManagement />
            </ProtectedRoute>
          }
        />
        <Route
          path="/kiosk"
          element={
            <ProtectedRoute role="Admin">
              <Kiosk />
            </ProtectedRoute>
          }
        />
      </Routes>
    </MainLayout>
  )
}
