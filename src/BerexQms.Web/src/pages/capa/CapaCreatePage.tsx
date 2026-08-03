import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './CapaCreatePage.module.css'

const priorityOptions = [
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' },
]

const sourceTypeOptions = [
  { value: 'NonConformance', label: 'Non-Conformance' },
  { value: 'AuditFinding', label: 'Audit Finding' },
  { value: 'CustomerComplaint', label: 'Customer Complaint' },
  { value: 'Standalone', label: 'Standalone' },
]

export function CapaCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [form, setForm] = useState({
    capaNumber: '',
    title: '',
    description: '',
    priority: 'Medium',
    sourceType: 'NonConformance',
    sourceNonConformanceId: '',
    sourceAuditFindingId: '',
    sourceDescription: '',
    targetClosureDate: '',
  })
  const [error, setError] = useState('')

  const createMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/capas', {
        capaNumber: form.capaNumber,
        title: form.title,
        description: form.description,
        priority: form.priority,
        sourceType: form.sourceType,
        sourceNonConformanceId: form.sourceNonConformanceId || null,
        sourceAuditFindingId: form.sourceAuditFindingId || null,
        sourceDescription: form.sourceDescription || null,
        targetClosureDate: form.targetClosureDate || null,
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['capas'] })
      navigate(`/capa/${data.id}`)
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setError(
        axiosErr.response?.data?.detail ??
          axiosErr.response?.data?.error ??
          'Failed to create CAPA.'
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
      <button className={styles.backLink} onClick={() => navigate('/capa')}>
        <ArrowLeft size={16} />
        Back to CAPA Management
      </button>

      <h1 className={styles.title}>Initiate CAPA</h1>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form className={styles.form} onSubmit={handleSubmit}>
        <div>
          <h2 className={styles.sectionTitle}>General Information</h2>
          <div className={styles.formGrid}>
            <Input
              label="CAPA Number"
              value={form.capaNumber}
              onChange={(e) => setForm((f) => ({ ...f, capaNumber: e.target.value }))}
              required
              placeholder="e.g. CAPA-2026-001"
            />
            <Select
              label="Priority"
              options={priorityOptions}
              value={form.priority}
              onChange={(e) => setForm((f) => ({ ...f, priority: e.target.value }))}
            />
            <div className={styles.descriptionField}>
              <Input
                label="Title"
                value={form.title}
                onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
                required
                placeholder="Brief title for this CAPA"
              />
            </div>
            <div className={styles.descriptionField}>
              <label className={styles.fieldLabel}>Description</label>
              <textarea
                className={styles.textarea}
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                rows={4}
                placeholder="Describe the issue and reason for CAPA initiation..."
                required
              />
            </div>
          </div>
        </div>

        <div>
          <h2 className={styles.sectionTitle}>Source</h2>
          <div className={styles.formGrid}>
            <Select
              label="Source Type"
              options={sourceTypeOptions}
              value={form.sourceType}
              onChange={(e) => setForm((f) => ({ ...f, sourceType: e.target.value }))}
            />
            <Input
              label="Target Closure Date"
              type="date"
              value={form.targetClosureDate}
              onChange={(e) => setForm((f) => ({ ...f, targetClosureDate: e.target.value }))}
            />
            {form.sourceType === 'NonConformance' && (
              <Input
                label="Source NC ID"
                value={form.sourceNonConformanceId}
                onChange={(e) => setForm((f) => ({ ...f, sourceNonConformanceId: e.target.value }))}
                placeholder="Non-Conformance UUID"
              />
            )}
            {form.sourceType === 'AuditFinding' && (
              <Input
                label="Source Audit Finding ID"
                value={form.sourceAuditFindingId}
                onChange={(e) => setForm((f) => ({ ...f, sourceAuditFindingId: e.target.value }))}
                placeholder="Audit Finding UUID"
              />
            )}
            <div className={styles.descriptionField}>
              <label className={styles.fieldLabel}>Source Description</label>
              <textarea
                className={styles.textarea}
                value={form.sourceDescription}
                onChange={(e) => setForm((f) => ({ ...f, sourceDescription: e.target.value }))}
                rows={2}
                placeholder="Additional details about the source..."
              />
            </div>
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/capa')}>
            Cancel
          </Button>
          <Button type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Initiate CAPA'}
          </Button>
        </div>
      </form>
    </div>
  )
}
