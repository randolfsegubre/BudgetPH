// ============================================================================
// The one shared axios instance every page's data-fetching hook imports and
// calls through — never a raw axios.get/post directly in a page/component.
// Centralizing it here is what makes the two interceptors below apply to
// every request/response in the app automatically, with no per-call opt-in.
// ============================================================================
import axios from 'axios'
import { useAuthStore } from '../store/authStore'
import toast from 'react-hot-toast'

const api = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
  timeout: 30000,
})

// [interceptor] Attach JWT — runs before every outgoing request. Reads the
// token straight out of the Zustand store (see ../store/authStore.js)
// rather than a prop/closure, so this file doesn't need to be a React
// component or hook to reach the current token.
api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().token
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})

// [interceptor] Handle 401 — try refresh, then retry the original request
// exactly once. `refreshing` is a module-level flag (not per-request) so
// that if several requests 401 at the same moment, only the first one
// triggers a refresh call; `original._retry` stops the retried request
// itself from re-triggering this same logic in an infinite loop.
let refreshing = false
api.interceptors.response.use(
  res => res,
  async (error) => {
    const original = error.config
    if (error.response?.status === 401 && !original._retry && !refreshing) {
      original._retry = true
      refreshing = true
      try {
        // STEP 1 of 3 - ask the backend for a new access token using the
        // stored refresh token. Uses plain `axios`, not `api`, deliberately:
        // going through `api` would re-attach the now-expired access token
        // and re-trigger this same 401 interceptor.
        const { refreshToken, setAuth, clearAuth } = useAuthStore.getState()
        if (!refreshToken) throw new Error('No refresh token')
        const res = await axios.post('/api/auth/refresh', { refreshToken })
        // STEP 2 of 3 - save the new token pair, then retry the ORIGINAL
        // request (the one that got the 401) with the new token attached.
        setAuth(res.data)
        original.headers.Authorization = `Bearer ${res.data.accessToken}`
        return api(original)
      } catch {
        // STEP 3 of 3 - refresh itself failed (refresh token missing or
        // also expired) - there's no way to recover, so log out for real.
        useAuthStore.getState().clearAuth()
        window.location.href = '/login'
      } finally {
        refreshing = false
      }
    }
    // Any other error (not a 401, or a 401 that already retried once):
    // surface it as a toast so no page has to handle its own error display.
    if (error.response?.status !== 401) {
      const msg = error.response?.data?.error || error.response?.data?.message || error.message
      toast.error(msg || 'Something went wrong')
    }
    return Promise.reject(error)
  }
)

export default api
