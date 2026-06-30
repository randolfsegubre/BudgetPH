import { useQuery } from '@tanstack/react-query'
import { useState } from 'react'
import { TrendingUp, TrendingDown, BarChart2, DollarSign, ChevronDown, ChevronRight } from 'lucide-react'
import { accountsService } from '../../services'
import { formatCurrency, formatPercent } from '../../utils/formatters'
import { PieChart, Pie, Cell, Tooltip, ResponsiveContainer } from 'recharts'
import clsx from 'clsx'

const COLORS = ['#7c3aed', '#f59e0b', '#22c55e', '#3b82f6', '#ef4444', '#06b6d4', '#d946ef', '#84cc16']

const investmentTypeLabel = {
  1: 'Stock', 2: 'Bond', 3: 'ETF', 4: 'Mutual Fund', 5: 'Crypto',
  6: 'Real Estate', 7: 'CD', 8: 'Treasury', 9: 'Option', 10: 'Other'
}

function HoldingRow({ holding }) {
  const gainLoss = (holding.currentPrice - holding.averageCostBasis) * holding.shares
  const gainLossPct = holding.averageCostBasis > 0
    ? ((holding.currentPrice - holding.averageCostBasis) / holding.averageCostBasis) * 100
    : 0
  const marketValue = holding.currentPrice * holding.shares
  const isPositive = gainLoss >= 0

  return (
    <tr className="border-t border-dark-700 hover:bg-dark-700/30 transition-colors">
      <td className="py-3 px-4">
        <div className="font-semibold text-dark-100">{holding.symbol}</div>
        <div className="text-xs text-dark-400">{holding.name}</div>
      </td>
      <td className="py-3 px-4 text-right text-dark-200">
        {holding.shares?.toLocaleString('en-PH', { minimumFractionDigits: 2, maximumFractionDigits: 4 })}
      </td>
      <td className="py-3 px-4 text-right text-dark-200">{formatCurrency(holding.averageCostBasis)}</td>
      <td className="py-3 px-4 text-right text-dark-200">{formatCurrency(holding.currentPrice)}</td>
      <td className="py-3 px-4 text-right font-semibold">{formatCurrency(marketValue)}</td>
      <td className={clsx('py-3 px-4 text-right font-semibold', isPositive ? 'text-success' : 'text-danger')}>
        <div>{isPositive ? '+' : ''}{formatCurrency(gainLoss)}</div>
        <div className="text-xs">{isPositive ? '+' : ''}{gainLossPct.toFixed(2)}%</div>
      </td>
    </tr>
  )
}

function AccountCard({ account }) {
  const [expanded, setExpanded] = useState(false)
  const inv = account.investment
  const holdings = account.investment?.holdings ?? []
  const totalValue = holdings.reduce((s, h) => s + h.currentPrice * h.shares, 0)
  const totalCost = holdings.reduce((s, h) => s + h.averageCostBasis * h.shares, 0)
  const totalGainLoss = totalValue - totalCost
  const isPositive = totalGainLoss >= 0

  return (
    <div className="card">
      <div
        className="flex items-center justify-between p-5 cursor-pointer"
        onClick={() => setExpanded(e => !e)}
      >
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-primary-900/50 flex items-center justify-center">
            <TrendingUp size={20} className="text-primary-400" />
          </div>
          <div>
            <div className="font-semibold text-dark-100">{account.name}</div>
            <div className="text-xs text-dark-400">
              {holdings.length} holding{holdings.length !== 1 ? 's' : ''}
              {inv?.institutionName ? ` · ${inv.institutionName}` : ''}
            </div>
          </div>
        </div>
        <div className="flex items-center gap-4">
          <div className="text-right">
            <div className="text-lg font-bold text-dark-100">{formatCurrency(totalValue)}</div>
            <div className={clsx('text-xs font-semibold', isPositive ? 'text-success' : 'text-danger')}>
              {isPositive ? '+' : ''}{formatCurrency(totalGainLoss)} ({isPositive ? '+' : ''}{totalCost > 0 ? ((totalGainLoss / totalCost) * 100).toFixed(2) : '0.00'}%)
            </div>
          </div>
          {expanded ? <ChevronDown size={16} className="text-dark-400" /> : <ChevronRight size={16} className="text-dark-400" />}
        </div>
      </div>

      {expanded && holdings.length > 0 && (
        <div className="border-t border-dark-700 overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="text-dark-400 text-xs">
                <th className="py-2 px-4 text-left">Symbol</th>
                <th className="py-2 px-4 text-right">Shares</th>
                <th className="py-2 px-4 text-right">Avg Cost</th>
                <th className="py-2 px-4 text-right">Price</th>
                <th className="py-2 px-4 text-right">Value</th>
                <th className="py-2 px-4 text-right">Gain/Loss</th>
              </tr>
            </thead>
            <tbody>
              {holdings.map(h => <HoldingRow key={h.id} holding={h} />)}
            </tbody>
          </table>
        </div>
      )}
      {expanded && holdings.length === 0 && (
        <div className="border-t border-dark-700 p-6 text-center text-dark-400 text-sm">
          No holdings recorded yet. Add transactions to this investment account to track holdings.
        </div>
      )}
    </div>
  )
}

export default function InvestmentsPage() {
  const { data: accounts = [], isLoading } = useQuery({
    queryKey: ['accounts'],
    queryFn: accountsService.getAll,
  })

  const investments = accounts.filter(a => a.accountType === 4)

  const totalValue = investments.reduce((s, a) => {
    const holdings = a.investment?.holdings ?? []
    return s + holdings.reduce((hs, h) => hs + h.currentPrice * h.shares, 0)
  }, 0)

  const totalCost = investments.reduce((s, a) => {
    const holdings = a.investment?.holdings ?? []
    return s + holdings.reduce((hs, h) => hs + h.averageCostBasis * h.shares, 0)
  }, 0)

  const totalGainLoss = totalValue - totalCost
  const isPositive = totalGainLoss >= 0

  // Build allocation data for pie chart
  const allHoldings = investments.flatMap(a => a.investment?.holdings ?? [])
  const pieData = allHoldings.slice(0, 8).map(h => ({
    name: h.symbol,
    value: h.currentPrice * h.shares
  })).filter(d => d.value > 0)

  if (isLoading) return (
    <div className="flex items-center justify-center h-48">
      <div className="w-8 h-8 border-2 border-primary-500 border-t-transparent rounded-full animate-spin" />
    </div>
  )

  return (
    <div className="space-y-6">
      {/* Summary */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="stat-card">
          <div className="stat-icon bg-primary-900/50">
            <BarChart2 size={20} className="text-primary-400" />
          </div>
          <div>
            <div className="text-xs text-dark-400">Total Portfolio Value</div>
            <div className="text-xl font-bold text-dark-100">{formatCurrency(totalValue)}</div>
          </div>
        </div>
        <div className="stat-card">
          <div className={clsx('stat-icon', isPositive ? 'bg-success/20' : 'bg-danger/20')}>
            {isPositive
              ? <TrendingUp size={20} className="text-success" />
              : <TrendingDown size={20} className="text-danger" />}
          </div>
          <div>
            <div className="text-xs text-dark-400">Total Gain / Loss</div>
            <div className={clsx('text-xl font-bold', isPositive ? 'text-success' : 'text-danger')}>
              {isPositive ? '+' : ''}{formatCurrency(totalGainLoss)}
            </div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon bg-gold-800/30">
            <DollarSign size={20} className="text-gold-400" />
          </div>
          <div>
            <div className="text-xs text-dark-400">Total Cost Basis</div>
            <div className="text-xl font-bold text-dark-100">{formatCurrency(totalCost)}</div>
          </div>
        </div>
      </div>

      {investments.length === 0 ? (
        <div className="card text-center py-16">
          <TrendingUp size={48} className="mx-auto mb-4 text-dark-600" />
          <h2 className="text-dark-200 font-semibold text-lg mb-2">No Investment Accounts Yet</h2>
          <p className="text-dark-400 text-sm max-w-md mx-auto">
            Add an Investment account from the <a href="/accounts" className="text-primary-400 hover:underline">Accounts page</a> to
            track your stocks, ETFs, mutual funds, UITFs, crypto, and other investments.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 xl:grid-cols-3 gap-6">
          {/* Accounts list */}
          <div className="xl:col-span-2 space-y-4">
            {investments.map(a => <AccountCard key={a.id} account={a} />)}
          </div>

          {/* Allocation pie */}
          {pieData.length > 0 && (
            <div className="card p-5">
              <h3 className="font-semibold text-dark-200 mb-4">Allocation</h3>
              <ResponsiveContainer width="100%" height={200}>
                <PieChart>
                  <Pie data={pieData} cx="50%" cy="50%" innerRadius={50} outerRadius={80} dataKey="value">
                    {pieData.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
                  </Pie>
                  <Tooltip
                    contentStyle={{ background: '#1e293b', border: '1px solid #334155', borderRadius: 8 }}
                    formatter={v => [formatCurrency(v), 'Value']}
                  />
                </PieChart>
              </ResponsiveContainer>
              <div className="mt-3 space-y-2">
                {pieData.map((d, i) => (
                  <div key={d.name} className="flex items-center justify-between text-sm">
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full" style={{ background: COLORS[i % COLORS.length] }} />
                      <span className="text-dark-300">{d.name}</span>
                    </div>
                    <span className="text-dark-200 font-medium">
                      {totalValue > 0 ? ((d.value / totalValue) * 100).toFixed(1) : '0.0'}%
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
