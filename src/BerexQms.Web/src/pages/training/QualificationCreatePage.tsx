import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import styles from './QualificationCreatePage.module.css'

export function QualificationCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)

  const [form, setForm] = useState({
    code: '',
    name: '',
    description: '',
    scopeProductFamily: '',
    scopeInspectionType: '',
    scopeProcessArea: '',
    validityMonths: '12',
    renewalWindowDays: '30',
  })

  const mutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/api/v1/qualifications', {
        code: form.code,
        name: form.name,
        description: form.description || null,
        scopeProductFamily: form.scopeProductFamily || null,
        scopeInspectionType: form.scopeInspectionType || null,
        scopeProcessArea: form.scopeProcessArea || null,
        validityMonths: parseInt(form.validityMonths, 10),
        renewalWindowDays: parseInt(form.renewalWindowDays, 10),
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['qualifications'] })
      navigate(`/training/qualifications/${data.id}`)
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error
      setError(msg ?? 'Failed to create qualification.')
    },
  })

  const handleChange = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm(prev => ({ ...prev, [field]: e.target.value }))
    setError(null)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!form.code.trim() || !form.name.trim()) {
      setError('Code and name are required.')
      return
    }
    if (parseInt(form.validityMonths, 10) <= 0) {
      setError('Validity period must be greater than zero.')
      return
    }
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <button
          type="button"
          className={styles.backButton}
          onClick={() => navigate('/training')}
        >
          <ArrowLeft size={16} />
        </button>
        <h1 className={styles.title}>New Qualification</h1>
      </div>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form onSubmit={handleSubmit} className={styles.form}>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Qualification Details</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Code"
              value={form.code}
              onChange={handleChange('code')}
              required
              placeholder="e.g., QUAL-VIS-001"
            />
            <Input
              label="Name"
              value={form.name}
              onChange={handleChange('name')}
              required
              placeholder="Qualification name"
            />
            <div className={styles.fieldFull}>
              <Input
                label="Description"
                value={form.description}
                onChange={handleChange('description')}
                placeholder="Describe the qualification scope and requirements"
              />
            </div>
          </div>
        </div>

        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Scope</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Product Family"
              value={form.scopeProductFamily}
              onChange={handleChange('scopeProductFamily')}
              placeholder="e.g., Automotive Castings"
            />
            <Input
              label="Inspection Type"
              value={form.scopeInspectionType}
              onChange={handleChange('scopeInspectionType')}
              placeholder="e.g., Dimensional"
            />
            <Input
              label="Process Area"
              value={form.scopeProcessArea}
              onChange={handleChange('scopeProcessArea')}
              placeholder="e.g., CNC Machining"
            />
          </div>
        </div>

        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Validity</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Validity Period (months)"
              type="number"
              value={form.validityMonths}
              onChange={handleChange('validityMonths')}
              required
              min="1"
            />
            <Input
              label="Renewal Window (days)"
              type="number"
              value={form.renewalWindowDays}
              onChange={handleChange('renewalWindowDays')}
              min="0"
            />
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/training')}>
            Cancel
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Creating...' : 'Create Qualification'}
          </Button>
        </div>
      </form>
    </div>
  )
}
