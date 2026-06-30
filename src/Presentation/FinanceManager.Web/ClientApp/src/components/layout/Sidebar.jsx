import { NavLink, useNavigate } from 'react-router-dom'
import { useAuthStore } from '../../store/authStore'
import { authService } from '../../services'
import toast from 'react-hot-toast'
import {
  LayoutDashboard, CreditCard, ArrowLeftRight, PiggyBank,
  TrendingUp, Target, FileText, Settings, LogOut, Wallet,
  Menu, X, Building2
} from 'lucide-react'

const navItems = [
  { to: '/',             icon: LayoutDashboard,  label: 'Dashboard' },
  { to: '/accounts',     icon: Building2,         label: 'Accounts' },
  { to: '/transactions', icon: ArrowLeftRight,    label: 'Transactions' },
  { to: '/budgets',      icon: Wallet,            label: 'Budgets' },
  { to: '/loans',        icon: CreditCard,        label: 'Loans & Debts' },
  { to: '/investments',  icon: TrendingUp,        label: 'Investments' },
  { to: '/savings',      icon: PiggyBank,         label: 'Savings Goals' },
  { to: '/reports',      icon: FileText,          label: 'Reports' },
  { to: '/settings',     icon: Settings,          label: 'Settings' },
]

export default function Sidebar({ open, onClose }) {
  const { user, clearAuth } = useAuthStore()
  const navigate = useNavigate()

  const handleLogout = async () => {
    try { await authService.logout() } catch {}
    clearAuth()
    navigate('/login')
    toast.success('Logged out successfully')
  }

  return (
    <aside className={`
      fixed inset-y-0 left-0 z-30 w-64 bg-dark-850 border-r border-dark-700
      flex flex-col transition-transform duration-300 ease-in-out
      lg:relative lg:translate-x-0 lg:z-auto
      ${open ? 'translate-x-0' : '-translate-x-full'}
    `}>
      {/* Logo */}
      <div className="flex items-center gap-3 px-5 py-5 border-b border-dark-700">
        <div className="w-9 h-9 rounded-xl bg-gradient-purple flex items-center justify-center shadow-glow flex-shrink-0">
          <span className="text-white font-bold text-base">₱</span>
        </div>
        <div className="flex-1 min-w-0">
          <p className="font-bold text-white text-sm leading-none">BudgetPH</p>
          <p className="text-dark-500 text-xs mt-0.5 truncate">Finance Manager</p>
        </div>
        <button onClick={onClose} className="btn-icon lg:hidden">
          <X size={16} />
        </button>
      </div>

      {/* Nav */}
      <nav className="flex-1 overflow-y-auto p-3 space-y-0.5">
        {navItems.map(({ to, icon: Icon, label }) => (
          <NavLink
            key={to}
            to={to}
            end={to === '/'}
            onClick={onClose}
            className={({ isActive }) => `nav-item ${isActive ? 'active' : ''}`}
          >
            <Icon size={17} className="flex-shrink-0" />
            <span>{label}</span>
          </NavLink>
        ))}
      </nav>

      {/* User footer */}
      <div className="p-3 border-t border-dark-700">
        <div className="flex items-center gap-3 px-3 py-2 rounded-lg bg-dark-900/50 mb-1">
          <div className="w-8 h-8 rounded-full bg-gradient-purple flex items-center justify-center text-white text-xs font-bold flex-shrink-0">
            {user?.firstName?.[0]}{user?.lastName?.[0]}
          </div>
          <div className="flex-1 min-w-0">
            <p className="text-sm font-medium text-dark-100 truncate">{user?.firstName} {user?.lastName}</p>
            <p className="text-xs text-dark-500 truncate">{user?.email}</p>
          </div>
        </div>
        <button onClick={handleLogout} className="nav-item w-full text-danger hover:bg-danger/10 hover:text-danger mt-0.5">
          <LogOut size={16} />
          <span>Log out</span>
        </button>
      </div>
    </aside>
  )
}
