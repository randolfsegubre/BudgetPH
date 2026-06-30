import { useState } from 'react'
import { useAuthStore } from '../../store/authStore'
import { authService } from '../../services'
import { useForm } from 'react-hook-form'
import toast from 'react-hot-toast'
import { Settings, User, Lock, Bell, Globe } from 'lucide-react'

const currencies = [
  { value: 'PHP', label: '₱ Philippine Peso (PHP)' },
  { value: 'USD', label: '$ US Dollar (USD)' },
  { value: 'EUR', label: '€ Euro (EUR)' },
  { value: 'GBP', label: '£ British Pound (GBP)' },
  { value: 'SGD', label: 'S$ Singapore Dollar (SGD)' },
  { value: 'AED', label: 'AED UAE Dirham (AED)' },
  { value: 'SAR', label: 'SAR Saudi Riyal (SAR)' },
]

export default function SettingsPage() {
  const { user, updateUser } = useAuthStore()
  const [tab, setTab] = useState('profile')
  const [saving, setSaving] = useState(false)

  const { register, handleSubmit, formState: { errors } } = useForm({
    defaultValues: { firstName: user?.firstName, lastName: user?.lastName, phoneNumber: user?.phoneNumber, preferredCurrency: user?.preferredCurrency, timeZone: user?.timeZone ?? 'Asia/Manila' }
  })

  const onProfileSave = async (data) => {
    setSaving(true)
    try {
      const updated = await authService.updateProfile(data)
      updateUser(updated)
      toast.success('Profile updated!')
    } catch { toast.error('Failed to update profile') }
    finally { setSaving(false) }
  }

  const tabs = [
    { id: 'profile', label: 'Profile', icon: User },
    { id: 'security', label: 'Security', icon: Lock },
    { id: 'preferences', label: 'Preferences', icon: Globe },
  ]

  return (
    <div className="max-w-2xl space-y-6">
      {/* Tabs */}
      <div className="flex gap-1 p-1 bg-dark-800 rounded-xl border border-dark-700">
        {tabs.map(t => (
          <button key={t.id} onClick={() => setTab(t.id)}
            className={`flex-1 flex items-center justify-center gap-2 py-2 px-3 rounded-lg text-sm font-medium transition-all
              ${tab === t.id ? 'bg-primary-600 text-white shadow-glow' : 'text-dark-400 hover:text-dark-200'}`}>
            <t.icon size={14} />
            {t.label}
          </button>
        ))}
      </div>

      {tab === 'profile' && (
        <div className="card">
          <h3 className="text-dark-100 font-semibold mb-5">Profile Information</h3>
          <form onSubmit={handleSubmit(onProfileSave)} className="space-y-4">
            <div className="flex items-center gap-4 mb-6">
              <div className="w-16 h-16 rounded-2xl bg-gradient-purple flex items-center justify-center text-2xl font-bold text-white">
                {user?.firstName?.[0]}{user?.lastName?.[0]}
              </div>
              <div>
                <p className="text-dark-200 font-medium">{user?.firstName} {user?.lastName}</p>
                <p className="text-dark-400 text-sm">{user?.email}</p>
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="label">First Name</label>
                <input {...register('firstName', { required: true })} className="input" />
              </div>
              <div>
                <label className="label">Last Name</label>
                <input {...register('lastName', { required: true })} className="input" />
              </div>
            </div>
            <div>
              <label className="label">Phone Number</label>
              <input {...register('phoneNumber')} className="input" placeholder="+63 9XX XXX XXXX" />
            </div>
            <div>
              <label className="label">Preferred Currency</label>
              <select {...register('preferredCurrency')} className="input">
                {currencies.map(c => <option key={c.value} value={c.value}>{c.label}</option>)}
              </select>
            </div>
            <div>
              <label className="label">Timezone</label>
              <select {...register('timeZone')} className="input">
                <option value="Asia/Manila">Asia/Manila (PHT, UTC+8)</option>
                <option value="UTC">UTC</option>
                <option value="America/New_York">America/New York (EST)</option>
                <option value="Europe/London">Europe/London (GMT)</option>
                <option value="Asia/Dubai">Asia/Dubai (GST)</option>
                <option value="Asia/Riyadh">Asia/Riyadh (AST)</option>
              </select>
            </div>
            <button type="submit" disabled={saving} className="btn-primary">
              {saving ? 'Saving…' : 'Save Changes'}
            </button>
          </form>
        </div>
      )}

      {tab === 'security' && (
        <div className="card">
          <h3 className="text-dark-100 font-semibold mb-5">Security</h3>
          <p className="text-dark-400 text-sm mb-6">Change your password to keep your account secure.</p>
          <div className="space-y-4">
            <div><label className="label">Current Password</label><input type="password" className="input" /></div>
            <div><label className="label">New Password</label><input type="password" className="input" /></div>
            <div><label className="label">Confirm New Password</label><input type="password" className="input" /></div>
            <button className="btn-primary">Update Password</button>
          </div>
        </div>
      )}

      {tab === 'preferences' && (
        <div className="card">
          <h3 className="text-dark-100 font-semibold mb-5">Preferences</h3>
          <p className="text-dark-400 text-sm">Notification and display preferences — coming soon.</p>
        </div>
      )}
    </div>
  )
}
