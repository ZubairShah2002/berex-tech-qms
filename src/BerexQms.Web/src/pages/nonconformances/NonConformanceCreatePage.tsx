import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './NonConformanceCreatePage.module.css'

const severityOptions = [
  { value: 'Minor', label: 'Minor' },
  { value: 'Major', label: 'Major' },
  { value: 'Critical', label: 'Critical' },
]

const sourceOptions = [
  { value: 'Inspection', label: 'Inspection' },
  { value: 'LineFinding', label: 'Line Finding' },
  { value: 'CustomerComplaint', label: 'Customer Complaint' },
  { value: 'AuditFinding', label: 'Audit Finding' },
  { value: 'SupplierNotification', label: 'Supplier Notification' },
]

const detectionPointOptions = [
  { value: 'IncomingInspection', label: 'Incoming Inspection' },
  { value: 'InProcess', label: 'In-Process' },
  { value: 'FinalInspection', label: 'Final Inspection' },
  { value: 'CustomerSite', label: 'Customer Site' },
  { value: 'FieldReturn', label: 'Field Return' },
]

export function NonConformanceCreatePage() {
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const [form, setForm] = useState({
    ncrNumber: '',
    severity: 'Minor',
    source: 'Inspection',
    detectionPoint: 'IncomingInspection',
    description: '',
    partId: '',
    partRevisionId: '',
    lotNumber: '',
    serialNumber: '',
    supplierId: '',
    supplierLotNumber: '',
    workOrderNumber: '',
    customerId: '',
    sourceInspectionId: '',
    quantityAffected: '',
    quantityDefective: '',
  })
  const [error, setError] = useState('')

  const createMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post('/non-conformances', {
        ncrNumber: form.ncrNumber,
        severity: form.severity,
        source: form.source,
        detectionPoint: form.detectionPoint,
        description: form.description,
        partId: form.partId,
        partRevisionId: form.partRevisionId || null,
        lotNumber: form.lotNumber || null,
        serialNumber: form.serialNumber || null,
        supplierId: form.supplierId || null,
        supplierLotNumber: form.supplierLotNumber || null,
        workOrderNumber: form.workOrderNumber || null,
        customerId: form.customerId || null,
        sourceInspectionId: form.sourceInspectionId || null,
        quantityAffected: form.quantityAffected ? Number(form.quantityAffected) : 0,
        quantityDefective: form.quantityDefective ? Number(form.quantityDefective) : 0,
      })
      return res.data
    },
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['nonconformances'] })
      navigate(`/nonconformances/${data.id}`)
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setError(
        axiosErr.response?.data?.detail ??
          axiosErr.response?.data?.error ??
          'Failed to create non-conformance.'
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
      <button className={styles.backLink} onClick={() => navigate('/nonconformances')}>
        <ArrowLeft size={16} />
        Back to Non-Conformances
      </button>

      <h1 className={styles.title}>New Non-Conformance Record</h1>

      {error && <div className={styles.errorBanner}>{error}</div>}

      <form className={styles.form} onSubmit={handleSubmit}>
        <div>
          <h2 className={styles.sectionTitle}>General Information</h2>
          <div className={styles.formGrid}>
            <Input
              label="NCR Number"
              value={form.ncrNumber}
              onChange={(e) => setForm((f) => ({ ...f, ncrNumber: e.target.value }))}
              required
              placeholder="e.g. NCR-2026-001"
            />
            <Select
              label="Severity"
              options={severityOptions}
              value={form.severity}
              onChange={(e) => setForm((f) => ({ ...f, severity: e.target.value }))}
            />
            <Select
              label="Source"
              options={sourceOptions}
              value={form.source}
              onChange={(e) => setForm((f) => ({ ...f, source: e.target.value }))}
            />
            <Select
              label="Detection Point"
              options={detectionPointOptions}
              value={form.detectionPoint}
              onChange={(e) => setForm((f) => ({ ...f, detectionPoint: e.target.value }))}
            />
            <div className={styles.descriptionField}>
              <label className={styles.fieldLabel}>Description</label>
              <textarea
                className={styles.textarea}
                value={form.description}
                onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
                rows={4}
                placeholder="Describe the non-conformance in detail..."
                required
              />
            </div>
          </div>
        </div>

        <div>
          <h2 className={styles.sectionTitle}>Product Identification</h2>
          <div className={styles.formGrid}>
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
              label="Serial Number"
              value={form.serialNumber}
              onChange={(e) => setForm((f) => ({ ...f, serialNumber: e.target.value }))}
              placeholder="Optional serial number"
            />
            <Input
              label="Quantity Affected"
              type="number"
              value={form.quantityAffected}
              onChange={(e) => setForm((f) => ({ ...f, quantityAffected: e.target.value }))}
              required
              placeholder="Total affected quantity"
            />
            <Input
              label="Quantity Defective"
              type="number"
              value={form.quantityDefective}
              onChange={(e) => setForm((f) => ({ ...f, quantityDefective: e.target.value }))}
              required
              placeholder="Number of defective units"
            />
          </div>
        </div>

        <div>
          <h2 className={styles.sectionTitle}>Traceability</h2>
          <div className={styles.formGrid}>
            <Input
              label="Supplier ID"
              value={form.supplierId}
              onChange={(e) => setForm((f) => ({ ...f, supplierId: e.target.value }))}
              placeholder="Optional supplier UUID"
            />
            <Input
              label="Supplier Lot Number"
              value={form.supplierLotNumber}
              onChange={(e) => setForm((f) => ({ ...f, supplierLotNumber: e.target.value }))}
              placeholder="Supplier lot reference"
            />
            <Input
              label="Work Order Number"
              value={form.workOrderNumber}
              onChange={(e) => setForm((f) => ({ ...f, workOrderNumber: e.target.value }))}
              placeholder="Work order reference"
            />
            <Input
              label="Customer ID"
              value={form.customerId}
              onChange={(e) => setForm((f) => ({ ...f, customerId: e.target.value }))}
              placeholder="Optional customer UUID"
            />
            <Input
              label="Source Inspection ID"
              value={form.sourceInspectionId}
              onChange={(e) => setForm((f) => ({ ...f, sourceInspectionId: e.target.value }))}
              placeholder="Linked inspection UUID"
            />
          </div>
        </div>

        <div className={styles.actions}>
          <Button variant="secondary" type="button" onClick={() => navigate('/nonconformances')}>
            Cancel
          </Button>
          <Button type="submit" disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating...' : 'Create NCR'}
          </Button>
        </div>
      </form>
    </div>
  )
}
