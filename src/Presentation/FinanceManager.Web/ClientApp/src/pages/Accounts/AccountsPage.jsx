import { useState } from 'react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { accountsService } from '../../services'
import { formatCurrency, formatDate, accountTypeLabel } from '../../utils/formatters'
import { Plus, Building2, CreditCard, TrendingUp, Wallet, Trash2, Edit3, Eye } from 'lucide-react'
import toast from 'react-hot-toast'

const accountTypeIcons = { 1: '🏦', 2: '💰', 3: '💳', 4: '📈', 5: '🏠', 6: '💵', 7: '📋' }

export default function AccountsPage() {
  const qc = useQueryClient()
  const [showForm, setShowForm] = useState(false)
  const [form, setForm] = useState({ name: '', accountType: 2, openingBalance: 0, currency: 'PHP', institutionName: '', includeInNetWorth: true })

  const { data: accounts = [], isLoading } = useQuery({ queryKey: ['accounts'], queryFn: accountsService.getAll })
  const { data: summary } = useQuery({ queryKey: ['accounts-summary'], queryFn: accountsService.getSummary })

  const createMutation = useMutation({
    mutationFn: accountsService.create,
    onSuccess: () => { qc.invalidateQueries(['accounts']); qc.invalidateQueries(['accounts-summary']); setShowForm(false); toast.success('Account added!') },
    onError: (e) => toast.error(e.response?.data?.error || 'Failed to add account')
  })

  const deleteMutation = useMutation({
    mutationFn: accountsService.delete,
    onSuccess: () => { qc.invalidateQueries(['accounts']); toast.success('Account removed') }
  })

  const handleSubmit = (e) => { e.preventDefault(); createMutation.mutate(form) }

  const banks = accounts.filter(a => a.accountType === 1 || a.accountType === 2)
  const cards = accounts.filter(a => a.accountType === 3)
  const loans = accounts.filter(a => a.accountType === 5)
  const investments = accounts.filter(a => a.accountType === 4)

  return (
    <div className="space-y-6 max-w-screen-xl">
      {/* Net Worth summary */}
      <div className="grid grid-cols-3 gap-4">
        {[
          { label: 'Total Assets', value: summary?.totalAssets, color: 'text-success', bg: 'bg-success/10 border-success/20' },
          { label: 'Total Liabilities', value: summary?.totalLiabilities, color: 'text-danger', bg: 'bg-danger/10 border-danger/20' },
          { label: 'Net Worth', value: summary?.netWorth, color: 'text-gradient-purple', bg: 'glow-border' },
        ].map(s => (
          <div key={s.label} className={`card border ${s.bg}`}>
            <p className="text-dark-400 text-xs mb-1">{s.label}</p>
            <p className={`text-2xl font-bold ${s.color}`}>{formatCurrency(s.value ?? 0)}</p>
          </div>
        ))}
      </div>

      {/* Add Account Button */}
      <div className="flex justify-end">
        <button onClick={() => setShowForm(v => !v)} className="btn-primary">
          <Plus size={16} /> Add Account
        </button>
      </div>

      {/* Add form */}
      {showForm && (
        <div className="card border border-primary-700/30">
          <h3 className="text-dark-100 font-semibold mb-4">New Account</h3>
          <form onSubmit={handleSubmit} className="grid grid-cols-2 gap-4">
            <div>
              <label className="label">Account Name</label>
              <input className="input" value={form.name} onChange={e => setForm(p => ({...p, name: e.target.value}))} placeholder="e.g. BDO Savings" required />
            </div>
            <div>
              <label className="label">Account Type</label>
              <select className="input" value={form.accountType} onChange={e => setForm(p => ({...p, accountType: +e.target.value}))}>
                {Object.entries(accountTypeLabel).map(([k, v]) => <option key={k} value={k}>{v}</option>)}
              </select>
            </div>
            <div>
              <label className="label">Institution</label>
              <input className="input" value={form.institutionName} onChange={e => setForm(p => ({...p, institutionName: e.target.value}))} placeholder="BDO, BPI, Metrobank…" />
            </div>
            <div>
              <label className="label">Opening Balance (₱)</label>
              <input type="number" step="0.01" className="input" value={form.openingBalance} onChange={e => setForm(p => ({...p, openingBalance: +e.target.value}))} />
            </div>
            <div className="col-span-2 flex gap-3 justify-end">
              <button type="button" onClick={() => setShowForm(false)} className="btn-secondary">Cancel</button>
              <button type="submit" disabled={createMutation.isPending} className="btn-primary">
                {createMutation.isPending ? 'Adding…' : 'Add Account'}
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Account groups */}
      {isLoading ? (
        <div className="text-center py-12 text-dark-400">Loading accounts…</div>
      ) : (
        <div className="space-y-6">
          {[
            { title: '🏦 Bank Accounts', items: banks },
            { title: '💳 Credit Cards', items: cards },
            { title: '📈 Investments', items: investments },
            { title: '🏠 Loans', items: loans },
          ].map(({ title, items }) => items.length > 0 && (
            <div key={title}>
              <h3 className="text-dark-300 font-semibold text-sm mb-3">{title}</h3>
              <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-3">
                {items.map(a => (
                  <div key={a.id} className="card-hover">
                    <div className="flex items-start justify-between mb-3">
                      <div className="flex items-center gap-3">
                        <div className="w-10 h-10 rounded-xl flex items-center justify-center text-xl"
                          style={{ background: (a.color ?? '#7c3aed') + '25' }}>
                          {a.icon ?? accountTypeIcons[a.accountType]}
                        </div>
                        <div>
                          <p className="text-dark-100 font-semibold text-sm">{a.name}</p>
                          <p className="text-dark-500 text-xs">{a.institutionName ?? 'Personal'}</p>
                        </div>
                      </div>
                      <button onClick={() => { if (confirm('Remove this account?')) deleteMutation.mutate(a.id) }}
                        className="btn-icon text-dark-600 hover:text-danger">
                        <Trash2 size={14} />
                      </button>
                    </div>
                    <p className={`text-2xl font-bold font-mono ${a.balance < 0 ? 'text-danger' : 'text-dark-100'}`}>
                      {formatCurrency(a.balance, a.currency)}
                    </p>
                    <p className="text-dark-500 text-xs mt-1">{accountTypeLabel[a.accountType]}</p>
                  </div>
                ))}
              </div>
            </div>
          ))}
          {accounts.length === 0 && (
            <div className="text-center py-16 text-dark-500">
              <Building2 size={40} className="mx-auto mb-3 opacity-40" />
              <p>No accounts yet. Add your first account to get started.</p>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
