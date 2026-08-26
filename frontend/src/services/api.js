import axios from 'axios'

// Central axios instance. Every request in the app goes through here, so
// the base URL and auth-token attachment are handled in exactly one place.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

// Attach whichever JWT is stored (Admin or Student) to every outgoing
// request. The backend's [Authorize] / [Authorize(Roles = "Admin")]
// attributes decide what each role is actually allowed to do — this is
// just "send the token if we have one".
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('smartlibrary_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Normalize error handling: the backend's ExceptionMiddleware always
// returns { success: false, message }. We surface that message consistently
// so every page can just do `catch (err) { setError(err.message) }`.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const message =
      error.response?.data?.message ||
      error.message ||
      'Something went wrong. Please try again.'
    return Promise.reject(new Error(message))
  }
)

// ---- Auth ----
export const login = (identifier, password) =>
  api.post('/auth/login', { identifier, password }).then((r) => r.data.data)

export const studentRegister = (fullName, indexNumber, password) =>
  api.post('/auth/student/register', { fullName, indexNumber, password }).then((r) => r.data.data)

// ---- Attendance (librarian-only actions) ----
export const checkIn = (indexNumber) =>
  api.post('/attendance/check-in', { indexNumber }).then((r) => r.data.data)

export const checkOut = (indexNumber) =>
  api.post('/attendance/check-out', { indexNumber }).then((r) => r.data.data)

export const getTodayAttendance = () =>
  api.get('/attendance/today').then((r) => r.data.data)

export const searchAttendance = (query) =>
  api.get('/attendance/search', { params: { query } }).then((r) => r.data.data)

// ---- Dashboard (any logged-in user) ----
export const getDashboard = () =>
  api.get('/dashboard').then((r) => r.data.data)

export const updateCapacity = (maxCapacity) =>
  api.put('/dashboard/capacity', { maxCapacity }).then((r) => r.data)

// ---- Analytics (any logged-in user) ----
export const getAnalyticsSummary = () =>
  api.get('/analytics/summary').then((r) => r.data.data)

// ---- Student directory (librarian-only) ----
export const getStudentDirectory = (query = '') =>
  api.get('/student/directory', { params: { query } }).then((r) => r.data.data)

export const addStudent = (fullName, indexNumber) =>
  api.post('/student', { fullName, indexNumber }).then((r) => r.data.data)

// ---- Book catalog (librarian-only) ----
export const getBooks = (query = '') =>
  api.get('/books', { params: { query } }).then((r) => r.data.data)

export const addBook = (payload) =>
  api.post('/books', payload).then((r) => r.data.data)

export const updateBook = (id, payload) =>
  api.put(`/books/${id}`, payload).then((r) => r.data.data)

export const deleteBook = (id) =>
  api.delete(`/books/${id}`).then((r) => r.data)

// ---- Book loans (librarian authorizes borrow/return) ----
export const borrowBook = (bookId, indexNumber) =>
  api.post('/loans/borrow', { bookId, indexNumber }).then((r) => r.data.data)

export const returnLoan = (loanId) =>
  api.post('/loans/return', { loanId }).then((r) => r.data.data)

export const getLoansForBook = (bookId) =>
  api.get(`/loans/book/${bookId}`).then((r) => r.data.data)

export const getAllLoans = () =>
  api.get('/loans').then((r) => r.data.data)

// ---- Kiosk / QR check-in ----
export const getKioskToken = () =>
  api.get('/kiosk/token').then((r) => r.data.data)

export const submitScan = (token) =>
  api.post('/kiosk/scan', { token }).then((r) => r.data.data)

export default api
