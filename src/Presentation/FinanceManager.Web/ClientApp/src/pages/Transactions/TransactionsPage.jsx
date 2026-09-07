import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { transactionsService, accountsService } from '../../services'
import { formatCurrency, formatDate } from '../../utils/formatters'
import { Plus, Search, Filter, Trash2, Download } from 'lucide-react'
import toast from 'react-hot-toast'

const txTypeColors = { 1: 'text-success', 2: 'text-danger', 3: 'text-info', 4: 'text-warning' }
const txTypeLabels = { 1: 'Income', 2: 'Expense', 3: 'Transfer', 4: 'Loan Payment', 5: 'Inv Buy', 6: 'Inv Sell' }

export default function TransactionsPage() {
  const qc = useQueryClient()
  const [filter, setFilter] = useState({ pageNumber: 1, pageSize: 25 })
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ description: '', amount: '', transactionType: 2, transactionDate: new Date().toISOString().split('T')[0], accountId: '', categoryId: '' })

  const { data, isLoading } = useQuery({
    queryKey: ['transactions', filter],
    queryFn: () => transactionsService.getAll(filter)
  })
  const { data: accounts = [] } = useQuery({ queryKey: ['accounts'], queryFn: accountsService.getAll })
  const { data: categories = [] } = useQuery({ queryKey: ['categories'], queryFn: transactionsService.getCategories })

  const createMutation = useMutation({
    mutationFn: transactionsService.create,
    onSuccess: () => { qc.invalidateQueries(['transactions']); qc.invalidateQueries(['accounts']); qc.invalidateQueries(['dashboard']); setShowForm(false); setForm(p => ({...p, description: '', amount: ''})); toast.success('Transaction added!') }
  })

  const deleteMutation = useMutation({
    mutationFn: transactionsService.delete,
    onSuccess: () => { qc.invalidateQueries(['transactions']); qc.invalidateQueries(['accounts']); toast.success('Deleted') }
  })

  const transactions = data?.items ?? []
  const total = data?.totalCount ?? 0

  return (
    <div className="space-y-5 max-w-screen-xl">
      {/* Header row */}
      <div className="flex flex-wrap gap-3 items-center justify-between">
        <div className="flex gap-2 flex-1 min-w-0 max-w-sm">
          <div className="relative flex-1">
            <Search size={14} className="absolute left-3 top-1/2 -translate-y-1/2 text-dark-500" />
            <input className="input pl-8" placeholder="Search transactions…"
              onChange={e => setFilter(p => ({...p, searchTerm: e.target.value, pageNumber: 1}))} />
          </div>
        </div>
        <div className="flex gap-2">
          <select className="input w-36 text-sm" onChange={e => setFilter(p => ({...p, type: e.target.value || undefined, pageNumber: 1}))}>
            <option value="">All types</option>
            {Object.entries(txTypeLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
          </select>
          <button onClick={() => setShowForm(v => !v)} className="btn-primary whitespace-nowrap">
            <Plus size={15} /> Add
          </button>
        </div>
      </div>

      {/* Add form */}
      {showForm && (
        <div className="card border border-primary-700/30">
          <h3 className="text-dark-100 font-semibold mb-4">New Transaction</h3>
          <form onSubmit={e => { e.preventDefault(); createMutation.mutate({ ...form, amount: +form.amount, categoryId: form.categoryId || null }) }}
            className="grid grid-cols-2 md:grid-cols-3 gap-4">
            <div className="col-span-2 md:col-span-1">
              <label className="label">Description</label>
              <input className="input" value={form.description} onChange={e => setForm(p => ({...p, description: e.target.value}))} placeholder="e.g. Jollibee lunch" required />
            </div>
            <div>
              <label className="label">Amount (₱)</label>
              <input type="number" step="0.01" min="0" className="input" value={form.amount} onChange={e => setForm(p => ({...p, amount: e.target.value}))} required />
            </div>
            <div>
              <label className="label">Type</label>
              <select className="input" value={form.transactionType} onChange={e => setForm(p => ({...p, transactionType: +e.target.value}))}>
                {Object.entries(txTypeLabels).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
              </select>
            </div>
            <div>
              <label className="label">Account</label>
              <select className="input" value={form.accountId} onChange={e => setForm(p => ({...p, accountId: e.target.value}))} required>
                <option value="">Select account</option>
                {accounts.map(a => <option key={a.id} value={a.id}>{a.name}</option>)}
              </select>
            </div>
            <div>
              <label className="label">Category</label>
              <select className="input" value={form.categoryId} onChange={e => setForm(p => ({...p, categoryId: e.target.value}))}>
                <option value="">No category</option>
                {categories.map(c => <option key={c.id} value={c.id}>{c.icon} {c.name}</option>)}
              </select>
            </div>
            <div>
              <label className="label">Date</label>
              <input type="date" className="input" value={form.transactionDate} onChange={e => setForm(p => ({...p, transactionDate: e.target.value}))} required />
            </div>
            <div className="col-span-2 md:col-span-3 flex gap-3 justify-end">
              <button type="button" onClick={() => setShowForm(false)} className="btn-secondary">Cancel</button>
              <button type="submit" disabled={createMutation.isPending} className="btn-primary">
                {createMutation.isPending ? 'Saving…' : 'Add Transaction'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Table */}
      <div className="card p-0">
        <div className="p-4 border-b border-dark-700 flex items-center justify-between">
          <p className="text-dark-300 text-sm">{total.toLocaleString()} transactions</p>
        </div>
        {isLoading ? (
          <div className="text-center py-12 text-dark-400">Loading…</div>
        ) : (
          <div className="table-container border-0 rounded-none">
            <table className="table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Description</th>
                  <th>Account</th>
                  <th>Category</th>
                  <th>Type</th>
                  <th className="text-right">Amount</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                {transactions.map(t => (
                  <tr key={t.id}>
                    <td className="text-dark-400 text-xs whitespace-nowrap">{formatDate(t.transactionDate)}</td>
                    <td className="font-medium text-dark-200 max-w-xs truncate">{t.description}</td>
                    <td className="text-dark-400 text-xs">{t.accountName}</td>
                    <td>
                      {t.categoryName
                        ? <span className="badge-purple text-xs">{t.categoryIcon} {t.categoryName}</span>
                        : <span className="text-dark-600 text-xs">—</span>}
                    </td>
                    <td><span className={`text-xs font-medium ${txTypeColors[t.transactionType] ?? 'text-dark-400'}`}>{txTypeLabels[t.transactionType]}</span></td>
                    <td className={`text-right font-semibold font-mono text-sm ${t.transactionType === 1 ? 'text-success' : t.transactionType === 2 ? 'text-danger' : 'text-dark-300'}`}>
                      {t.transactionType === 1 ? '+' : t.transactionType === 2 ? '-' : ''}{formatCurrency(Math.abs(t.amount))}
                    </td>
                    <td>
                      <button onClick={() => { if (confirm('Delete?')) deleteMutation.mutate(t.id) }} className="btn-icon text-dark-600 hover:text-danger">
                        <Trash2 size={13} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {transactions.length === 0 && (
              <div className="text-center py-12 text-dark-500 text-sm">No transactions found</div>
            )}
          </div>
        )}
        {/* Pagination */}
        {total > filter.pageSize && (
          <div className="p-4 border-t border-dark-700 flex items-center justify-between">
            <p className="text-dark-500 text-xs">Page {filter.pageNumber} of {Math.ceil(total / filter.pageSize)}</p>
            <div className="flex gap-2">
              <button disabled={filter.pageNumber <= 1} onClick={() => setFilter(p => ({...p, pageNumber: p.pageNumber - 1}))} className="btn-secondary text-xs px-3 py-1.5 disabled:opacity-40">← Prev</button>
              <button disabled={filter.pageNumber >= Math.ceil(total / filter.pageSize)} onClick={() => setFilter(p => ({...p, pageNumber: p.pageNumber + 1}))} className="btn-secondary text-xs px-3 py-1.5 disabled:opacity-40">Next →</button>
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
