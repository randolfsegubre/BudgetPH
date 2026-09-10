import { Routes, Route, Navigate } from 'react-router-dom'
import { useAuthStore } from './store/authStore'
import AppLayout from './components/layout/AppLayout'
import AuthLayout from './components/layout/AuthLayout'
import LoginPage from './pages/Auth/LoginPage'
import RegisterPage from './pages/Auth/RegisterPage'
import DashboardPage from './pages/Dashboard/DashboardPage'
import AccountsPage from './pages/Accounts/AccountsPage'
import TransactionsPage from './pages/Transactions/TransactionsPage'
import BudgetsPage from './pages/Budgets/BudgetsPage'
import LoansPage from './pages/Loans/LoansPage'
import InvestmentsPage from './pages/Investments/InvestmentsPage'
import SavingsPage from './pages/Savings/SavingsPage'
import ReportsPage from './pages/Reports/ReportsPage'
import SettingsPage from './pages/Settings/SettingsPage'
import NotFoundPage from './pages/NotFoundPage'

function PrivateRoute({ children }) {
  const token = useAuthStore(s => s.token)
  return token ? children : <Navigate to="/login" replace />
}

function PublicRoute({ children }) {
  const token = useAuthStore(s => s.token)
  return token ? <Navigate to="/" replace /> : children
}

export default function App() {
  return (
    <Routes>
      {/* Public */}
      <Route element={<PublicRoute><AuthLayout /></PublicRoute>}>
        <Route path="/login"    element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>

      {/* Protected */}
      <Route element={<PrivateRoute><AppLayout /></PrivateRoute>}>
        <Route path="/"             element={<DashboardPage />} />
        <Route path="/accounts"     element={<AccountsPage />} />
        <Route path="/transactions" element={<TransactionsPage />} />
        <Route path="/budgets"      element={<BudgetsPage />} />
        <Route path="/loans"        element={<LoansPage />} />
        <Route path="/investments"  element={<InvestmentsPage />} />
        <Route path="/savings"      element={<SavingsPage />} />
        <Route path="/reports"      element={<ReportsPage />} />
        <Route path="/settings"     element={<SettingsPage />} />
      </Route>

      {/* A real 404 page, not a silent redirect - a bad URL should say so, not just bounce home. */}
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  )
}
