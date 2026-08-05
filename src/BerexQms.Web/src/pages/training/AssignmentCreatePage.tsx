import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import styles from './QualificationCreatePage.module.css'

export function AssignmentCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)

  const [form, setForm] = useState({
    employeeId: '',
    courseId: '',
    dueDate: '',
  })

  const mutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/api/v1/training/assignments', {
        employeeId: form.employeeId,
        courseId: form.courseId,
        dueDate: form.dueDate,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['training-assignments'] })
      navigate('/training/assignments')
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error
      setError(msg ?? 'Failed to create assignment.')
    },
  })

  const handleChange = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm(prev => ({ ...prev, [field]: e.target.value }))
    setError(null)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!form.employeeId.trim() || !form.courseId.trim() || !form.dueDate) {
      setError('Employee ID, Course ID, and Due Date are required.')
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
          onClick={() => navigate('/training/assignments')}
        >
          <ArrowLeft size={16} />
        </button>
        <h1 className={styles.title}>New Training Assignment</h1>
      </div>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form onSubmit={handleSubmit} className={styles.form}>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Assignment Details</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Employee ID"
              value={form.employeeId}
              onChange={handleChange('employeeId')}
              required
              placeholder="Employee UUID"
            />
            <Input
              label="Course ID"
              value={form.courseId}
              onChange={handleChange('courseId')}
              required
              placeholder="Course UUID"
            />
            <Input
              label="Due Date"
              type="date"
              value={form.dueDate}
              onChange={handleChange('dueDate')}
              required
            />
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/training/assignments')}>
            Cancel
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Creating...' : 'Create Assignment'}
          </Button>
        </div>
      </form>
    </div>
  )
}
