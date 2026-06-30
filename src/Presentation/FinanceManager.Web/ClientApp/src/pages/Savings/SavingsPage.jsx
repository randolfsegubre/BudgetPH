import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { PiggyBank, Plus, X, Target, Calendar, TrendingUp, CheckCircle2 } from 'lucide-react'
import toast from 'react-hot-toast'
import api from '../../services/api'
import { formatCurrency, formatDate, formatPercent } from '../../utils/formatters'
import clsx from 'clsx'

const savingsService = {
  getAll: () => api.get('/savings-goals').then(r => r.data),
  create: (data) => api.post('/savings-goals', data).then(r => r.data),
  addContribution: (goalId, data) => api.post(`/savings-goals/${goalId}/contributions`, data).then(r => r.data),
}

function AddGoalModal({ onClose, onCreated }) {
  const { register, handleSubmit, formState: { errors } } = useForm()

  const mutation = useMutation({
    mutationFn: savingsService.create,
    onSuccess: (data) => { onCreated(data); onClose() },
    onError: () => toast.error('Failed to create savings goal')
  })

  const onSubmit = (values) => mutation.mutate({
    ...values,
    targetAmount: parseFloat(values.targetAmount),
    targetDate: values.targetDate || null,
  })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="card w-full max-w-md p-6 mx-4">
        <div className="flex items-center justify-between mb-5">
          <h3 className="font-semibold text-dark-100 text-lg">New Savings Goal</h3>
          <button onClick={onClose} className="btn-icon"><X size={18} /></button>
        </div>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="label">Goal Name</label>
            <input className={clsx('input', errors.name && 'input-error')}
              placeholder="Emergency Fund, Travel, etc."
              {...register('name', { required: 'Name is required' })} />
            {errors.name && <p className="text-xs text-danger mt-1">{errors.name.message}</p>}
          </div>
          <div>
            <label className="label">Target Amount (₱)</label>
            <input type="number" step="0.01" className={clsx('input', errors.targetAmount && 'input-error')}
              placeholder="0.00"
              {...register('targetAmount', { required: 'Amount is required', min: { value: 1, message: 'Must be positive' } })} />
            {errors.targetAmount && <p className="text-xs text-danger mt-1">{errors.targetAmount.message}</p>}
          </div>
          <div>
            <label className="label">Target Date (optional)</label>
            <input type="date" className="input" {...register('targetDate')} />
          </div>
          <div>
            <label className="label">Description (optional)</label>
            <input className="input" placeholder="What are you saving for?" {...register('description')} />
          </div>
          <div className="flex gap-3 pt-2">
            <button type="button" onClick={onClose} className="btn btn-ghost flex-1">Cancel</button>
            <button type="submit" disabled={mutation.isPending} className="btn btn-primary flex-1">
              {mutation.isPending ? 'Creating…' : 'Create Goal'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

function ContributeModal({ goal, onClose, onDone }) {
  const { register, handleSubmit, formState: { errors } } = useForm()

  const mutation = useMutation({
    mutationFn: (data) => savingsService.addContribution(goal.id, data),
    onSuccess: () => { toast.success('Contribution recorded!'); onDone(); onClose() },
    onError: () => toast.error('Failed to record contribution')
  })

  const onSubmit = (values) => mutation.mutate({
    amount: parseFloat(values.amount),
    isWithdrawal: values.isWithdrawal === 'true',
    notes: values.notes || null,
    contributionDate: new Date().toISOString(),
  })

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60">
      <div className="card w-full max-w-sm p-6 mx-4">
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-semibold text-dark-100">Log Contribution</h3>
          <button onClick={onClose} className="btn-icon"><X size={18} /></button>
        </div>
        <p className="text-sm text-dark-400 mb-4">{goal.name}</p>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="label">Amount (₱)</label>
            <input type="number" step="0.01" className={clsx('input', errors.amount && 'input-error')}
              placeholder="0.00"
              {...register('amount', { required: 'Amount is required', min: { value: 0.01, message: 'Must be positive' } })} />
          </div>
          <div>
            <label className="label">Type</label>
            <select className="input" {...register('isWithdrawal')}>
              <option value="false">Deposit / Add</option>
              <option value="true">Withdrawal</option>
            </select>
          </div>
          <div>
            <label className="label">Notes (optional)</label>
            <input className="input" placeholder="Optional notes…" {...register('notes')} />
          </div>
          <div className="flex gap-3">
            <button type="button" onClick={onClose} className="btn btn-ghost flex-1">Cancel</button>
            <button type="submit" disabled={mutation.isPending} className="btn btn-primary flex-1">
              {mutation.isPending ? 'Saving…' : 'Save'}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}

function GoalCard({ goal, onContribute }) {
  const pct = goal.progressPercentage ?? 0
  const isComplete = goal.isCompleted || pct >= 100
  const daysLeft = goal.daysRemaining

  return (
    <div className={clsx('card p-5', isComplete && 'ring-1 ring-success/40')}>
      <div className="flex items-start justify-between mb-3">
        <div className="flex items-center gap-3">
          <div className={clsx(
            'w-10 h-10 rounded-full flex items-center justify-center text-xl',
            isComplete ? 'bg-success/20' : 'bg-primary-900/50'
          )}>
            {goal.icon ?? '🎯'}
          </div>
          <div>
            <div className="font-semibold text-dark-100 flex items-center gap-2">
              {goal.name}
              {isComplete && <CheckCircle2 size={14} className="text-success" />}
            </div>
            {goal.description && <div className="text-xs text-dark-400">{goal.description}</div>}
          </div>
        </div>
        <button
          onClick={() => onContribute(goal)}
          disabled={isComplete}
          className="btn btn-ghost text-xs px-2 py-1 disabled:opacity-40"
        >
          + Add
        </button>
      </div>

      {/* Progress */}
      <div className="mb-3">
        <div className="flex justify-between text-xs text-dark-400 mb-1">
          <span>{formatCurrency(goal.currentAmount)} saved</span>
          <span>{pct.toFixed(1)}%</span>
        </div>
        <div className="progress-bar">
          <div
            className={clsx('progress-fill', isComplete ? 'bg-success' : 'bg-primary-500')}
            style={{ width: `${Math.min(pct, 100)}%` }}
          />
        </div>
        <div className="text-xs text-dark-400 mt-1 text-right">
          Target: {formatCurrency(goal.targetAmount)}
        </div>
      </div>

      {/* Meta */}
      <div className="flex items-center gap-4 text-xs text-dark-400">
        {goal.targetDate && (
          <span className="flex items-center gap-1">
            <Calendar size={11} />
            {formatDate(goal.targetDate)}
            {daysLeft != null && !isComplete && (
              <span className={clsx('ml-1', daysLeft < 30 ? 'text-danger' : 'text-dark-400')}>
                ({daysLeft}d left)
              </span>
            )}
          </span>
        )}
        {goal.priority > 1 && (
          <span className="badge badge-purple">P{goal.priority}</span>
        )}
        {goal.isPaused && (
          <span className="badge badge-gray">Paused</span>
        )}
      </div>
    </div>
  )
}

export default function SavingsPage() {
  const queryClient = useQueryClient()
  const [showAdd, setShowAdd] = useState(false)
  const [contributing, setContributing] = useState(null)

  const { data: goals = [], isLoading } = useQuery({
    queryKey: ['savings-goals'],
    queryFn: savingsService.getAll,
  })

  const totalSaved = goals.reduce((s, g) => s + (g.currentAmount ?? 0), 0)
  const totalTarget = goals.reduce((s, g) => s + (g.targetAmount ?? 0), 0)
  const completedCount = goals.filter(g => g.isCompleted || g.progressPercentage >= 100).length

  const handleCreated = () => queryClient.invalidateQueries({ queryKey: ['savings-goals'] })
  const handleContributed = () => queryClient.invalidateQueries({ queryKey: ['savings-goals'] })

  if (isLoading) return (
    <div className="flex items-center justify-center h-48">
      <div className="w-8 h-8 border-2 border-primary-500 border-t-transparent rounded-full animate-spin" />
    </div>
  )

  return (
    <div className="space-y-6">
      {showAdd && <AddGoalModal onClose={() => setShowAdd(false)} onCreated={handleCreated} />}
      {contributing && (
        <ContributeModal
          goal={contributing}
          onClose={() => setContributing(null)}
          onDone={handleContributed}
        />
      )}

      {/* Header */}
      <div className="flex items-center justify-between">
        <div />
        <button onClick={() => setShowAdd(true)} className="btn btn-primary">
          <Plus size={16} /> New Goal
        </button>
      </div>

      {/* Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="stat-card">
          <div className="stat-icon bg-primary-900/50">
            <PiggyBank size={20} className="text-primary-400" />
          </div>
          <div>
            <div className="text-xs text-dark-400">Total Saved</div>
            <div className="text-xl font-bold text-dark-100">{formatCurrency(totalSaved)}</div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon bg-gold-800/30">
            <Target size={20} className="text-gold-400" />
          </div>
          <div>
            <div className="text-xs text-dark-400">Total Target</div>
            <div className="text-xl font-bold text-dark-100">{formatCurrency(totalTarget)}</div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon bg-success/20">
            <TrendingUp size={20} className="text-success" />
          </div>
          <div>
            <div className="text-xs text-dark-400">Goals Completed</div>
            <div className="text-xl font-bold text-dark-100">{completedCount} / {goals.length}</div>
          </div>
        </div>
      </div>

      {/* Goals grid */}
      {goals.length === 0 ? (
        <div className="card text-center py-16">
          <PiggyBank size={48} className="mx-auto mb-4 text-dark-600" />
          <h2 className="text-dark-200 font-semibold text-lg mb-2">No Savings Goals Yet</h2>
          <p className="text-dark-400 text-sm max-w-md mx-auto mb-6">
            Set a goal for your emergency fund, travel, gadgets, OFW remittances, down payment, or anything you're saving toward.
          </p>
          <button onClick={() => setShowAdd(true)} className="btn btn-primary">
            <Plus size={16} /> Create Your First Goal
          </button>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
          {goals.map(g => (
            <GoalCard key={g.id} goal={g} onContribute={setContributing} />
          ))}
        </div>
      )}
    </div>
  )
}
