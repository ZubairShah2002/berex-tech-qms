import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'

const apiClient = axios.create({
  baseURL: '/api/v1',
  headers: { 'Content-Type': 'application/json' },
})

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem('auth_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }

  const tenantId = localStorage.getItem('tenant_id')
  if (tenantId) {
    config.headers['X-Tenant-Id'] = tenantId
  }

  config.headers['X-Correlation-Id'] = crypto.randomUUID()

  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    if (error.response?.status === 401) {
      // Don't redirect if we're already on the login page or making a login request —
      // the login form handles its own 401 (invalid credentials) display.
      const isLoginRequest = error.config?.url?.includes('/auth/login')
      const isOnLoginPage = window.location.pathname === '/login'
      if (!isLoginRequest && !isOnLoginPage) {
        localStorage.removeItem('auth_token')
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  },
)

/**
 * Extract an error message from an API error response.
 * Handles both { error: "..." } and ProblemDetails { detail: "..." } formats.
 */
export function extractApiError(err: unknown, fallback = 'An unexpected error occurred.'): string {
  const axiosErr = err as { response?: { data?: Record<string, unknown> } }
  const data = axiosErr?.response?.data
  if (data) {
    if (typeof data.error === 'string') return data.error
    if (typeof data.detail === 'string') return data.detail
    if (typeof data.title === 'string') return data.title
  }
  return fallback
}

export { apiClient }
