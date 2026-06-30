import { useQuery } from '@tanstack/react-query'
import { CreditCard, AlertCircle, TrendingDown, Calendar, Percent, DollarSign, Home, Car, GraduationCap, Briefcase, Heart } from 'lucide-react'
import { accountsService } from '../../services'
import { formatCurrency, formatDate, loanTypeLabel } from '../../utils/formatters'
import clsx from 'clsx'

const loanTypeIcon = {
  1: <Briefcase size={20} />,
  2: <Car size={20} />,
  3: <Home size={20} />,
  4: <GraduationCap size={20} />,
  5: <Briefcase size={20} />,
  6: <Heart size={20} />,
}

function LoanCard({ account }) {
  const loan = account.loan
  if (!loan) return null

  const paid = loan.originalAmount - loan.outstandingBalance
  const paidPct = loan.originalAmount > 0 ? Math.min((paid / loan.originalAmount) * 100, 100) : 0
  const remaining = loan.remainingPayments ?? 0
  const isOverdue = account.balance < 0 && loan.outstandingBalance > 0

  return (
    <div className="card p-5">
      <div className="flex items-start justify-between mb-4">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-full bg-primary-900/50 flex items-center justify-center text-primary-400">
            {loanTypeIcon[loan.loanType] ?? <CreditCard size={20} />}
          </div>
          <div>
            <div className="font-semibold text-dark-100">{account.name}</div>
            <div className="text-xs text-dark-400">
              {loanTypeLabel[loan.loanType] ?? 'Loan'}
              {loan.lenderName ? ` · ${loan.lenderName}` : ''}
            </div>
          </div>
        </div>
        <div className="text-right">
          <div className="text-lg font-bold text-danger">{formatCurrency(loan.outstandingBalance)}</div>
          <div className="text-xs text-dark-400">outstanding</div>
        </div>
      </div>

      {/* Progress bar */}
      <div className="mb-3">
        <div className="flex justify-between text-xs text-dark-400 mb-1">
          <span>Paid {formatCurrency(paid)}</span>
          <span>{paidPct.toFixed(1)}% paid off</span>
        </div>
        <div className="progress-bar">
          <div
            className="progress-fill bg-success"
            style={{ width: `${paidPct}%` }}
          />
        </div>
      </div>

      {/* Key stats */}
      <div className="grid grid-cols-3 gap-3 mt-4">
        <div className="text-center p-2 bg-dark-700/50 rounded-lg">
          <div className="text-xs text-dark-400 mb-1">Monthly</div>
          <div className="text-sm font-semibold text-dark-100">{formatCurrency(loan.monthlyPayment)}</div>
        </div>
        <div className="text-center p-2 bg-dark-700/50 rounded-lg">
          <div className="text-xs text-dark-400 mb-1">Interest</div>
          <div className="text-sm font-semibold text-dark-100">{loan.interestRate?.toFixed(2)}%</div>
        </div>
        <div className="text-center p-2 bg-dark-700/50 rounded-lg">
          <div className="text-xs text-dark-400 mb-1">Remaining</div>
          <div className="text-sm font-semibold text-dark-100">{remaining} mo.</div>
        </div>
      </div>

      {/* Mortgage extra info */}
      {loan.loanType === 3 && loan.propertyAddress && (
        <div className="mt-3 p-2 bg-dark-700/30 rounded text-xs text-dark-400 flex items-center gap-1">
          <Home size={12} />
          <span>{loan.propertyAddress}</span>
          {loan.propertyValue ? <span className="ml-auto">{formatCurrency(loan.propertyValue)}</span> : null}
        </div>
      )}

      {isOverdue && (
        <div className="mt-3 flex items-center gap-2 text-danger text-xs">
          <AlertCircle size={12} />
          <span>Payment may be overdue</span>
        </div>
      )}
    </div>
  )
}

export default function LoansPage() {
  const { data: accounts = [], isLoading } = useQuery({
    queryKey: ['accounts'],
    queryFn: accountsService.getAll,
  })

  const loans = accounts.filter(a => a.accountType === 5)

  const totalDebt = loans.reduce((s, a) => s + (a.loan?.outstandingBalance ?? 0), 0)
  const totalMonthly = loans.reduce((s, a) => s + (a.loan?.monthlyPayment ?? 0), 0)
  const totalInterestPaid = loans.reduce((s, a) => s + (a.loan?.totalInterestPaid ?? 0), 0)

  if (isLoading) return (
    <div className="flex items-center justify-center h-48">
      <div className="w-8 h-8 border-2 border-primary-500 border-t-transparent rounded-full animate-spin" />
    </div>
  )

  return (
    <div className="space-y-6">
      {/* Summary cards */}
      <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div className="stat-card">
          <div className="stat-icon bg-danger/20">
            <TrendingDown size={20} className="text-danger" />
          </div>
          <div>
            <div className="text-xs text-dark-400">Total Outstanding Debt</div>
            <div className="text-xl font-bold text-danger">{formatCurrency(totalDebt)}</div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon bg-warning/20">
            <Calendar size={20} className="text-warning" />
          </div>
          <div>
            <div className="text-xs text-dark-400">Total Monthly Payments</div>
            <div className="text-xl font-bold text-dark-100">{formatCurrency(totalMonthly)}</div>
          </div>
        </div>
        <div className="stat-card">
          <div className="stat-icon bg-primary-900/50">
            <Percent size={20} className="text-primary-400" />
          </div>
          <div>
            <div className="text-xs text-dark-400">Total Interest Paid</div>
            <div className="text-xl font-bold text-dark-100">{formatCurrency(totalInterestPaid)}</div>
          </div>
        </div>
      </div>

      {/* Loans list */}
      {loans.length === 0 ? (
        <div className="card text-center py-16">
          <CreditCard size={48} className="mx-auto mb-4 text-dark-600" />
          <h2 className="text-dark-200 font-semibold text-lg mb-2">No Loans Yet</h2>
          <p className="text-dark-400 text-sm max-w-md mx-auto">
            Add a Loan account from the <a href="/accounts" className="text-primary-400 hover:underline">Accounts page</a> to
            start tracking your mortgages, personal loans, auto loans, and other debts.
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {loans.map(a => <LoanCard key={a.id} account={a} />)}
        </div>
      )}
    </div>
  )
}
