import './SeatGrid.css'

// The signature visual element of the app: occupancy rendered as an actual
// grid of seats rather than an abstract gauge or progress ring. Each dot is
// one seat; filled amber dots are occupied, outlined dots are free. It's a
// direct, literal representation of "how full is the room right now" —
// which is exactly the question a student opening this page wants answered
// in under a second.
export default function SeatGrid({ occupied, capacity }) {
  const safeCapacity = Math.max(capacity, 1)
  const seats = Array.from({ length: safeCapacity }, (_, i) => i < occupied)

  return (
    <div className="seat-grid" role="img" aria-label={`${occupied} of ${safeCapacity} seats occupied`}>
      {seats.map((isOccupied, i) => (
        <span
          key={i}
          className={`seat-dot ${isOccupied ? 'seat-dot--occupied' : ''}`}
          style={{ transitionDelay: `${Math.min(i * 4, 400)}ms` }}
        />
      ))}
    </div>
  )
}
