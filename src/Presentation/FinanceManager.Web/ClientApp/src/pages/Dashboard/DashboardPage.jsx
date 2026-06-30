import { useQuery } from '@tanstack/react-query'
import { dashboardService } from '../../services'
import { formatCurrency, formatDate, formatPercent, getAmountClass } from '../../utils/formatters'
import {
  AreaChart, Area, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip,
  ResponsiveContainer, PieChart, Pie, Cell, Legend
} from 'recharts'
import {
  TrendingUp, TrendingDown, Wallet, CreditCard, PiggyBank, Target,
  ArrowUpRight, ArrowDownRight, AlertCircle, RefreshCw
} from 'lucide-react'

const PURPLE_SHADES = ['#7c3aed', '#8b5cf6', '#a78bfa', '#c4b5fd', '#ddd6fe', '#5b21b6', '#4c1d95']

function StatCard({ label, value, subtitle, icon: Icon, iconBg, trend, trendValue }) {
  return (
    <div className="stat-card">
      <div className={`stat-icon ${iconBg}`}>
        <Icon size={20} className="text-white" />
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-dark-400 text-xs font-medium mb-0.5">{label}</p>
        <p className="text-dark-100 text-xl font-bold truncate">{value}</p>
        {subtitle && <p className="text-dark-500 text-xs mt-0.5">{subtitle}</p>}
        {trendValue !== undefined && (
          <div className={`flex items-center gap-1 text-xs mt-1 ${trend >= 0 ? 'text-success' : 'text-danger'}`}>
            {trend >= 0 ? <ArrowUpRight size={12} /> : <ArrowDownRight size={12} />}
            {formatPercent(Math.abs(trendValue))} vs last month
          </div>
        )}
      </div>
    </div>
  )
}

function CustomTooltip({ active, payload, label, currency = 'PHP' }) {
  if (!active || !payload?.length) return null
  return (
    <div className="bg-dark-800 border border-dark-600 rounded-lg p-3 text-xs shadow-xl">
      <p className="text-dark-300 mb-2 font-medium">{label}</p>
      {payload.map((p, i) => (
        <div key={i} className="flex items-center gap-2 mb-1">
          <div className="w-2 h-2 rounded-full" style={{ background: p.color }} />
          <span className="text-dark-400">{p.name}:</span>
          <span className="text-dark-100 font-semibold">{formatCurrency(p.value)}</span>
        </div>
      ))}
    </div>
  )
}

export default function DashboardPage() {
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['dashboard'],
    queryFn: dashboardService.getSummary,
    refetchInterval: 5 * 60 * 1000,
  })

  if (isLoading) return (
    <div className="flex items-center justify-center h-64">
      <div className="text-center">
        <div className="w-10 h-10 border-2 border-primary-600 border-t-transparent rounded-full animate-spin mx-auto mb-3" />
        <p className="text-dark-400 text-sm">Loading your finances…</p>
      </div>
    </div>
  )

  if (error) return (
    <div className="flex items-center justify-center h-64">
      <div className="text-center">
        <AlertCircle className="text-danger mx-auto mb-3" size={32} />
        <p className="text-dark-300 mb-3">Failed to load dashboard</p>
        <button onClick={refetch} className="btn-secondary gap-2"><RefreshCw size={14} /> Retry</button>
      </div>
    </div>
  )

  const d = data ?? {}
  const savingsRateColor = d.monthlySavingsRate >= 20 ? 'text-success' : d.monthlySavingsRate >= 10 ? 'text-warning' : 'text-danger'

  return (
    <div className="space-y-6 max-w-screen-2xl">
      {/* Overview stats */}
      <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
        <StatCard
          label="Net Worth"
          value={formatCurrency(d.totalNetWorth)}
          subtitle={`Assets: ${formatCurrency(d.totalAssets)}`}
          icon={TrendingUp}
          iconBg="bg-gradient-purple"
        />
        <StatCard
          label="Monthly Income"
          value={formatCurrency(d.monthlyIncome)}
          subtitle="This month"
          icon={ArrowUpRight}
          iconBg="bg-success/20"
        />
        <StatCard
          label="Monthly Expenses"
          value={formatCurrency(d.monthlyExpenses)}
          subtitle="This month"
          icon={ArrowDownRight}
          iconBg="bg-danger/20"
        />
        <div className="stat-card">
          <div className="stat-icon bg-gold-500/20">
            <PiggyBank size={20} className="text-gold-400" />
          </div>
          <div>
            <p className="text-dark-400 text-xs font-medium mb-0.5">Savings Rate</p>
            <p className={`text-xl font-bold ${savingsRateColor}`}>{formatPercent(d.monthlySavingsRate)}</p>
            <p className="text-dark-500 text-xs mt-0.5">
              {d.monthlySavingsRate >= 20 ? '🎯 Great job!' : d.monthlySavingsRate >= 10 ? '👍 Good' : '⚠️ Low savings'}
            </p>
          </div>
        </div>
      </div>

      {/* Charts row */}
      <div className="grid grid-cols-1 xl:grid-cols-3 gap-4">
        {/* Spending trend */}
        <div className="card xl:col-span-2">
          <h3 className="text-dark-200 font-semibold mb-4">Income vs Expenses (6 months)</h3>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={d.spendingTrend ?? []} barGap={4} barSize={18}>
              <CartesianGrid strokeDasharray="3 3" stroke="#334155" vertical={false} />
              <XAxis dataKey="month" tick={{ fill: '#64748b', fontSize: 11 }} axisLine={false} tickLine={false} />
              <YAxis tick={{ fill: '#64748b', fontSize: 11 }} axisLine={false} tickLine={false}
                tickFormatter={v => `₱${v >= 1000 ? (v / 1000).toFixed(0) + 'k' : v}`} />
              <Tooltip content={<CustomTooltip />} cursor={{ fill: 'rgba(124,58,237,0.06)' }} />
              <Bar dataKey="income"   name="Income"   fill="#22c55e" radius={[4,4,0,0]} />
              <Bar dataKey="expenses" name="Expenses" fill="#7c3aed" radius={[4,4,0,0]} />
            </BarChart>
          </ResponsiveContainer>
        </div>

        {/* Spending by category */}
        <div className="card">
          <h3 className="text-dark-200 font-semibold mb-4">Top Spending</h3>
          {d.topSpendingCategories?.length ? (
            <>
              <ResponsiveContainer width="100%" height={160}>
                <PieChart>
                  <Pie data={d.topSpendingCategories} dataKey="amount" nameKey="categoryName"
                    cx="50%" cy="50%" innerRadius={45} outerRadius={70} paddingAngle={3}>
                    {d.topSpendingCategories.map((_, i) => (
                      <Cell key={i} fill={PURPLE_SHADES[i % PURPLE_SHADES.length]} />
                    ))}
                  </Pie>
                  <Tooltip formatter={(v) => formatCurrency(v)} />
                </PieChart>
              </ResponsiveContainer>
              <div className="space-y-2 mt-2">
                {d.topSpendingCategories.slice(0, 4).map((c, i) => (
                  <div key={c.categoryName} className="flex items-center gap-2 text-xs">
                    <div className="w-2 h-2 rounded-full flex-shrink-0" style={{ background: PURPLE_SHADES[i] }} />
                    <span className="text-dark-300 flex-1 truncate">{c.icon} {c.categoryName}</span>
                    <span className="text-dark-200 font-medium">{formatCurrency(c.amount)}</span>
                  </div>
                ))}
              </div>
            </>
          ) : (
            <div className="h-48 flex items-center justify-center text-dark-500 text-sm">No spending this month</div>
          )}
        </div>
      </div>

      {/* Accounts + Savings Goals */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-4">
        {/* Accounts */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-dark-200 font-semibold">Accounts</h3>
            <a href="/accounts" className="text-primary-400 hover:text-primary-300 text-xs">View all →</a>
          </div>
          <div className="space-y-2">
            {d.accounts?.length ? d.accounts.map(a => (
              <div key={a.id} className="flex items-center gap-3 p-3 rounded-lg bg-dark-900/50 hover:bg-dark-700/30 transition-colors">
                <div className="w-8 h-8 rounded-lg flex items-center justify-center flex-shrink-0 text-sm"
                  style={{ background: a.color ? a.color + '30' : '#7c3aed30' }}>
                  {a.icon ?? '🏦'}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-dark-100 text-sm font-medium truncate">{a.name}</p>
                  <p className="text-dark-500 text-xs">{a.institutionName ?? 'Personal'}</p>
                </div>
                <span className={`text-sm font-semibold font-mono ${a.balance < 0 ? 'text-danger' : 'text-dark-100'}`}>
                  {formatCurrency(a.balance, a.currency)}
                </span>
              </div>
            )) : <p className="text-dark-500 text-sm text-center py-6">No accounts yet</p>}
          </div>
        </div>

        {/* Savings Goals */}
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-dark-200 font-semibold">Savings Goals</h3>
            <a href="/savings" className="text-primary-400 hover:text-primary-300 text-xs">View all →</a>
          </div>
          <div className="space-y-3">
            {d.savingsGoals?.length ? d.savingsGoals.map(g => (
              <div key={g.goalId} className="space-y-1.5">
                <div className="flex items-center justify-between text-sm">
                  <span className="text-dark-200 font-medium">{g.icon} {g.name}</span>
                  <span className="text-dark-400 text-xs">{formatCurrency(g.currentAmount)} / {formatCurrency(g.targetAmount)}</span>
                </div>
                <div className="progress-bar">
                  <div className="progress-fill bg-gradient-purple" style={{ width: `${g.percentage}%` }} />
                </div>
                <div className="flex justify-between text-xs text-dark-500">
                  <span>{formatPercent(g.percentage)}</span>
                  {g.daysRemaining !== null && (
                    <span>{g.daysRemaining > 0 ? `${g.daysRemaining} days left` : 'Overdue'}</span>
                  )}
                </div>
              </div>
            )) : <p className="text-dark-500 text-sm text-center py-6">No savings goals yet</p>}
          </div>
        </div>
      </div>

      {/* Budget Progress */}
      {d.budgetProgress?.length > 0 && (
        <div className="card">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-dark-200 font-semibold">Budget Progress — This Month</h3>
            <a href="/budgets" className="text-primary-400 hover:text-primary-300 text-xs">Manage budgets →</a>
          </div>
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-3">
            {d.budgetProgress.slice(0, 8).map(b => (
              <div key={b.budgetId + b.categoryName} className="bg-dark-900/50 rounded-lg p-3">
                <div className="flex items-center justify-between mb-2">
                  <span className="text-dark-200 text-xs font-medium">{b.categoryIcon} {b.categoryName}</span>
                  {b.isOverBudget && <span className="badge-red text-xs">Over!</span>}
                </div>
                <div className="progress-bar mb-1.5">
                  <div
                    className={`progress-fill ${b.isOverBudget ? 'bg-danger' : b.percentage > 80 ? 'bg-warning' : 'bg-gradient-purple'}`}
                    style={{ width: `${Math.min(b.percentage, 100)}%` }}
                  />
                </div>
                <div className="flex justify-between text-xs text-dark-500">
                  <span>{formatCurrency(b.spent)}</span>
                  <span>{formatCurrency(b.allocated)}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Recent Transactions */}
      <div className="card">
        <div className="flex items-center justify-between mb-4">
          <h3 className="text-dark-200 font-semibold">Recent Transactions</h3>
          <a href="/transactions" className="text-primary-400 hover:text-primary-300 text-xs">View all →</a>
        </div>
        {d.recentTransactions?.length ? (
          <div className="table-container">
            <table className="table">
              <thead>
                <tr>
                  <th>Date</th>
                  <th>Description</th>
                  <th>Account</th>
                  <th>Category</th>
                  <th className="text-right">Amount</th>
                </tr>
              </thead>
              <tbody>
                {d.recentTransactions.map(t => (
                  <tr key={t.id}>
                    <td className="text-dark-400 text-xs whitespace-nowrap">{formatDate(t.transactionDate)}</td>
                    <td>
                      <div className="font-medium text-dark-200 text-sm">{t.description}</div>
                    </td>
                    <td className="text-dark-400 text-xs">{t.accountName}</td>
                    <td>
                      {t.categoryName ? (
                        <span className="badge-purple text-xs">{t.categoryIcon} {t.categoryName}</span>
                      ) : <span className="text-dark-600 text-xs">—</span>}
                    </td>
                    <td className={`text-right font-semibold font-mono text-sm ${t.type === 1 ? 'text-success' : t.type === 2 ? 'text-danger' : 'text-dark-300'}`}>
                      {t.type === 1 ? '+' : t.type === 2 ? '-' : ''}{formatCurrency(Math.abs(t.amount))}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div className="text-center py-8 text-dark-500 text-sm">No recent transactions</div>
        )}
      </div>
    </div>
  )
}
