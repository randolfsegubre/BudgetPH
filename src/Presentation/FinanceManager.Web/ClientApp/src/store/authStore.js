import { create } from 'zustand'
import { persist } from 'zustand/middleware'

export const useAuthStore = create(
  persist(
    (set, get) => ({
      token: null,
      refreshToken: null,
      expiresAt: null,
      user: null,

      setAuth: (authResponse) => set({
        token: authResponse.accessToken,
        refreshToken: authResponse.refreshToken,
        expiresAt: authResponse.expiresAt,
        user: authResponse.user,
      }),

      clearAuth: () => set({ token: null, refreshToken: null, expiresAt: null, user: null }),

      updateUser: (user) => set({ user }),

      isTokenExpired: () => {
        const { expiresAt } = get()
        if (!expiresAt) return true
        return new Date(expiresAt) <= new Date()
      },
    }),
    { name: 'budgetph-auth' }
  )
)
