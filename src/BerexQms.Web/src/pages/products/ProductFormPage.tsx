import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './ProductFormPage.module.css'

interface PartDetail {
  id: string
  partNumber: string
  name: string
  description: string | null
  productFamily: string | null
  category: string | null
  serializationMode: string
  status: string
  unitOfMeasure: string | null
}

const serializationOptions = [
  { value: 'None', label: 'None' },
  { value: 'Lot', label: 'Lot' },
  { value: 'Serial', label: 'Serial' },
  { value: 'LotAndSerial', label: 'Lot and Serial' },
]

export function ProductFormPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const isEdit = Boolean(id)
  const queryClient = useQueryClient()

  const [form, setForm] = useState({
    partNumber: '',
    name: '',
    description: '',
    productFamily: '',
    category: '',
    serializationMode: 'None',
    unitOfMeasure: '',
  })
  const [error, setError] = useState('')

  const { data: part } = useQuery<PartDetail>({
    queryKey: ['part', id],
    queryFn: async () => {
      const res = await apiClient.get(`/parts/${id}`)
      return res.data
    },
    enabled: isEdit,
  })

  useEffect(() => {
    if (part) {
      setForm({
        partNumber: part.partNumber,
        name: part.name,
        description: part.description ?? '',
        productFamily: part.productFamily ?? '',
        category: part.category ?? '',
        serializationMode: part.serializationMode,
        unitOfMeasure: part.unitOfMeasure ?? '',
      })
    }
  }, [part])

  const createMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/parts', {
        partNumber: form.partNumber,
        name: form.name,
        description: form.description || null,
        productFamily: form.productFamily || null,
        category: form.category || null,
        serializationMode: form.serializationMode,
        unitOfMeasure: form.unitOfMeasure || null,
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['parts'] })
      navigate(`/products/${data.id}`)
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setError(axiosErr.response?.data?.detail ?? axiosErr.response?.data?.error ?? 'Failed to create part.')
    },
  })

  const updateMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.put(`/parts/${id}`, {
        name: form.name,
        description: form.description || null,
        productFamily: form.productFamily || null,
        category: form.category || null,
        serializationMode: form.serializationMode,
        unitOfMeasure: form.unitOfMeasure || null,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['parts'] })
      queryClient.invalidateQueries({ queryKey: ['part', id] })
      navigate(`/products/${id}`)
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setError(axiosErr.response?.data?.detail ?? axiosErr.response?.data?.error ?? 'Failed to update part.')
    },
  })

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    if (isEdit) {
      updateMutation.mutate()
    } else {
      createMutation.mutate()
    }
  }

  const isPending = createMutation.isPending || updateMutation.isPending

  return (
    <div className={styles.page}>
      <button className={styles.backLink} onClick={() => navigate('/products')}>
        <ArrowLeft size={16} />
        Back to Product Catalog
      </button>

      <h1 className={styles.title}>{isEdit ? 'Edit Part' : 'New Part'}</h1>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form className={styles.form} onSubmit={handleSubmit}>
        <div className={styles.formGrid}>
          <Input
            label="Part Number"
            value={form.partNumber}
            onChange={(e) => setForm((f) => ({ ...f, partNumber: e.target.value }))}
            required
            disabled={isEdit}
            placeholder="e.g. PART-001"
          />
          <Input
            label="Name"
            value={form.name}
            onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
            required
            placeholder="Part name"
          />
          <Input
            label="Product Family"
            value={form.productFamily}
            onChange={(e) => setForm((f) => ({ ...f, productFamily: e.target.value }))}
            placeholder="e.g. Actuators"
          />
          <Input
            label="Category"
            value={form.category}
            onChange={(e) => setForm((f) => ({ ...f, category: e.target.value }))}
            placeholder="e.g. Raw Material"
          />
          <Select
            label="Serialization Mode"
            options={serializationOptions}
            value={form.serializationMode}
            onChange={(e) => setForm((f) => ({ ...f, serializationMode: e.target.value }))}
          />
          <Input
            label="Unit of Measure"
            value={form.unitOfMeasure}
            onChange={(e) => setForm((f) => ({ ...f, unitOfMeasure: e.target.value }))}
            placeholder="e.g. EA, KG, M"
          />
        </div>

        <div className={styles.descriptionField}>
          <label className={styles.label}>Description</label>
          <textarea
            className={styles.textarea}
            value={form.description}
            onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
            rows={3}
            placeholder="Optional part description"
          />
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/products')}>
            Cancel
          </Button>
          <Button type="submit" disabled={isPending}>
            {isPending ? 'Saving...' : isEdit ? 'Save Changes' : 'Create Part'}
          </Button>
        </div>
      </form>
    </div>
  )
}
