import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './InspectionCreatePage.module.css'

const typeOptions = [
  { value: 'IQC', label: 'IQC — Incoming Quality' },
  { value: 'IPQC', label: 'IPQC — In-Process Quality' },
  { value: 'OQC', label: 'OQC — Outgoing Quality' },
]

export function InspectionCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [form, setForm] = useState({
    inspectionNumber: '',
    type: 'IQC',
    partId: '',
    partRevisionId: '',
    lotNumber: '',
    lotSize: '',
    sampleSize: '',
    supplierId: '',
    samplingPlanId: '',
    inspectorId: '',
  })
  const [error, setError] = useState('')

  const createMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/inspections', {
        inspectionNumber: form.inspectionNumber,
        type: form.type,
        partId: form.partId,
        partRevisionId: form.partRevisionId || null,
        lotNumber: form.lotNumber || null,
        lotSize: form.lotSize ? Number(form.lotSize) : null,
        sampleSize: form.sampleSize ? Number(form.sampleSize) : null,
        supplierId: form.supplierId || null,
        samplingPlanId: form.samplingPlanId || null,
        inspectorId: form.inspectorId,
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['inspections'] })
      navigate(`/inspections/${data.id}`)
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setError(
        axiosErr.response?.data?.detail ??
          axiosErr.response?.data?.error ??
          'Failed to create inspection.'
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
      <button className={styles.backLink} onClick={() => navigate('/inspections')}>
        <ArrowLeft size={16} />
        Back to Inspections
      </button>

      <h1 className={styles.title}>New Inspection</h1>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.formGrid}>
          <Input
            label="Inspection Number"
            value={form.inspectionNumber}
            onChange={(e) => setForm((f) => ({ ...f, inspectionNumber: e.target.value }))}
            required
            placeholder="e.g. INS-2026-001"
          />
          <Select
            label="Inspection Type"
            options={typeOptions}
            value={form.type}
            onChange={(e) => setForm((f) => ({ ...f, type: e.target.value }))}
          />
          <Input
            label="Part ID"
            value={form.partId}
            onChange={(e) => setForm((f) => ({ ...f, partId: e.target.value }))}
            required
            placeholder="Part UUID"
          />
          <Input
            label="Part Revision ID"
            value={form.partRevisionId}
            onChange={(e) => setForm((f) => ({ ...f, partRevisionId: e.target.value }))}
            placeholder="Optional revision UUID"
          />
          <Input
            label="Lot Number"
            value={form.lotNumber}
            onChange={(e) => setForm((f) => ({ ...f, lotNumber: e.target.value }))}
            placeholder="e.g. LOT-001"
          />
          <Input
            label="Lot Size"
            type="number"
            value={form.lotSize}
            onChange={(e) => setForm((f) => ({ ...f, lotSize: e.target.value }))}
            placeholder="Total lot quantity"
          />
          <Input
            label="Sample Size"
            type="number"
            value={form.sampleSize}
            onChange={(e) => setForm((f) => ({ ...f, sampleSize: e.target.value }))}
            placeholder="Number of samples"
          />
          <Input
            label="Inspector ID"
            value={form.inspectorId}
            onChange={(e) => setForm((f) => ({ ...f, inspectorId: e.target.value }))}
            required
            placeholder="Inspector user ID"
          />
          <Input
            label="Supplier ID"
            value={form.supplierId}
            onChange={(e) => setForm((f) => ({ ...f, supplierId: e.target.value }))}
            placeholder="Optional supplier UUID"
          />
          <Input
            label="Sampling Plan ID"
            value={form.samplingPlanId}
            onChange={(e) => setForm((f) => ({ ...f, samplingPlanId: e.target.value }))}
            placeholder="Optional sampling plan UUID"
          />
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/inspections')}>
            Cancel
          </Button>
          <Button type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Create Inspection'}
          </Button>
        </div>
      </form>
    </div>
  )
}
