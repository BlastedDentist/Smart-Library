import { useEffect, useState } from 'react'
import { getBooks, addBook, updateBook, deleteBook, getLoansForBook, borrowBook, returnLoan } from '../services/api'
import LoadingSpinner from '../components/LoadingSpinner'
import '../pages/AdminPanel.css'
import './BookManagement.css'

const emptyNewBook = { title: '', author: '', isbn: '', category: '', totalCopies: '', description: '' }

// Badge color per loan status — mirrors the sage/amber/coral convention
// already used for attendance ("Inside"/"Checked out") and book stock
// ("available"/"all checked out") elsewhere in this app.
const loanStatusBadge = {
  Borrowed: 'badge-amber',
  Overdue: 'badge-coral',
  Returned: 'badge-sage',
}

export default function BookManagement() {
  const [books, setBooks] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(null)
  const [query, setQuery] = useState('')

  const [newBook, setNewBook] = useState(emptyNewBook)
  const [addStatus, setAddStatus] = useState(null)
  const [addSubmitting, setAddSubmitting] = useState(false)

  const [editingId, setEditingId] = useState(null)
  const [editForm, setEditForm] = useState(null)
  const [editError, setEditError] = useState(null)
  const [editSubmitting, setEditSubmitting] = useState(false)

  // ---- Borrowing activity panel (one book expanded at a time) ----
  const [expandedBookId, setExpandedBookId] = useState(null)
  // Keyed by book id: { items, loading, error }. Loaded lazily, the first
  // time a book's panel is opened, then refreshed after any borrow/return.
  const [loansByBook, setLoansByBook] = useState({})
  // Keyed by book id: the index number currently typed into that book's
  // "authorize a borrow" form.
  const [borrowIndex, setBorrowIndex] = useState({})
  const [borrowStatus, setBorrowStatus] = useState({}) // keyed by book id: { type, message }
  const [borrowSubmittingId, setBorrowSubmittingId] = useState(null)
  const [returnPendingId, setReturnPendingId] = useState(null) // loan id currently being returned

  const load = async (q = query) => {
    setLoading(true)
    try {
      setBooks(await getBooks(q))
      setError(null)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load('')
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const handleSearch = async (e) => {
    e.preventDefault()
    await load(query.trim())
  }

  const handleAddBook = async (e) => {
    e.preventDefault()
    setAddStatus(null)

    const totalCopies = Number(newBook.totalCopies)
    if (!newBook.title.trim() || !newBook.author.trim() || !newBook.isbn.trim() || !totalCopies) {
      setAddStatus({ type: 'error', message: 'Title, author, ISBN, and total copies are all required.' })
      return
    }

    setAddSubmitting(true)
    try {
      await addBook({
        title: newBook.title.trim(),
        author: newBook.author.trim(),
        isbn: newBook.isbn.trim(),
        category: newBook.category.trim(),
        totalCopies,
        description: newBook.description.trim(),
      })
      setAddStatus({ type: 'success', message: `"${newBook.title.trim()}" added to the catalog.` })
      setNewBook(emptyNewBook)
      await load()
    } catch (err) {
      setAddStatus({ type: 'error', message: err.message })
    } finally {
      setAddSubmitting(false)
    }
  }

  const startEditing = (book) => {
    setEditingId(book.id)
    setEditError(null)
    setEditForm({
      title: book.title,
      author: book.author,
      isbn: book.isbn,
      category: book.category,
      totalCopies: String(book.totalCopies),
      availableCopies: String(book.availableCopies),
      description: book.description,
    })
  }

  const handleSaveEdit = async (id) => {
    setEditError(null)
    const totalCopies = Number(editForm.totalCopies)
    const availableCopies = Number(editForm.availableCopies)

    if (!editForm.title.trim() || !editForm.author.trim() || !editForm.isbn.trim() || !totalCopies) {
      setEditError('Title, author, ISBN, and total copies are all required.')
      return
    }
    if (availableCopies < 0 || availableCopies > totalCopies) {
      setEditError('Available copies must be between 0 and the total number of copies.')
      return
    }

    setEditSubmitting(true)
    try {
      await updateBook(id, {
        title: editForm.title.trim(),
        author: editForm.author.trim(),
        isbn: editForm.isbn.trim(),
        category: editForm.category.trim(),
        totalCopies,
        availableCopies,
        description: editForm.description.trim(),
      })
      setEditingId(null)
      await load()
    } catch (err) {
      setEditError(err.message)
    } finally {
      setEditSubmitting(false)
    }
  }

  const handleDelete = async (id, title) => {
    if (!window.confirm(`Remove "${title}" from the catalog?`)) return
    try {
      await deleteBook(id)
      await load()
    } catch (err) {
      setError(err.message)
    }
  }

  const loadLoansForBook = async (bookId) => {
    setLoansByBook((m) => ({ ...m, [bookId]: { ...(m[bookId] || {}), loading: true, error: null } }))
    try {
      const items = await getLoansForBook(bookId)
      setLoansByBook((m) => ({ ...m, [bookId]: { items, loading: false, error: null } }))
    } catch (err) {
      setLoansByBook((m) => ({ ...m, [bookId]: { items: [], loading: false, error: err.message } }))
    }
  }

  const toggleLoans = (bookId) => {
    if (expandedBookId === bookId) {
      setExpandedBookId(null)
      return
    }
    setExpandedBookId(bookId)
    if (!loansByBook[bookId]) {
      loadLoansForBook(bookId)
    }
  }

  const handleBorrowSubmit = async (book, e) => {
    e.preventDefault()
    const indexNumber = (borrowIndex[book.id] || '').trim()
    setBorrowStatus((s) => ({ ...s, [book.id]: null }))

    if (!indexNumber) {
      setBorrowStatus((s) => ({ ...s, [book.id]: { type: 'error', message: 'Enter the student\u2019s index number.' } }))
      return
    }

    setBorrowSubmittingId(book.id)
    try {
      const loan = await borrowBook(book.id, indexNumber)
      setBorrowStatus((s) => ({
        ...s,
        [book.id]: { type: 'success', message: `Borrow authorized for ${loan.studentFullName}.` },
      }))
      setBorrowIndex((m) => ({ ...m, [book.id]: '' }))
      await Promise.all([loadLoansForBook(book.id), load()])
    } catch (err) {
      setBorrowStatus((s) => ({ ...s, [book.id]: { type: 'error', message: err.message } }))
    } finally {
      setBorrowSubmittingId(null)
    }
  }

  const handleReturn = async (loan) => {
    setReturnPendingId(loan.id)
    try {
      await returnLoan(loan.id)
      await Promise.all([loadLoansForBook(loan.bookId), load()])
    } catch (err) {
      setLoansByBook((m) => ({ ...m, [loan.bookId]: { ...(m[loan.bookId] || {}), error: err.message } }))
    } finally {
      setReturnPendingId(null)
    }
  }

  return (
    <div className="book-management">
      <div className="admin__header">
        <div>
          <p className="form-page__eyebrow">Librarian</p>
          <h1 className="admin__title">Book catalog</h1>
          <p className="form-page__subtitle">{books.length} title{books.length === 1 ? '' : 's'} in the catalog</p>
        </div>
      </div>

      <section className="card admin__add-card">
        <h2 className="admin__section-title">Add a book</h2>
        <form className="admin__add-form book-management__add-form" onSubmit={handleAddBook}>
          <div className="form-field">
            <label htmlFor="title">Title</label>
            <input id="title" type="text" value={newBook.title} onChange={(e) => setNewBook((f) => ({ ...f, title: e.target.value }))} />
          </div>
          <div className="form-field">
            <label htmlFor="author">Author</label>
            <input id="author" type="text" value={newBook.author} onChange={(e) => setNewBook((f) => ({ ...f, author: e.target.value }))} />
          </div>
          <div className="form-field">
            <label htmlFor="isbn">ISBN</label>
            <input id="isbn" type="text" className="mono" value={newBook.isbn} onChange={(e) => setNewBook((f) => ({ ...f, isbn: e.target.value }))} />
          </div>
          <div className="form-field">
            <label htmlFor="category">Category</label>
            <input id="category" type="text" placeholder="e.g. Computer Science" value={newBook.category} onChange={(e) => setNewBook((f) => ({ ...f, category: e.target.value }))} />
          </div>
          <div className="form-field">
            <label htmlFor="totalCopies">Total copies</label>
            <input id="totalCopies" type="number" min="1" value={newBook.totalCopies} onChange={(e) => setNewBook((f) => ({ ...f, totalCopies: e.target.value }))} />
          </div>
          <div className="form-field book-management__description-field">
            <label htmlFor="description">Description</label>
            <textarea id="description" value={newBook.description} onChange={(e) => setNewBook((f) => ({ ...f, description: e.target.value }))} placeholder="Optional — a short blurb about the book" />
          </div>
          {addStatus && (
            <div className={`alert ${addStatus.type === 'error' ? 'alert-error' : 'alert-success'}`} style={{ marginBottom: 'var(--space-3)' }}>
              {addStatus.message}
            </div>
          )}
          <button type="submit" className="btn btn-primary" disabled={addSubmitting}>
            {addSubmitting ? 'Adding…' : 'Add book'}
          </button>
        </form>
      </section>

      <section className="card admin__table-card">
        <div className="admin__table-header">
          <h2 className="admin__section-title">Catalog</h2>
          <form className="admin__search" onSubmit={handleSearch}>
            <input
              type="text"
              placeholder="Search title, author, category, ISBN…"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
            />
            <button type="submit" className="btn btn-outline">Search</button>
          </form>
        </div>

        {error && <div className="alert alert-error">{error}</div>}
        {loading && <LoadingSpinner label="Loading catalog…" />}

        {!loading && books.length === 0 && (
          <p className="admin__empty">No books match yet. Add one above.</p>
        )}

        {!loading && books.length > 0 && (
          <div className="book-management__list">
            {books.map((book) => (
              <div key={book.id} className="card book-management__book-card">
                {editingId === book.id ? (
                  <>
                    <div className="book-management__edit-grid">
                      <div className="form-field">
                        <label>Title</label>
                        <input type="text" value={editForm.title} onChange={(e) => setEditForm((f) => ({ ...f, title: e.target.value }))} />
                      </div>
                      <div className="form-field">
                        <label>Author</label>
                        <input type="text" value={editForm.author} onChange={(e) => setEditForm((f) => ({ ...f, author: e.target.value }))} />
                      </div>
                      <div className="form-field">
                        <label>ISBN</label>
                        <input type="text" className="mono" value={editForm.isbn} onChange={(e) => setEditForm((f) => ({ ...f, isbn: e.target.value }))} />
                      </div>
                      <div className="form-field">
                        <label>Category</label>
                        <input type="text" value={editForm.category} onChange={(e) => setEditForm((f) => ({ ...f, category: e.target.value }))} />
                      </div>
                      <div className="form-field">
                        <label>Total copies</label>
                        <input type="number" min="1" value={editForm.totalCopies} onChange={(e) => setEditForm((f) => ({ ...f, totalCopies: e.target.value }))} />
                      </div>
                      <div className="form-field">
                        <label>Available now</label>
                        <input type="number" min="0" value={editForm.availableCopies} onChange={(e) => setEditForm((f) => ({ ...f, availableCopies: e.target.value }))} />
                      </div>
                    </div>
                    <div className="form-field">
                      <label>Description</label>
                      <textarea value={editForm.description} onChange={(e) => setEditForm((f) => ({ ...f, description: e.target.value }))} />
                    </div>
                    {editError && <div className="alert alert-error" style={{ marginBottom: 'var(--space-3)' }}>{editError}</div>}
                    <div className="book-management__book-actions">
                      <button className="btn btn-primary" onClick={() => handleSaveEdit(book.id)} disabled={editSubmitting}>
                        {editSubmitting ? 'Saving…' : 'Save'}
                      </button>
                      <button className="btn btn-outline" onClick={() => setEditingId(null)}>Cancel</button>
                    </div>
                  </>
                ) : (
                  <>
                    <div className="book-management__book-header">
                      <div>
                        <h3 className="book-management__book-title">{book.title}</h3>
                        <p className="book-management__book-author">by {book.author}</p>
                      </div>
                      <div className="book-management__book-badges">
                        {book.isRecentlyAdded && <span className="badge badge-yellow">New</span>}
                        <span className={`badge ${book.availableCopies > 0 ? (book.availableCopies <= 2 ? 'badge-amber' : 'badge-sage') : 'badge-coral'}`}>
                          {book.availableCopies > 0 ? `${book.availableCopies} of ${book.totalCopies} available` : 'All copies checked out'}
                        </span>
                      </div>
                    </div>
                    <p className="book-management__book-meta">
                      {book.category && <span>{book.category} · </span>}
                      <span className="mono">ISBN {book.isbn}</span>
                    </p>
                    {book.description && <p className="book-management__book-description">{book.description}</p>}
                    <div className="book-management__book-actions">
                      <button className="btn btn-outline" onClick={() => startEditing(book)}>Edit</button>
                      <button className="btn btn-danger" onClick={() => handleDelete(book.id, book.title)}>Delete</button>
                      <button className="btn btn-accent" onClick={() => toggleLoans(book.id)}>
                        {expandedBookId === book.id ? 'Hide borrowing activity' : 'Borrowing activity'}
                      </button>
                    </div>

                    {expandedBookId === book.id && (
                      <div className="book-management__loans-panel">
                        {/* ---- Authorize a new borrow ---- */}
                        <form className="book-management__borrow-form" onSubmit={(e) => handleBorrowSubmit(book, e)}>
                          <div className="form-field" style={{ marginBottom: 0, flex: 1 }}>
                            <label htmlFor={`borrow-${book.id}`}>Student index number</label>
                            <input
                              id={`borrow-${book.id}`}
                              type="text"
                              className="mono"
                              placeholder="e.g. UG12345"
                              value={borrowIndex[book.id] || ''}
                              onChange={(e) => setBorrowIndex((m) => ({ ...m, [book.id]: e.target.value }))}
                            />
                          </div>
                          <button
                            type="submit"
                            className="btn btn-primary"
                            disabled={borrowSubmittingId === book.id || book.availableCopies <= 0}
                          >
                            {borrowSubmittingId === book.id ? 'Authorizing…' : 'Authorize borrow'}
                          </button>
                        </form>
                        {book.availableCopies <= 0 && (
                          <p className="admin__section-caption">No copies left to lend until one is returned.</p>
                        )}
                        {borrowStatus[book.id] && (
                          <div
                            className={`alert ${borrowStatus[book.id].type === 'error' ? 'alert-error' : 'alert-success'}`}
                            style={{ marginTop: 'var(--space-2)' }}
                          >
                            {borrowStatus[book.id].message}
                          </div>
                        )}

                        {/* ---- Who has it now, and who's had it before ---- */}
                        <div className="book-management__loans-list">
                          {loansByBook[book.id]?.error && (
                            <div className="alert alert-error">{loansByBook[book.id].error}</div>
                          )}
                          {loansByBook[book.id]?.loading && <LoadingSpinner label="Loading borrowing activity…" />}
                          {!loansByBook[book.id]?.loading && (loansByBook[book.id]?.items?.length ?? 0) === 0 && (
                            <p className="admin__empty">No one has borrowed this title yet.</p>
                          )}
                          {!loansByBook[book.id]?.loading && (loansByBook[book.id]?.items?.length ?? 0) > 0 && (
                            <div className="admin__table-wrap">
                              <table className="admin__table">
                                <thead>
                                  <tr>
                                    <th>Student</th>
                                    <th>Index number</th>
                                    <th>Borrowed</th>
                                    <th>Due</th>
                                    <th>Returned</th>
                                    <th>Status</th>
                                    <th></th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {loansByBook[book.id].items.map((loan) => (
                                    <tr key={loan.id}>
                                      <td>{loan.studentFullName}</td>
                                      <td className="mono">{loan.indexNumber}</td>
                                      <td>{new Date(loan.borrowedAt).toLocaleDateString()}</td>
                                      <td>{new Date(loan.dueAt).toLocaleDateString()}</td>
                                      <td>{loan.returnedAt ? new Date(loan.returnedAt).toLocaleDateString() : '—'}</td>
                                      <td>
                                        <span className={`badge ${loanStatusBadge[loan.status] || 'badge-amber'}`}>
                                          {loan.status}
                                        </span>
                                      </td>
                                      <td>
                                        {loan.status !== 'Returned' && (
                                          <button
                                            className="btn btn-outline admin__row-btn"
                                            disabled={returnPendingId === loan.id}
                                            onClick={() => handleReturn(loan)}
                                          >
                                            {returnPendingId === loan.id ? 'Working…' : 'Mark returned'}
                                          </button>
                                        )}
                                      </td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                            </div>
                          )}
                        </div>
                      </div>
                    )}
                  </>
                )}
              </div>
            ))}
          </div>
        )}
      </section>
    </div>
  )
}
