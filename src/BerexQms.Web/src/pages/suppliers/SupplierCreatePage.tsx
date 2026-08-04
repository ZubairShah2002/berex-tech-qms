import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import styles from './SupplierCreatePage.module.css'

export function SupplierCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [form, setForm] = useState({
    code: '',
    name: '',
    tier: '',
    contactName: '',
    contactRole: '',
    contactEmail: '',
    contactPhone: '',
  })
  const [error, setError] = useState('')

  const createMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/suppliers', {
        code: form.code,
        name: form.name,
        tier: form.tier || null,
        contactName: form.contactName || null,
        contactRole: form.contactRole || null,
        contactEmail: form.contactEmail || null,
        contactPhone: form.contactPhone || null,
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['suppliers'] })
      navigate(`/suppliers/${data.id}`)
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setError(
        axiosErr.response?.data?.detail ??
          axiosErr.response?.data?.error ??
          'Failed to create supplier.'
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
      <button className={styles.backLink} onClick={() => navigate('/suppliers')}>
        <ArrowLeft size={16} />
        Back to Supplier Quality
      </button>

      <h1 className={styles.title}>Register Supplier</h1>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form className={styles.form} onSubmit={handleSubmit}>
        <div>
          <h2 className={styles.sectionTitle}>Supplier Information</h2>
          <div className={styles.formGrid}>
            <Input
              label="Supplier Code"
              value={form.code}
              onChange={(e) => setForm((f) => ({ ...f, code: e.target.value }))}
              required
              placeholder="e.g. SUP-001"
              maxLength={50}
            />
            <Input
              label="Supplier Name"
              value={form.name}
              onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))}
              required
              placeholder="e.g. Precision Components Ltd."
              maxLength={200}
            />
            <Input
              label="Tier"
              value={form.tier}
              onChange={(e) => setForm((f) => ({ ...f, tier: e.target.value }))}
              placeholder="e.g. Tier 1, Strategic"
            />
          </div>
        </div>

        <div>
          <h2 className={styles.sectionTitle}>Primary Contact</h2>
          <div className={styles.formGrid}>
            <Input
              label="Contact Name"
              value={form.contactName}
              onChange={(e) => setForm((f) => ({ ...f, contactName: e.target.value }))}
              placeholder="Full name"
            />
            <Input
              label="Role"
              value={form.contactRole}
              onChange={(e) => setForm((f) => ({ ...f, contactRole: e.target.value }))}
              placeholder="e.g. Quality Manager"
            />
            <Input
              label="Email"
              type="email"
              value={form.contactEmail}
              onChange={(e) => setForm((f) => ({ ...f, contactEmail: e.target.value }))}
              placeholder="contact@supplier.com"
            />
            <Input
              label="Phone"
              value={form.contactPhone}
              onChange={(e) => setForm((f) => ({ ...f, contactPhone: e.target.value }))}
              placeholder="+1 555-0123"
            />
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/suppliers')}>
            Cancel
          </Button>
          <Button type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Register Supplier'}
          </Button>
        </div>
      </form>
    </div>
  )
}
