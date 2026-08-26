import Navbar from '../components/Navbar'

export default function MainLayout({ children }) {
  return (
    <div>
      <Navbar />
      <main className="container" style={{ paddingTop: 'var(--space-7)', paddingBottom: 'var(--space-8)' }}>
        {children}
      </main>
    </div>
  )
}
