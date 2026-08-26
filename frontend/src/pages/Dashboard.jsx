import { useOccupancy } from '../hooks/useOccupancy'
import { useAuth } from '../hooks/useAuth'
import SeatGrid from '../components/SeatGrid'
import StatCard from '../components/StatCard'
import StatusBadge from '../components/StatusBadge'
import LoadingSpinner from '../components/LoadingSpinner'
import './Dashboard.css'

export default function Dashboard() {
  const { data, loading, error } = useOccupancy()
  const { displayName, isStudent } = useAuth()

  return (
    <div className="dashboard">
      <div className="dashboard__hero">
        <p className="dashboard__eyebrow">{isStudent ? `Hi, ${displayName?.split(' ')[0]}` : 'Right now'}</p>
        <h1 className="dashboard__title">
          {loading ? 'Checking the room…' : data?.libraryStatus ?? 'Library status unavailable'}
        </h1>
        <p className="dashboard__subtitle">
          Every dot below is one seat. Updates automatically every 15 seconds.
        </p>
      </div>

      {error && <div className="alert alert-error" style={{ marginBottom: 'var(--space-5)' }}>{error}</div>}

      {loading && !data && <LoadingSpinner label="Loading current occupancy…" />}

      {data && (
        <>
          <div className="card dashboard__seat-card">
            <div className="dashboard__seat-card-header">
              <div>
                <span className="mono dashboard__occupancy-figure">
                  {data.currentOccupancy}/{data.maxCapacity}
                </span>
                <span className="dashboard__occupancy-label">seats occupied</span>
              </div>
              <StatusBadge status={data.libraryStatus} />
            </div>
            <SeatGrid occupied={data.currentOccupancy} capacity={data.maxCapacity} />
          </div>

          <div className="dashboard__stats">
            <StatCard label="Available seats" value={data.availableSeats} tone="sage" />
            <StatCard label="Occupancy" value={`${data.occupancyPercentage}%`} tone="amber" />
            <StatCard label="Capacity" value={data.maxCapacity} />
          </div>
        </>
      )}
    </div>
  )
}
