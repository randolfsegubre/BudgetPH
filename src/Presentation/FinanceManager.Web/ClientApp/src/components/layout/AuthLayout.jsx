import { Outlet } from 'react-router-dom'

export default function AuthLayout() {
  return (
    <div className="min-h-screen bg-dark-900 flex">
      {/* Left branding panel */}
      <div className="hidden lg:flex lg:w-1/2 bg-gradient-to-br from-primary-950 via-primary-900 to-dark-900 
                      flex-col items-center justify-center p-12 relative overflow-hidden">
        {/* Decorative blobs */}
        <div className="absolute top-0 right-0 w-96 h-96 bg-primary-600/20 rounded-full blur-3xl -translate-y-1/2 translate-x-1/2" />
        <div className="absolute bottom-0 left-0 w-80 h-80 bg-gold-500/10 rounded-full blur-3xl translate-y-1/2 -translate-x-1/2" />

        <div className="relative z-10 text-center max-w-md">
          {/* Logo */}
          <div className="flex items-center justify-center gap-3 mb-8">
            <div className="w-14 h-14 rounded-2xl bg-gradient-purple flex items-center justify-center shadow-glow">
              <span className="text-2xl">₱</span>
            </div>
            <div className="text-left">
              <h1 className="text-3xl font-bold text-white">BudgetPH</h1>
              <p className="text-primary-300 text-sm">Personal Finance Manager</p>
            </div>
          </div>

          <h2 className="text-2xl font-semibold text-white mb-4 leading-snug">
            Take control of your<br />
            <span className="text-gradient-gold">financial future</span>
          </h2>
          <p className="text-dark-300 text-sm leading-relaxed">
            Track income, expenses, investments, loans and savings — all in one secure place.
            Designed for Filipinos, ready for the world.
          </p>

          {/* Feature bullets */}
          <div className="mt-8 space-y-3 text-left">
            {['📊 Smart budget tracking & alerts', '💳 Credit cards & loan management', 
              '📈 Investment portfolio tracking', '📑 PDF & Excel reports', 
              '🔒 Bank-level security'].map(f => (
              <div key={f} className="flex items-center gap-3 text-sm text-dark-300">
                <div className="w-1.5 h-1.5 rounded-full bg-gold-500 flex-shrink-0" />
                {f}
              </div>
            ))}
          </div>
        </div>
      </div>

      {/* Right form panel */}
      <div className="flex-1 flex items-center justify-center p-6">
        <div className="w-full max-w-md">
          {/* Mobile logo */}
          <div className="flex items-center gap-2 mb-8 lg:hidden">
            <div className="w-10 h-10 rounded-xl bg-gradient-purple flex items-center justify-center">
              <span className="text-lg">₱</span>
            </div>
            <span className="text-xl font-bold text-white">BudgetPH</span>
          </div>
          <Outlet />
        </div>
      </div>
    </div>
  )
}
