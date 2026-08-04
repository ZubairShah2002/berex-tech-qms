import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import styles from './EquipmentCreatePage.module.css'

export function EquipmentCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [error, setError] = useState<string | null>(null)

  const [form, setForm] = useState({
    code: '',
    name: '',
    type: '',
    manufacturer: '',
    model: '',
    serialNumber: '',
    location: '',
    department: '',
    area: '',
  })

  const mutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/api/v1/equipment', {
        code: form.code,
        name: form.name,
        type: form.type || null,
        manufacturer: form.manufacturer || null,
        model: form.model || null,
        serialNumber: form.serialNumber || null,
        location: form.location || null,
        department: form.department || null,
        area: form.area || null,
        custodianId: null,
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['equipment'] })
      navigate(`/calibration/${data.id}`)
    },
    onError: (err: unknown) => {
      const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error
      setError(msg ?? 'Failed to register equipment.')
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
    mutation.mutate()
  }

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <button
          type="button"
          className={styles.backButton}
          onClick={() => navigate('/calibration')}
        >
          <ArrowLeft size={16} />
        </button>
        <h1 className={styles.title}>Register Equipment</h1>
      </div>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form onSubmit={handleSubmit} className={styles.form}>
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Equipment Information</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Equipment Code"
              value={form.code}
              onChange={handleChange('code')}
              required
              placeholder="e.g., CAL-001"
            />
            <Input
              label="Name"
              value={form.name}
              onChange={handleChange('name')}
              required
              placeholder="Equipment name"
            />
            <Input
              label="Type"
              value={form.type}
              onChange={handleChange('type')}
              placeholder="e.g., Caliper, Micrometer"
            />
            <Input
              label="Manufacturer"
              value={form.manufacturer}
              onChange={handleChange('manufacturer')}
              placeholder="Manufacturer name"
            />
            <Input
              label="Model"
              value={form.model}
              onChange={handleChange('model')}
              placeholder="Model number"
            />
            <Input
              label="Serial Number"
              value={form.serialNumber}
              onChange={handleChange('serialNumber')}
              placeholder="Serial number"
            />
          </div>
        </div>

        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Location & Assignment</h2>
          <div className={styles.fieldGrid}>
            <Input
              label="Location"
              value={form.location}
              onChange={handleChange('location')}
              placeholder="e.g., Building A, Lab 3"
            />
            <Input
              label="Department"
              value={form.department}
              onChange={handleChange('department')}
              placeholder="Assigned department"
            />
            <Input
              label="Area"
              value={form.area}
              onChange={handleChange('area')}
              placeholder="Specific area"
            />
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/calibration')}>
            Cancel
          </Button>
          <Button type="submit" disabled={mutation.isPending}>
            {mutation.isPending ? 'Registering...' : 'Register Equipment'}
          </Button>
        </div>
      </form>
    </div>
  )
}
