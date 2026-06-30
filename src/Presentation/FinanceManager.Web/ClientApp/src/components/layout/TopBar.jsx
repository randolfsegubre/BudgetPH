import { useLocation } from 'react-router-dom'
import { useAuthStore } from '../../store/authStore'
import { Menu, Bell, Search } from 'lucide-react'

const pageTitles = {
  '/':             'Dashboard',
  '/accounts':     'Accounts',
  '/transactions': 'Transactions',
  '/budgets':      'Budgets',
  '/loans':        'Loans & Debts',
  '/investments':  'Investments',
  '/savings':      'Savings Goals',
  '/reports':      'Reports',
  '/settings':     'Settings',
}

export default function TopBar({ onMenuClick }) {
  const { pathname } = useLocation()
  const user = useAuthStore(s => s.user)
  const title = pageTitles[pathname] ?? 'BudgetPH'

  return (
    <header className="h-16 flex items-center gap-3 px-4 lg:px-6 border-b border-dark-700 bg-dark-850/90 backdrop-blur-sm flex-shrink-0">
      <button onClick={onMenuClick} className="btn-icon lg:hidden">
        <Menu size={20} />
      </button>

      <h1 className="text-lg font-semibold text-dark-100 flex-1">{title}</h1>

      <div className="flex items-center gap-2">
        <button className="btn-icon relative">
          <Bell size={18} />
          <span className="absolute top-1.5 right-1.5 w-1.5 h-1.5 rounded-full bg-primary-500" />
        </button>
        <div className="w-8 h-8 rounded-full bg-gradient-purple flex items-center justify-center text-white text-xs font-bold">
          {user?.firstName?.[0]}{user?.lastName?.[0]}
        </div>
      </div>
    </header>
  )
}
