import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './SpcCreatePage.module.css'

const chartTypeOptions = [
  { value: 'XBarR', label: 'X̄/R (Subgroup Mean & Range)' },
  { value: 'XBarS', label: 'X̄/S (Subgroup Mean & Std Dev)' },
  { value: 'IndividualMovingRange', label: 'I/MR (Individual & Moving Range)' },
  { value: 'PChart', label: 'p Chart (Proportion Defective)' },
  { value: 'NpChart', label: 'np Chart (Count Defective)' },
  { value: 'CChart', label: 'c Chart (Defects per Unit)' },
  { value: 'UChart', label: 'u Chart (Defect Rate)' },
]

export function SpcCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)

  const [form, setForm] = useState({
    code: '',
    name: '',
    chartType: 'XBarR',
    partId: '',
    characteristicName: '',
    subgroupSize: '5',
    upperSpecLimit: '',
    lowerSpecLimit: '',
  })

  const mutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/api/v1/spc/charts', {
        code: form.code,
        name: form.name,
        chartType: form.chartType,
        partId: form.partId,
        characteristicName: form.characteristicName,
        subgroupSize: Number(form.subgroupSize),
        upperSpecLimit: form.upperSpecLimit ? Number(form.upperSpecLimit) : null,
        lowerSpecLimit: form.lowerSpecLimit ? Number(form.lowerSpecLimit) : null,
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['spc-charts'] })
      navigate(`/spc/${data.id}`)
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error
      setError(msg ?? 'Failed to create control chart.')
    },
  })

  const handleChange = (field: string) => (e: React.ChangeEvent<HTMLInputElement>) => {
    setForm(prev => ({ ...prev, [field]: e.target.value }))
    setError(null)
  }

  const handleSelectChange = (field: string) => (e: React.ChangeEvent<HTMLSelectElement>) => {
    setForm(prev => ({ ...prev, [field]: e.target.value }))
    setError(null)
  }

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    if (!form.code.trim() || !form.name.trim() || !form.partId.trim() || !form.characteristicName.trim()) {
      setError('Code, name, part ID, and characteristic name are required.')
      return
    }
    if (Number(form.subgroupSize) < 1) {
      setError('Subgroup size must be at least 1.')
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
          onClick={() => navigate('/spc')}
        >
          <ArrowLeft size={16} />
        </button>
        <h1 className={styles.title}>New Control Chart</h1>
      </div>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form onSubmit={handleSubmit} className={styles.form}>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Chart Definition</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Chart Code"
              value={form.code}
              onChange={handleChange('code')}
              required
              placeholder="e.g., SPC-001"
            />
            <Input
              label="Name"
              value={form.name}
              onChange={handleChange('name')}
              required
              placeholder="Chart name"
            />
            <Select
              label="Chart Type"
              value={form.chartType}
              onChange={handleSelectChange('chartType')}
              options={chartTypeOptions}
            />
            <Input
              label="Subgroup Size"
              type="number"
              value={form.subgroupSize}
              onChange={handleChange('subgroupSize')}
              required
              min={1}
              placeholder="5"
            />
          </div>
        </div>

        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Measurement Characteristic</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Part ID"
              value={form.partId}
              onChange={handleChange('partId')}
              required
              placeholder="Part UUID"
            />
            <Input
              label="Characteristic Name"
              value={form.characteristicName}
              onChange={handleChange('characteristicName')}
              required
              placeholder="e.g., Diameter, Length, Weight"
            />
          </div>
        </div>

        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Specification Limits (Optional)</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Upper Specification Limit (USL)"
              type="number"
              step="any"
              value={form.upperSpecLimit}
              onChange={handleChange('upperSpecLimit')}
              placeholder="Optional"
            />
            <Input
              label="Lower Specification Limit (LSL)"
              type="number"
              step="any"
              value={form.lowerSpecLimit}
              onChange={handleChange('lowerSpecLimit')}
              placeholder="Optional"
            />
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/spc')}>
            Cancel
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Creating...' : 'Create Chart'}
          </Button>
        </div>
      </form>
    </div>
  )
}
