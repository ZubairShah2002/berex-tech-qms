import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import styles from './AuditCreatePage.module.css'

export function AuditCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [form, setForm] = useState({
    planName: '',
    year: String(new Date().getFullYear()),
    description: '',
    scope: '',
  })
  const [error, setError] = useState('')

  const createMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/audits', {
        planName: form.planName,
        year: Number(form.year),
        description: form.description || null,
        scope: form.scope || null,
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['audits'] })
      navigate(`/audits/${data.id}`)
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setError(
        axiosErr.response?.data?.detail ??
          axiosErr.response?.data?.error ??
          'Failed to create audit plan.'
      )
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    createMutation.mutate()
  }

  return (
    <div className={styles.page}>
      <button className={styles.backLink} onClick={() => navigate('/audits')}>
        <ArrowLeft size={16} />
        Back to Audit Management
      </button>

      <h1 className={styles.title}>Create Audit Plan</h1>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form className={styles.form} onSubmit={handleSubmit}>
        <div>
          <h2 className={styles.sectionTitle}>Plan Details</h2>
          <div className={styles.formGrid}>
            <Input
              label="Plan Name"
              value={form.planName}
              onChange={(e) => setForm((f) => ({ ...f, planName: e.target.value }))}
              required
              placeholder="e.g. Annual Internal Audit 2026"
            />
            <Input
              label="Year"
              type="number"
              value={form.year}
              onChange={(e) => setForm((f) => ({ ...f, year: e.target.value }))}
              required
              min={2021}
              max={2099}
            />
            <div className={styles.descriptionField}>
              <label className={styles.fieldLabel}>Description</label>
              <textarea
                className={styles.textarea}
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                rows={3}
                placeholder="Describe the purpose and objectives of this audit plan..."
              />
            </div>
            <div className={styles.descriptionField}>
              <label className={styles.fieldLabel}>Scope</label>
              <textarea
                className={styles.textarea}
                value={form.scope}
                onChange={(e) => setForm((f) => ({ ...f, scope: e.target.value }))}
                rows={3}
                placeholder="Define the scope of audits under this plan..."
              />
            </div>
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/audits')}>
            Cancel
          </Button>
          <Button type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Create Plan'}
          </Button>
        </div>
      </form>
    </div>
  )
}
