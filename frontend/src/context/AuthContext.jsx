import { createContext, useCallback, useState } from 'react'
import { login as apiLogin, studentRegister as apiStudentRegister } from '../services/api'

const TOKEN_KEY = 'smartlibrary_token'
const ROLE_KEY = 'smartlibrary_role'         // "Admin" | "Student"
const NAME_KEY = 'smartlibrary_display_name'

// eslint-disable-next-line react-refresh/only-export-components
export const AuthContext = createContext(null)

// Single shared source of truth for auth state. Every component that needs
// to know "who's logged in" reads from THIS one instance via useAuth()
// (see hooks/useAuth.js), instead of each maintaining its own local copy.
//
// Why this matters: Navbar lives in MainLayout and mounts once, for the
// whole lifetime of the SPA. If auth state lived in a plain (non-context)
// hook, Navbar's copy of `role` would only ever be set at that first mount
// and at whatever point Navbar's own logout() call ran — it would never
// find out about a login that happened from a different component (e.g.
// LoginPage), because a plain hook gives every caller an independent
// useState. A Context Provider fixes this: there's exactly one `role`
// value, and every consumer re-renders when it changes.
export function AuthProvider({ children }) {
  const [role, setRole] = useState(() => localStorage.getItem(ROLE_KEY))
  const [displayName, setDisplayName] = useState(() => localStorage.getItem(NAME_KEY))
  const [isAuthenticated, setIsAuthenticated] = useState(() => Boolean(localStorage.getItem(TOKEN_KEY)))

  const persistSession = (result) => {
    localStorage.setItem(TOKEN_KEY, result.token)
    localStorage.setItem(ROLE_KEY, result.role)
    localStorage.setItem(NAME_KEY, result.displayName)
    setRole(result.role)
    setDisplayName(result.displayName)
    setIsAuthenticated(true)
  }

  const login = useCallback(async (identifier, password) => {
    const result = await apiLogin(identifier, password)
    persistSession(result)
    return result
  }, [])

  const registerAsStudent = useCallback(async (fullName, indexNumber, password) => {
    const result = await apiStudentRegister(fullName, indexNumber, password)
    persistSession(result)
    return result
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(ROLE_KEY)
    localStorage.removeItem(NAME_KEY)
    setRole(null)
    setDisplayName(null)
    setIsAuthenticated(false)
  }, [])

  const value = {
    isAuthenticated,
    isAdmin: role === 'Admin',
    isStudent: role === 'Student',
    role,
    displayName,
    login,
    registerAsStudent,
    logout,
  }

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
