import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { useAuthStore } from '../../store/authStore'
import { authService } from '../../services'
import { Eye, EyeOff, UserPlus } from 'lucide-react'
import toast from 'react-hot-toast'

export default function RegisterPage() {
  const [showPassword, setShowPassword] = useState(false)
  const [loading, setLoading] = useState(false)
  const { setAuth } = useAuthStore()
  const navigate = useNavigate()

  const { register, handleSubmit, watch, formState: { errors } } = useForm()
  const password = watch('password')

  const onSubmit = async (data) => {
    setLoading(true)
    try {
      const res = await authService.register(data)
      setAuth(res)
      toast.success(`Welcome to BudgetPH, ${res.user.firstName}! 🎉`)
      navigate('/')
    } catch (err) {
      toast.error(err.response?.data?.error || 'Registration failed')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div>
      <h2 className="text-2xl font-bold text-white mb-1">Create your account</h2>
      <p className="text-dark-400 text-sm mb-8">Start managing your finances today — it's free</p>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="label">First name</label>
            <input {...register('firstName', { required: 'Required' })}
              className={errors.firstName ? 'input-error' : 'input'} placeholder="Juan" />
            {errors.firstName && <p className="text-danger text-xs mt-1">{errors.firstName.message}</p>}
          </div>
          <div>
            <label className="label">Last name</label>
            <input {...register('lastName', { required: 'Required' })}
              className={errors.lastName ? 'input-error' : 'input'} placeholder="dela Cruz" />
            {errors.lastName && <p className="text-danger text-xs mt-1">{errors.lastName.message}</p>}
          </div>
        </div>

        <div>
          <label className="label">Email address</label>
          <input {...register('email', {
            required: 'Email is required',
            pattern: { value: /\S+@\S+\.\S+/, message: 'Invalid email' }
          })} type="email" className={errors.email ? 'input-error' : 'input'} placeholder="juan@example.com" />
          {errors.email && <p className="text-danger text-xs mt-1">{errors.email.message}</p>}
        </div>

        <div>
          <label className="label">Phone number <span className="text-dark-500">(optional)</span></label>
          <input {...register('phoneNumber')} type="tel" className="input" placeholder="+63 9XX XXX XXXX" />
        </div>

        <div>
          <label className="label">Password</label>
          <div className="relative">
            <input {...register('password', {
              required: 'Password is required',
              minLength: { value: 8, message: 'Minimum 8 characters' }
            })} type={showPassword ? 'text' : 'password'}
              className={`${errors.password ? 'input-error' : 'input'} pr-10`} placeholder="Minimum 8 characters" />
            <button type="button" onClick={() => setShowPassword(v => !v)}
              className="absolute right-3 top-1/2 -translate-y-1/2 text-dark-500 hover:text-dark-300">
              {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
            </button>
          </div>
          {errors.password && <p className="text-danger text-xs mt-1">{errors.password.message}</p>}
        </div>

        <div>
          <label className="label">Confirm password</label>
          <input {...register('confirmPassword', {
            required: 'Please confirm your password',
            validate: v => v === password || 'Passwords do not match'
          })} type="password" className={errors.confirmPassword ? 'input-error' : 'input'} placeholder="••••••••" />
          {errors.confirmPassword && <p className="text-danger text-xs mt-1">{errors.confirmPassword.message}</p>}
        </div>

        <button type="submit" disabled={loading} className="btn-primary w-full justify-center py-2.5 text-base mt-2">
          {loading ? (
            <span className="flex items-center gap-2">
              <span className="w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin" />
              Creating account…
            </span>
          ) : (
            <span className="flex items-center gap-2"><UserPlus size={16} /> Create account</span>
          )}
        </button>
      </form>

      <p className="text-center text-dark-400 text-sm mt-6">
        Already have an account?{' '}
        <Link to="/login" className="text-primary-400 hover:text-primary-300 font-medium">Sign in</Link>
      </p>
    </div>
  )
}
