const TONE_MAP = {
  'Space Available': 'badge-sage',
  'Almost Full': 'badge-amber',
  'Library Full': 'badge-coral',
}

// Maps the backend's LibraryStatus string directly to a badge tone, so the
// color vocabulary (sage = fine, amber = caution, coral = full) stays
// consistent everywhere it appears.
export default function StatusBadge({ status }) {
  const toneClass = TONE_MAP[status] || 'badge-sage'
  return <span className={`badge ${toneClass}`}>{status}</span>
}
