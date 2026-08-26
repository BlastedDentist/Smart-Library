import { useEffect, useState } from 'react'
import { Line, Bar } from 'react-chartjs-2'
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  BarElement,
  Tooltip,
  Filler,
} from 'chart.js'
import { getAnalyticsSummary } from '../services/api'
import LoadingSpinner from '../components/LoadingSpinner'
import StatCard from '../components/StatCard'
import './Analytics.css'

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, BarElement, Tooltip, Filler)

// Shared chart look so every chart on this page reads as part of one system
// rather than each pulling in Chart.js defaults.
const chartFont = { family: 'Inter', size: 12 }
const gridColor = '#E4EDE7'

const lineOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false }, tooltip: { backgroundColor: '#16241C', titleColor: '#FFFFFF', bodyColor: '#FFFFFF', padding: 10, cornerRadius: 8 } },
  scales: {
    x: { grid: { display: false }, ticks: { font: chartFont, color: '#74897C' } },
    y: { beginAtZero: true, grid: { color: gridColor }, ticks: { font: chartFont, color: '#74897C', precision: 0 } },
  },
}

const barOptions = { ...lineOptions }

export default function Analytics() {
  const [data, setData] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    getAnalyticsSummary()
      .then(setData)
      .catch((err) => setError(err.message))
  }, [])

  if (error) return <div className="alert alert-error">{error}</div>
  if (!data) return <LoadingSpinner label="Crunching attendance history…" />

  const dailyData = {
    labels: data.dailyOccupancy.map((p) => p.label),
    datasets: [
      {
        label: 'Visits',
        data: data.dailyOccupancy.map((p) => p.visitCount),
        borderColor: '#217A3C',
        backgroundColor: 'rgba(33, 122, 60, 0.1)',
        fill: true,
        tension: 0.35,
        pointRadius: 3,
        pointBackgroundColor: '#217A3C',
      },
    ],
  }

  const weeklyData = {
    labels: data.weeklyOccupancy.map((p) => p.label),
    datasets: [
      {
        label: 'Visits',
        data: data.weeklyOccupancy.map((p) => p.visitCount),
        backgroundColor: '#2F9E5C',
        borderRadius: 6,
      },
    ],
  }

  const hourlyData = {
    labels: data.hourlyLoad.map((h) => `${h.hour}:00`),
    datasets: [
      {
        label: 'Visits',
        data: data.hourlyLoad.map((h) => h.visitCount),
        backgroundColor: data.hourlyLoad.map((h) =>
          data.peakHours.includes(h.hour) ? '#C0392B' : data.quietHours.includes(h.hour) ? '#2F9E5C' : '#C8880D'
        ),
        borderRadius: 4,
      },
    ],
  }

  return (
    <div className="analytics">
      <p className="form-page__eyebrow">Plan ahead</p>
      <h1 className="analytics__title">Best times to visit</h1>

      <div className="card analytics__callout">
        <p className="analytics__callout-label">Our recommendation</p>
        <p className="analytics__callout-text">{data.bestTimeToVisit}</p>
      </div>

      <div className="dashboard__stats" style={{ margin: '1.5rem 0' }}>
        <StatCard label="Average visit length" value={`${Math.round(data.averageVisitDurationMinutes)} min`} tone="amber" />
        <StatCard
          label="Peak hours"
          value={data.peakHours.map((h) => `${h}:00`).join(', ') || '—'}
          tone="coral"
        />
        <StatCard
          label="Quiet hours"
          value={data.quietHours.map((h) => `${h}:00`).join(', ') || '—'}
          tone="sage"
        />
      </div>

      <section className="card analytics__chart-card">
        <h2 className="analytics__chart-title">Visits by hour of day</h2>
        <p className="analytics__chart-caption">Red = busiest hours · Green = quietest hours</p>
        <div className="analytics__chart-wrap">
          <Bar data={hourlyData} options={barOptions} />
        </div>
      </section>

      <section className="card analytics__chart-card">
        <h2 className="analytics__chart-title">Daily visits, last 14 days</h2>
        <div className="analytics__chart-wrap">
          <Line data={dailyData} options={lineOptions} />
        </div>
      </section>

      <section className="card analytics__chart-card">
        <h2 className="analytics__chart-title">Visits by day of week</h2>
        <div className="analytics__chart-wrap">
          <Bar data={weeklyData} options={barOptions} />
        </div>
      </section>
    </div>
  )
}
