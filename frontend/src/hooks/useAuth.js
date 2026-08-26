import { useContext } from 'react'
import { AuthContext } from '../context/AuthContext'

// Thin accessor for the single shared auth state in AuthContext. Every
// component gets the SAME role/displayName/isAuthenticated values and
// re-renders together when login()/logout() run — no more stale copies.
export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within an <AuthProvider>')
  }
  return ctx
}
