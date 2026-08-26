import './StatCard.css'

// A small, reusable stat display used across the Dashboard and Analytics
// pages. Kept deliberately simple/generic since it's a supporting element,
// not the page's signature piece (that's SeatGrid).
export default function StatCard({ label, value, sublabel, tone = 'ink' }) {
  return (
    <div className="card stat-card">
      <p className="stat-card__label">{label}</p>
      <p className={`stat-card__value stat-card__value--${tone}`}>{value}</p>
      {sublabel && <p className="stat-card__sublabel">{sublabel}</p>}
    </div>
  )
}
