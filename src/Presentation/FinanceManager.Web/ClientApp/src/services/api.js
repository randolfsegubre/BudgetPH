import axios from 'axios'
import { useAuthStore } from '../store/authStore'
import toast from 'react-hot-toast'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 30000,
})

// Attach JWT
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// Handle 401 — try refresh
let refreshing = false
api.interceptors.response.use(
  res => res,
  async (error) => {
    const original = error.config
    if (error.response?.status === 401 && !original._retry && !refreshing) {
      original._retry = true
      refreshing = true
      try {
        const { refreshToken, setAuth, clearAuth } = useAuthStore.getState()
        if (!refreshToken) throw new Error('No refresh token')
        const res = await axios.post('/api/auth/refresh', { refreshToken })
        setAuth(res.data)
        original.headers.Authorization = `Bearer ${res.data.accessToken}`
        return api(original)
      } catch {
        useAuthStore.getState().clearAuth()
        window.location.href = '/login'
      } finally {
        refreshing = false
      }
    }
    if (error.response?.status !== 401) {
      const msg = error.response?.data?.error || error.response?.data?.message || error.message
      toast.error(msg || 'Something went wrong')
    }
    return Promise.reject(error)
  }
)

export default api
