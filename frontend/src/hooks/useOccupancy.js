import { useEffect, useState, useCallback } from 'react'
import { getDashboard } from '../services/api'

// Polls the dashboard endpoint every `intervalMs` so the occupancy shown to
// students stays reasonably fresh without needing a websocket/SignalR
// connection yet (that's flagged as a future expansion in the project spec —
// this hook is the natural place to swap polling for a live subscription
// later, since every page already consumes it the same way).
export function useOccupancy(intervalMs = 15000) {
  const [data, setData] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)

  const refresh = useCallback(async () => {
    try {
      const result = await getDashboard()
      setData(result)
      setError(null)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    refresh()
    const id = setInterval(refresh, intervalMs)
    return () => clearInterval(id)
  }, [refresh, intervalMs])

  return { data, loading, error, refresh }
}
