import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './DocumentCreatePage.module.css'

const documentTypeOptions = [
  { value: 'Procedure', label: 'Procedure' },
  { value: 'WorkInstruction', label: 'Work Instruction' },
  { value: 'Specification', label: 'Specification' },
  { value: 'Form', label: 'Form' },
  { value: 'Template', label: 'Template' },
  { value: 'Policy', label: 'Policy' },
  { value: 'Manual', label: 'Manual' },
  { value: 'ExternalDocument', label: 'External Document' },
]

export function DocumentCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [form, setForm] = useState({
    documentNumber: '',
    title: '',
    documentType: 'Procedure',
    description: '',
    department: '',
  })
  const [error, setError] = useState('')

  const createMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/documents', {
        documentNumber: form.documentNumber,
        title: form.title,
        documentType: form.documentType,
        description: form.description || null,
        department: form.department || null,
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['documents'] })
      navigate(`/documents/${data.id}`)
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setError(
        axiosErr.response?.data?.detail ??
          axiosErr.response?.data?.error ??
          'Failed to create document.'
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
      <button className={styles.backLink} onClick={() => navigate('/documents')}>
        <ArrowLeft size={16} />
        Back to Document Control
      </button>

      <h1 className={styles.title}>Create Document</h1>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form className={styles.form} onSubmit={handleSubmit}>
        <div>
          <h2 className={styles.sectionTitle}>Document Information</h2>
          <div className={styles.formGrid}>
            <Input
              label="Document Number"
              value={form.documentNumber}
              onChange={(e) => setForm((f) => ({ ...f, documentNumber: e.target.value }))}
              required
              placeholder="e.g. SOP-2026-001"
            />
            <Select
              label="Document Type"
              options={documentTypeOptions}
              value={form.documentType}
              onChange={(e) => setForm((f) => ({ ...f, documentType: e.target.value }))}
            />
            <div className={styles.fullWidthField}>
              <Input
                label="Title"
                value={form.title}
                onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
                required
                placeholder="Document title"
              />
            </div>
            <Input
              label="Department"
              value={form.department}
              onChange={(e) => setForm((f) => ({ ...f, department: e.target.value }))}
              placeholder="e.g. Quality, Engineering"
            />
            <div className={styles.fullWidthField}>
              <label className={styles.fieldLabel}>Description</label>
              <textarea
                className={styles.textarea}
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                rows={4}
                placeholder="Brief description of the document's purpose and scope..."
              />
            </div>
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/documents')}>
            Cancel
          </Button>
          <Button type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Create Document'}
          </Button>
        </div>
      </form>
    </div>
  )
}
