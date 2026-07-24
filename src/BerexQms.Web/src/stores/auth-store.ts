import { create } from 'zustand'

interface User {
  id: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
  tenantId: string
}

interface AuthState {
  user: User | null
  token: string | null
  isAuthenticated: boolean
  setAuth: (user: User, token: string) => void
  clearAuth: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: localStorage.getItem('auth_token'),
  isAuthenticated: !!localStorage.getItem('auth_token'),

  setAuth: (user, token) => {
    localStorage.setItem('auth_token', token)
    localStorage.setItem('tenant_id', user.tenantId)
    set({ user, token, isAuthenticated: true })
  },

  clearAuth: () => {
    localStorage.removeItem('auth_token')
    localStorage.removeItem('tenant_id')
    set({ user: null, token: null, isAuthenticated: false })
  },
}))
