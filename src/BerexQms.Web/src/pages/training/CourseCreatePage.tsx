import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import styles from './QualificationCreatePage.module.css'

export function CourseCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)

  const [form, setForm] = useState({
    code: '',
    name: '',
    description: '',
    durationHours: '1',
    assessmentType: '',
    passCriteria: '',
  })

  const mutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/api/v1/training/courses', {
        code: form.code,
        name: form.name,
        description: form.description || null,
        durationHours: parseFloat(form.durationHours),
        assessmentType: form.assessmentType || null,
        passCriteria: form.passCriteria || null,
        qualificationId: null,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['training-courses'] })
      navigate('/training/courses')
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error
      setError(msg ?? 'Failed to create course.')
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
    if (parseFloat(form.durationHours) <= 0) {
      setError('Duration must be greater than zero.')
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
          onClick={() => navigate('/training/courses')}
        >
          <ArrowLeft size={16} />
        </button>
        <h1 className={styles.title}>New Training Course</h1>
      </div>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form onSubmit={handleSubmit} className={styles.form}>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Course Details</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Course Code"
              value={form.code}
              onChange={handleChange('code')}
              required
              placeholder="e.g., TRN-VIS-001"
            />
            <Input
              label="Name"
              value={form.name}
              onChange={handleChange('name')}
              required
              placeholder="Course name"
            />
            <div className={styles.fieldFull}>
              <Input
                label="Description"
                value={form.description}
                onChange={handleChange('description')}
                placeholder="Course description and objectives"
              />
            </div>
          </div>
        </div>

        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Assessment</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Duration (hours)"
              type="number"
              value={form.durationHours}
              onChange={handleChange('durationHours')}
              required
              min="0.5"
              step="0.5"
            />
            <Input
              label="Assessment Type"
              value={form.assessmentType}
              onChange={handleChange('assessmentType')}
              placeholder="e.g., Written Exam, Practical"
            />
            <div className={styles.fieldFull}>
              <Input
                label="Pass Criteria"
                value={form.passCriteria}
                onChange={handleChange('passCriteria')}
                placeholder="e.g., Score ≥ 80%"
              />
            </div>
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/training/courses')}>
            Cancel
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Creating...' : 'Create Course'}
          </Button>
        </div>
      </form>
    </div>
  )
}
