import { useEffect, useState } from 'react'
import {
  getTodayAttendance,
  searchAttendance,
  updateCapacity,
  getStudentDirectory,
  addStudent,
  checkIn,
  checkOut,
} from '../services/api'
import { useOccupancy } from '../hooks/useOccupancy'
import { useAuth } from '../hooks/useAuth'
import StatCard from '../components/StatCard'
import LoadingSpinner from '../components/LoadingSpinner'
import './AdminPanel.css'

export default function AdminPanel() {
  const { displayName } = useAuth()
  const { data: dashboard, refresh: refreshDashboard } = useOccupancy()

  // ---- Today's attendance log ----
  const [records, setRecords] = useState([])
  const [recordsLoading, setRecordsLoading] = useState(true)
  const [recordsError, setRecordsError] = useState(null)
  const [attendanceQuery, setAttendanceQuery] = useState('')

  // ---- Student directory (check students in/out from here) ----
  const [directory, setDirectory] = useState([])
  const [directoryLoading, setDirectoryLoading] = useState(true)
  const [directoryError, setDirectoryError] = useState(null)
  const [directoryQuery, setDirectoryQuery] = useState('')
  const [actionPending, setActionPending] = useState(null) // indexNumber currently being checked in/out

  // ---- Add walk-in student ----
  const [newName, setNewName] = useState('')
  const [newIndex, setNewIndex] = useState('')
  const [addStatus, setAddStatus] = useState(null)
  const [addSubmitting, setAddSubmitting] = useState(false)

  // ---- Capacity ----
  const [capacityInput, setCapacityInput] = useState('')
  const [capacityStatus, setCapacityStatus] = useState(null)

  const loadToday = async () => {
    setRecordsLoading(true)
    try {
      setRecords(await getTodayAttendance())
      setRecordsError(null)
    } catch (err) {
      setRecordsError(err.message)
    } finally {
      setRecordsLoading(false)
    }
  }

  const loadDirectory = async (query = '') => {
    setDirectoryLoading(true)
    try {
      setDirectory(await getStudentDirectory(query))
      setDirectoryError(null)
    } catch (err) {
      setDirectoryError(err.message)
    } finally {
      setDirectoryLoading(false)
    }
  }

  useEffect(() => {
    loadToday()
    loadDirectory()
  }, [])

  useEffect(() => {
    if (dashboard) setCapacityInput(String(dashboard.maxCapacity))
  }, [dashboard])

  const handleAttendanceSearch = async (e) => {
    e.preventDefault()
    setRecordsLoading(true)
    try {
      const result = attendanceQuery.trim() ? await searchAttendance(attendanceQuery.trim()) : await getTodayAttendance()
      setRecords(result)
      setRecordsError(null)
    } catch (err) {
      setRecordsError(err.message)
    } finally {
      setRecordsLoading(false)
    }
  }

  const handleDirectorySearch = async (e) => {
    e.preventDefault()
    await loadDirectory(directoryQuery.trim())
  }

  // Refreshes everything that could have changed after a check-in/out:
  // the directory row's status, today's attendance log, and the live
  // occupancy count on the dashboard.
  const refreshAfterAttendanceChange = async () => {
    await Promise.all([loadDirectory(directoryQuery.trim()), loadToday()])
    refreshDashboard()
  }

  const handleCheckIn = async (indexNumber) => {
    setActionPending(indexNumber)
    setDirectoryError(null)
    try {
      await checkIn(indexNumber)
      await refreshAfterAttendanceChange()
    } catch (err) {
      setDirectoryError(err.message)
    } finally {
      setActionPending(null)
    }
  }

  const handleCheckOut = async (indexNumber) => {
    setActionPending(indexNumber)
    setDirectoryError(null)
    try {
      await checkOut(indexNumber)
      await refreshAfterAttendanceChange()
    } catch (err) {
      setDirectoryError(err.message)
    } finally {
      setActionPending(null)
    }
  }

  const handleAddStudent = async (e) => {
    e.preventDefault()
    setAddStatus(null)
    if (!newName.trim() || !newIndex.trim()) {
      setAddStatus({ type: 'error', message: 'Enter both a name and an index number.' })
      return
    }
    setAddSubmitting(true)
    try {
      await addStudent(newName.trim(), newIndex.trim())
      setAddStatus({ type: 'success', message: `${newName.trim()} added to the directory.` })
      setNewName('')
      setNewIndex('')
      await loadDirectory(directoryQuery.trim())
    } catch (err) {
      setAddStatus({ type: 'error', message: err.message })
    } finally {
      setAddSubmitting(false)
    }
  }

  const handleCapacitySubmit = async (e) => {
    e.preventDefault()
    setCapacityStatus(null)
    const value = Number(capacityInput)
    if (!Number.isInteger(value) || value <= 0) {
      setCapacityStatus({ type: 'error', message: 'Enter a whole number greater than zero.' })
      return
    }
    try {
      await updateCapacity(value)
      setCapacityStatus({ type: 'success', message: 'Capacity updated.' })
      refreshDashboard()
    } catch (err) {
      setCapacityStatus({ type: 'error', message: err.message })
    }
  }

  const currentlyInside = records.filter((r) => r.status === 'Inside').length

  return (
    <div className="admin">
      <div className="admin__header">
        <div>
          <p className="form-page__eyebrow">Librarian</p>
          <h1 className="admin__title">Attendance &amp; reports</h1>
          <p className="form-page__subtitle">Signed in as {displayName}</p>
        </div>
      </div>

      {dashboard && (
        <div className="dashboard__stats admin__stats">
          <StatCard label="Currently inside" value={dashboard.currentOccupancy} tone="amber" />
          <StatCard label="Available seats" value={dashboard.availableSeats} tone="sage" />
          <StatCard label="Today's total sign-ins" value={records.length} />
        </div>
      )}

      {/* ---- Student directory: check students in/out from here ---- */}
      <section className="card admin__table-card">
        <div className="admin__table-header">
          <h2 className="admin__section-title">Check students in / out</h2>
          <form className="admin__search" onSubmit={handleDirectorySearch}>
            <input
              type="text"
              placeholder="Search directory…"
              value={directoryQuery}
              onChange={(e) => setDirectoryQuery(e.target.value)}
            />
            <button type="submit" className="btn btn-outline">Search</button>
          </form>
        </div>

        {directoryError && <div className="alert alert-error" style={{ marginBottom: 'var(--space-3)' }}>{directoryError}</div>}
        {directoryLoading && <LoadingSpinner label="Loading directory…" />}

        {!directoryLoading && directory.length === 0 && (
          <p className="admin__empty">No students match yet. Add one below.</p>
        )}

        {!directoryLoading && directory.length > 0 && (
          <div className="admin__table-wrap">
            <table className="admin__table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Index number</th>
                  <th>Website account</th>
                  <th>Status</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {directory.map((s) => (
                  <tr key={s.id}>
                    <td>{s.fullName}</td>
                    <td className="mono">{s.indexNumber}</td>
                    <td>
                      <span className={`badge ${s.hasWebsiteAccount ? 'badge-sage' : 'badge-amber'}`}>
                        {s.hasWebsiteAccount ? 'Registered' : 'Walk-in only'}
                      </span>
                    </td>
                    <td>
                      <span className={`badge ${s.isCurrentlyInside ? 'badge-sage' : 'badge-coral'}`}>
                        {s.isCurrentlyInside ? 'Inside' : 'Not in library'}
                      </span>
                    </td>
                    <td>
                      {s.isCurrentlyInside ? (
                        <button
                          className="btn btn-outline admin__row-btn"
                          disabled={actionPending === s.indexNumber}
                          onClick={() => handleCheckOut(s.indexNumber)}
                        >
                          {actionPending === s.indexNumber ? 'Working…' : 'Check out'}
                        </button>
                      ) : (
                        <button
                          className="btn btn-accent admin__row-btn"
                          disabled={actionPending === s.indexNumber}
                          onClick={() => handleCheckIn(s.indexNumber)}
                        >
                          {actionPending === s.indexNumber ? 'Working…' : 'Check in'}
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>

      {/* ---- Add a walk-in student who hasn't registered online ---- */}
      <section className="card admin__add-card">
        <h2 className="admin__section-title">Add a walk-in student</h2>
        <p className="admin__section-caption">
          For students who haven't created a website account yet — they can still be checked in/out right away.
        </p>
        <form className="admin__add-form" onSubmit={handleAddStudent}>
          <div className="form-field" style={{ marginBottom: 0, flex: 1 }}>
            <label htmlFor="newName">Full name</label>
            <input id="newName" type="text" value={newName} onChange={(e) => setNewName(e.target.value)} />
          </div>
          <div className="form-field" style={{ marginBottom: 0, flex: 1 }}>
            <label htmlFor="newIndex">Index number</label>
            <input id="newIndex" type="text" className="mono" value={newIndex} onChange={(e) => setNewIndex(e.target.value)} />
          </div>
          <button type="submit" className="btn btn-primary" disabled={addSubmitting}>
            {addSubmitting ? 'Adding…' : 'Add student'}
          </button>
        </form>
        {addStatus && (
          <div className={`alert ${addStatus.type === 'error' ? 'alert-error' : 'alert-success'}`} style={{ marginTop: 'var(--space-3)' }}>
            {addStatus.message}
          </div>
        )}
      </section>

      {/* ---- Manage capacity ---- */}
      <section className="card admin__capacity-card">
        <h2 className="admin__section-title">Manage library capacity</h2>
        <form className="admin__capacity-form" onSubmit={handleCapacitySubmit}>
          <div className="form-field" style={{ marginBottom: 0, flex: 1 }}>
            <label htmlFor="capacity">Maximum capacity (seats)</label>
            <input
              id="capacity"
              type="number"
              min="1"
              value={capacityInput}
              onChange={(e) => setCapacityInput(e.target.value)}
            />
          </div>
          <button type="submit" className="btn btn-accent">Save</button>
        </form>
        {capacityStatus && (
          <div className={`alert ${capacityStatus.type === 'error' ? 'alert-error' : 'alert-success'}`} style={{ marginTop: 'var(--space-3)' }}>
            {capacityStatus.message}
          </div>
        )}
      </section>

      {/* ---- Today's attendance log ---- */}
      <section className="card admin__table-card">
        <div className="admin__table-header">
          <h2 className="admin__section-title">Attendance history ({currentlyInside} inside now)</h2>
          <form className="admin__search" onSubmit={handleAttendanceSearch}>
            <input
              type="text"
              placeholder="Search name or index number…"
              value={attendanceQuery}
              onChange={(e) => setAttendanceQuery(e.target.value)}
            />
            <button type="submit" className="btn btn-outline">Search</button>
          </form>
        </div>

        {recordsError && <div className="alert alert-error">{recordsError}</div>}
        {recordsLoading && <LoadingSpinner label="Loading attendance…" />}

        {!recordsLoading && records.length === 0 && (
          <p className="admin__empty">No attendance records match yet.</p>
        )}

        {!recordsLoading && records.length > 0 && (
          <div className="admin__table-wrap">
            <table className="admin__table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Index number</th>
                  <th>Checked in</th>
                  <th>Checked out</th>
                  <th>Duration</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {records.map((r) => (
                  <tr key={r.id}>
                    <td>{r.fullName}</td>
                    <td className="mono">{r.indexNumber}</td>
                    <td>{new Date(r.checkInTime).toLocaleString()}</td>
                    <td>{r.checkOutTime ? new Date(r.checkOutTime).toLocaleString() : '—'}</td>
                    <td>{r.durationMinutes ? `${Math.round(r.durationMinutes)} min` : '—'}</td>
                    <td>
                      <span className={`badge ${r.status === 'Inside' ? 'badge-sage' : 'badge-amber'}`}>
                        {r.status === 'Inside' ? 'Inside' : 'Checked out'}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </section>
    </div>
  )
}
