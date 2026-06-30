import { useQuery } from '@tanstack/react-query'
import { budgetsService } from '../../services'
import { formatCurrency, formatPercent } from '../../utils/formatters'
import { Wallet, Plus } from 'lucide-react'
import { useState } from 'react'

export default function BudgetsPage() {
  const { data: budget, isLoading, error } = useQuery({ queryKey: ['budget-current'], queryFn: budgetsService.getCurrent })
  const now = new Date()

  return (
    <div className="space-y-6 max-w-screen-xl">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-dark-100 font-semibold text-lg">Budget — {now.toLocaleDateString('en-PH', { month: 'long', year: 'numeric' })}</h2>
          <p className="text-dark-400 text-sm">Track your spending against your budget</p>
        </div>
        <button className="btn-primary"><Plus size={15}/> Set Budget</button>
      </div>

      {isLoading && <div className="text-center py-16 text-dark-400">Loading budget…</div>}

      {error && (
        <div className="card border border-primary-700/30 text-center py-12">
          <Wallet size={40} className="mx-auto mb-4 text-dark-600" />
          <p className="text-dark-300 font-medium mb-2">No budget set for this month</p>
          <p className="text-dark-500 text-sm mb-6">Create a budget to track your spending goals</p>
          <button className="btn-primary mx-auto"><Plus size={15} /> Create Budget</button>
        </div>
      )}

      {budget && (
        <>
          {/* Overview */}
          <div className="grid grid-cols-3 gap-4">
            <div className="card"><p className="text-dark-400 text-xs mb-1">Budget Limit</p><p className="text-2xl font-bold text-dark-100">{formatCurrency(budget.totalExpenseLimit)}</p></div>
            <div className="card"><p className="text-dark-400 text-xs mb-1">Total Spent</p><p className={`text-2xl font-bold ${budget.totalSpent > budget.totalExpenseLimit ? 'text-danger' : 'text-dark-100'}`}>{formatCurrency(budget.totalSpent)}</p></div>
            <div className="card"><p className="text-dark-400 text-xs mb-1">Remaining</p><p className={`text-2xl font-bold ${budget.remainingBudget < 0 ? 'text-danger' : 'text-success'}`}>{formatCurrency(budget.remainingBudget)}</p></div>
          </div>

          {/* Overall progress */}
          <div className="card">
            <div className="flex justify-between text-sm mb-2">
              <span className="text-dark-300">Overall Budget Used</span>
              <span className={budget.overallProgress > 100 ? 'text-danger font-semibold' : 'text-dark-300'}>{formatPercent(budget.overallProgress)}</span>
            </div>
            <div className="progress-bar h-3">
              <div className={`progress-fill ${budget.overallProgress > 100 ? 'bg-danger' : budget.overallProgress > 80 ? 'bg-warning' : 'bg-gradient-purple'}`}
                style={{ width: `${Math.min(budget.overallProgress, 100)}%` }} />
            </div>
          </div>

          {/* Category items */}
          <div className="card p-0">
            <div className="p-5 border-b border-dark-700">
              <h3 className="text-dark-200 font-semibold">Category Breakdown</h3>
            </div>
            <div className="divide-y divide-dark-700/50">
              {budget.items.map(item => (
                <div key={item.id} className="p-5">
                  <div className="flex items-center justify-between mb-2">
                    <div className="flex items-center gap-2">
                      <span className="text-base">{item.categoryIcon ?? '📦'}</span>
                      <span className="text-dark-200 font-medium text-sm">{item.categoryName}</span>
                      {item.isOverBudget && <span className="badge-red text-xs">Over Budget</span>}
                    </div>
                    <div className="text-right">
                      <span className={`text-sm font-semibold font-mono ${item.isOverBudget ? 'text-danger' : 'text-dark-200'}`}>{formatCurrency(item.spentAmount)}</span>
                      <span className="text-dark-500 text-xs ml-1">/ {formatCurrency(item.allocatedAmount)}</span>
                    </div>
                  </div>
                  <div className="progress-bar">
                    <div className={`progress-fill ${item.isOverBudget ? 'bg-danger' : item.percentageUsed > 80 ? 'bg-warning' : 'bg-gradient-purple'}`}
                      style={{ width: `${Math.min(item.percentageUsed, 100)}%` }} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </>
      )}
    </div>
  )
}
