import { type FormEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Alert } from '@/components/feedback/Alert'
import { useAuthStore } from '@/stores/auth-store'
import { apiClient } from '@/lib/api-client'
import styles from './LoginPage.module.css'

interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresAt: string
  user: {
    id: string
    email: string
    firstName: string
    lastName: string
    displayName: string
    status: string
    roles: string[]
  }
}

export function LoginPage() {
  const navigate = useNavigate()
  const setAuth = useAuthStore((s) => s.setAuth)
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setLoading(true)

    try {
      const response = await apiClient.post<LoginResponse>('/auth/login', {
        email,
        password,
      })

      const { accessToken, user } = response.data

      setAuth(
        {
          id: user.id,
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName,
          roles: user.roles,
          tenantId: '',
        },
        accessToken,
      )

      navigate('/', { replace: true })
    } catch (err: unknown) {
      const axiosErr = err as { response?: { data?: { error?: string } } }
      setError(
        axiosErr.response?.data?.error ?? 'Login failed. Please try again.',
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className={styles.page}>
      <div className={styles.card}>
        <div className={styles.header}>
          <h1 className={styles.title}>Berex Tech QMS</h1>
          <p className={styles.subtitle}>Quality Management System</p>
        </div>

        <form onSubmit={handleSubmit} className={styles.form}>
          {error && <Alert variant="error">{error}</Alert>}

          <Input
            label="Email"
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            autoComplete="email"
            autoFocus
          />

          <Input
            label="Password"
            type="password"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            autoComplete="current-password"
          />

          <Button type="submit" fullWidth disabled={loading}>
            {loading ? 'Signing in...' : 'Sign in'}
          </Button>
        </form>

        <p className={styles.footer}>
          Enterprise Quality Management for Discrete Manufacturing
        </p>
      </div>
    </div>
  )
}
