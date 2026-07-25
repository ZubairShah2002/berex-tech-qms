import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Play, CheckCircle, XCircle, Ban, Plus } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { StatusBadge } from '@/components/ui/Badge'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './InspectionDetailPage.module.css'

interface MeasurementDto {
  id: string
  checklistItemId: string | null
  characteristicName: string
  measuredValue: number | null
  textValue: string | null
  unit: string | null
  result: string
  equipmentId: string | null
  operatorId: string | null
  recordedAt: string
  sequenceNumber: number
}

interface ChecklistItemDto {
  id: string
  characteristicName: string
  specificationLimit: string | null
  nominalValue: number | null
  upperLimit: number | null
  lowerLimit: number | null
  unit: string | null
  isCritical: boolean
  sortOrder: number
}

interface ChecklistDto {
  id: string
  partRevisionId: string
  revisionCode: string
  snapshotAt: string
  items: ChecklistItemDto[]
}

interface GateResultDto {
  gateType: string
  passed: boolean
  detail: string | null
  checkedAt: string
}

interface DispositionDto {
  type: string
  justification: string
  approvedBy: string
  approvedAt: string
}

interface InspectionDetail {
  id: string
  inspectionNumber: string
  type: string
  status: string
  partId: string
  partRevisionId: string | null
  lotNumber: string | null
  lotSize: number | null
  sampleSize: number | null
  supplierId: string | null
  samplingPlanId: string | null
  inspectorId: string
  result: string | null
  notes: string | null
  completedAt: string | null
  completedBy: string | null
  approvedAt: string | null
  approvedBy: string | null
  disposition: DispositionDto | null
  checklist: ChecklistDto | null
  gateResults: GateResultDto[]
  measurements: MeasurementDto[]
  createdAt: string
  createdBy: string
  modifiedAt: string | null
}

type Tab = 'measurements' | 'checklist' | 'disposition'

const resultOptions = [
  { value: 'Pass', label: 'Pass' },
  { value: 'Fail', label: 'Fail' },
  { value: 'NotApplicable', label: 'N/A' },
]

const dispositionOptions = [
  { value: 'Accept', label: 'Accept' },
  { value: 'AcceptWithDeviation', label: 'Accept with Deviation' },
  { value: 'Sort', label: 'Sort' },
  { value: 'Rework', label: 'Rework' },
  { value: 'ReturnToSupplier', label: 'Return to Supplier' },
  { value: 'Scrap', label: 'Scrap' },
]

export function InspectionDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('measurements')
  const [actionError, setActionError] = useState('')
  const [showMeasurementForm, setShowMeasurementForm] = useState(false)
  const [measurementForm, setMeasurementForm] = useState({
    characteristicName: '',
    measuredValue: '',
    textValue: '',
    unit: '',
    result: 'Pass',
  })
  const [measurementError, setMeasurementError] = useState('')
  const [showDispositionForm, setShowDispositionForm] = useState(false)
  const [dispositionForm, setDispositionForm] = useState({
    type: 'Accept',
    justification: '',
  })
  const [rejectNotes, setRejectNotes] = useState('')
  const [showRejectForm, setShowRejectForm] = useState(false)

  const { data: inspection, isLoading } = useQuery<InspectionDetail>({
    queryKey: ['inspection', id],
    queryFn: async () => {
      const res = await apiClient.get(`/inspections/${id}`)
      return res.data
    },
    enabled: Boolean(id),
  })

  const handleError = (err: unknown) => {
    const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
    setActionError(
      axiosErr.response?.data?.detail ??
        axiosErr.response?.data?.error ??
        'An error occurred.'
    )
  }

  const invalidate = () => {
    setActionError('')
    queryClient.invalidateQueries({ queryKey: ['inspection', id] })
    queryClient.invalidateQueries({ queryKey: ['inspections'] })
  }

  const startMutation = useMutation({
    mutationFn: () => apiClient.post(`/inspections/${id}/start`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const completeMutation = useMutation({
    mutationFn: () => apiClient.post(`/inspections/${id}/complete`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const approveMutation = useMutation({
    mutationFn: () => apiClient.post(`/inspections/${id}/approve`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const rejectMutation = useMutation({
    mutationFn: (notes: string | null) =>
      apiClient.post(`/inspections/${id}/reject`, { notes }),
    onSuccess: () => {
      invalidate()
      setShowRejectForm(false)
      setRejectNotes('')
    },
    onError: handleError,
  })

  const cancelMutation = useMutation({
    mutationFn: () => apiClient.post(`/inspections/${id}/cancel`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const measurementMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post(`/inspections/${id}/measurements`, {
        characteristicName: measurementForm.characteristicName,
        measuredValue: measurementForm.measuredValue
          ? Number(measurementForm.measuredValue)
          : null,
        textValue: measurementForm.textValue || null,
        unit: measurementForm.unit || null,
        result: measurementForm.result,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['inspection', id] })
      setShowMeasurementForm(false)
      setMeasurementForm({
        characteristicName: '',
        measuredValue: '',
        textValue: '',
        unit: '',
        result: 'Pass',
      })
      setMeasurementError('')
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setMeasurementError(
        axiosErr.response?.data?.detail ??
          axiosErr.response?.data?.error ??
          'Failed to record measurement.'
      )
    },
  })

  const dispositionMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/inspections/${id}/disposition`, {
        type: dispositionForm.type,
        justification: dispositionForm.justification,
      }),
    onSuccess: () => {
      invalidate()
      setShowDispositionForm(false)
      setDispositionForm({ type: 'Accept', justification: '' })
    },
    onError: handleError,
  })

  if (isLoading || !inspection) {
    return <div className={styles.page}>Loading...</div>
  }

  const canStart = inspection.status === 'Draft'
  const canRecordMeasurement = inspection.status === 'InProgress'
  const canComplete = inspection.status === 'InProgress'
  const canApprove = inspection.status === 'PendingApproval'
  const canReject = inspection.status === 'PendingApproval'
  const canSetDisposition =
    inspection.status === 'Approved' && inspection.result === 'Fail'
  const canCancel =
    inspection.status === 'Draft' || inspection.status === 'InProgress'

  return (
    <div className={styles.page}>
      <button className={styles.backLink} onClick={() => navigate('/inspections')}>
        <ArrowLeft size={16} />
        Back to Inspections
      </button>

      {actionError && <div className={styles.errorBanner}>{actionError}</div>}

      <div className={styles.header}>
        <div className={styles.headerInfo}>
          <div className={styles.titleRow}>
            <h1 className={styles.title}>{inspection.inspectionNumber}</h1>
            <StatusBadge status={inspection.status} />
            {inspection.result && <StatusBadge status={inspection.result} />}
          </div>
          <p className={styles.subtitle}>
            {inspection.type} Inspection
            {inspection.lotNumber ? ` — Lot ${inspection.lotNumber}` : ''}
          </p>
        </div>
        <div className={styles.headerActions}>
          {canStart && (
            <Button
              size="sm"
              icon={<Play size={14} />}
              onClick={() => startMutation.mutate()}
              disabled={startMutation.isPending}
            >
              Start
            </Button>
          )}
          {canComplete && (
            <Button
              size="sm"
              icon={<CheckCircle size={14} />}
              onClick={() => {
                if (
                  window.confirm(
                    'Complete this inspection? Results will be auto-calculated.'
                  )
                ) {
                  completeMutation.mutate()
                }
              }}
              disabled={completeMutation.isPending}
            >
              Complete
            </Button>
          )}
          {canApprove && (
            <Button
              size="sm"
              icon={<CheckCircle size={14} />}
              onClick={() => approveMutation.mutate()}
              disabled={approveMutation.isPending}
            >
              Approve
            </Button>
          )}
          {canReject && !showRejectForm && (
            <Button
              size="sm"
              variant="danger"
              icon={<XCircle size={14} />}
              onClick={() => setShowRejectForm(true)}
            >
              Reject
            </Button>
          )}
          {canCancel && (
            <Button
              size="sm"
              variant="secondary"
              icon={<Ban size={14} />}
              onClick={() => {
                if (window.confirm('Cancel this inspection?')) {
                  cancelMutation.mutate()
                }
              }}
              disabled={cancelMutation.isPending}
            >
              Cancel
            </Button>
          )}
        </div>
      </div>

      {showRejectForm && (
        <div className={styles.inlineForm}>
          <Input
            label="Rejection Notes (optional)"
            value={rejectNotes}
            onChange={(e) => setRejectNotes(e.target.value)}
            placeholder="Reason for rejection..."
          />
          <div className={styles.inlineFormActions}>
            <Button
              size="sm"
              variant="danger"
              onClick={() => rejectMutation.mutate(rejectNotes || null)}
              disabled={rejectMutation.isPending}
            >
              {rejectMutation.isPending ? 'Rejecting...' : 'Confirm Reject'}
            </Button>
            <Button
              size="sm"
              variant="ghost"
              onClick={() => setShowRejectForm(false)}
            >
              Cancel
            </Button>
          </div>
        </div>
      )}

      <div className={styles.meta}>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Type</span>
          <span className={styles.metaValue}>{inspection.type}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Lot Number</span>
          <span className={styles.metaValue}>{inspection.lotNumber ?? '—'}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Lot Size</span>
          <span className={styles.metaValue}>
            {inspection.lotSize != null ? String(inspection.lotSize) : '—'}
          </span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Sample Size</span>
          <span className={styles.metaValue}>
            {inspection.sampleSize != null ? String(inspection.sampleSize) : '—'}
          </span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Created</span>
          <span className={styles.metaValue}>
            {new Date(inspection.createdAt).toLocaleDateString()}
          </span>
        </div>
        {inspection.completedAt && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Completed</span>
            <span className={styles.metaValue}>
              {new Date(inspection.completedAt).toLocaleDateString()}
            </span>
          </div>
        )}
        {inspection.approvedAt && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Approved</span>
            <span className={styles.metaValue}>
              {new Date(inspection.approvedAt).toLocaleDateString()}
            </span>
          </div>
        )}
      </div>

      {inspection.notes && (
        <div className={styles.notesSection}>
          <h3 className={styles.sectionTitle}>Notes</h3>
          <p className={styles.notesText}>{inspection.notes}</p>
        </div>
      )}

      <div className={styles.tabs}>
        <button
          className={`${styles.tab} ${tab === 'measurements' ? styles.tabActive : ''}`}
          onClick={() => setTab('measurements')}
        >
          Measurements ({inspection.measurements.length})
        </button>
        <button
          className={`${styles.tab} ${tab === 'checklist' ? styles.tabActive : ''}`}
          onClick={() => setTab('checklist')}
        >
          Checklist{' '}
          {inspection.checklist
            ? `(${inspection.checklist.items.length})`
            : '(0)'}
        </button>
        <button
          className={`${styles.tab} ${tab === 'disposition' ? styles.tabActive : ''}`}
          onClick={() => setTab('disposition')}
        >
          Disposition
        </button>
      </div>

      {tab === 'measurements' && (
        <div className={styles.tabContent}>
          {canRecordMeasurement && (
            <div className={styles.tabHeader}>
              {!showMeasurementForm ? (
                <Button
                  size="sm"
                  icon={<Plus size={14} />}
                  onClick={() => setShowMeasurementForm(true)}
                >
                  Record Measurement
                </Button>
              ) : (
                <div className={styles.measurementForm}>
                  {measurementError && (
                    <div className={styles.formError}>{measurementError}</div>
                  )}
                  <div className={styles.measurementFormGrid}>
                    <Input
                      label="Characteristic"
                      value={measurementForm.characteristicName}
                      onChange={(e) =>
                        setMeasurementForm((f) => ({
                          ...f,
                          characteristicName: e.target.value,
                        }))
                      }
                      required
                      placeholder="e.g. Outer Diameter"
                    />
                    <Input
                      label="Measured Value"
                      type="number"
                      value={measurementForm.measuredValue}
                      onChange={(e) =>
                        setMeasurementForm((f) => ({
                          ...f,
                          measuredValue: e.target.value,
                        }))
                      }
                      placeholder="Numeric value"
                    />
                    <Input
                      label="Text Value"
                      value={measurementForm.textValue}
                      onChange={(e) =>
                        setMeasurementForm((f) => ({
                          ...f,
                          textValue: e.target.value,
                        }))
                      }
                      placeholder="Text result (if applicable)"
                    />
                    <Input
                      label="Unit"
                      value={measurementForm.unit}
                      onChange={(e) =>
                        setMeasurementForm((f) => ({
                          ...f,
                          unit: e.target.value,
                        }))
                      }
                      placeholder="e.g. mm, kg"
                    />
                    <Select
                      label="Result"
                      options={resultOptions}
                      value={measurementForm.result}
                      onChange={(e) =>
                        setMeasurementForm((f) => ({
                          ...f,
                          result: e.target.value,
                        }))
                      }
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button
                      size="sm"
                      onClick={() => measurementMutation.mutate()}
                      disabled={
                        !measurementForm.characteristicName ||
                        measurementMutation.isPending
                      }
                    >
                      {measurementMutation.isPending ? 'Saving...' : 'Save'}
                    </Button>
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => {
                        setShowMeasurementForm(false)
                        setMeasurementError('')
                      }}
                    >
                      Cancel
                    </Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {inspection.measurements.length === 0 ? (
            <p className={styles.emptyText}>
              No measurements recorded yet.
              {canRecordMeasurement &&
                ' Click "Record Measurement" to add one.'}
            </p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Characteristic</th>
                    <th>Measured</th>
                    <th>Text</th>
                    <th>Unit</th>
                    <th>Result</th>
                    <th>Recorded</th>
                  </tr>
                </thead>
                <tbody>
                  {inspection.measurements.map((m) => (
                    <tr key={m.id}>
                      <td>{m.sequenceNumber}</td>
                      <td>{m.characteristicName}</td>
                      <td>
                        {m.measuredValue != null
                          ? String(m.measuredValue)
                          : '—'}
                      </td>
                      <td>{m.textValue ?? '—'}</td>
                      <td>{m.unit ?? '—'}</td>
                      <td>
                        <StatusBadge status={m.result} />
                      </td>
                      <td>
                        {new Date(m.recordedAt).toLocaleString()}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {tab === 'checklist' && (
        <div className={styles.tabContent}>
          {!inspection.checklist ? (
            <p className={styles.emptyText}>No checklist assigned to this inspection.</p>
          ) : (
            <>
              <div className={styles.checklistMeta}>
                <span>
                  Revision: <strong>{inspection.checklist.revisionCode}</strong>
                </span>
                <span>
                  Snapshot:{' '}
                  {new Date(inspection.checklist.snapshotAt).toLocaleDateString()}
                </span>
              </div>
              {inspection.checklist.items.length === 0 ? (
                <p className={styles.emptyText}>
                  Checklist has no items defined.
                </p>
              ) : (
                <div className={styles.tableWrapper}>
                  <table className={styles.dataTable}>
                    <thead>
                      <tr>
                        <th>#</th>
                        <th>Characteristic</th>
                        <th>Spec Limit</th>
                        <th>Nominal</th>
                        <th>Lower</th>
                        <th>Upper</th>
                        <th>Unit</th>
                        <th>Critical</th>
                      </tr>
                    </thead>
                    <tbody>
                      {inspection.checklist.items.map((item) => (
                        <tr key={item.id}>
                          <td>{item.sortOrder}</td>
                          <td>{item.characteristicName}</td>
                          <td>{item.specificationLimit ?? '—'}</td>
                          <td>
                            {item.nominalValue != null
                              ? String(item.nominalValue)
                              : '—'}
                          </td>
                          <td>
                            {item.lowerLimit != null
                              ? String(item.lowerLimit)
                              : '—'}
                          </td>
                          <td>
                            {item.upperLimit != null
                              ? String(item.upperLimit)
                              : '—'}
                          </td>
                          <td>{item.unit ?? '—'}</td>
                          <td>{item.isCritical ? 'Yes' : 'No'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              )}
            </>
          )}
        </div>
      )}

      {tab === 'disposition' && (
        <div className={styles.tabContent}>
          {inspection.disposition ? (
            <div className={styles.dispositionCard}>
              <div className={styles.dispositionGrid}>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Type</span>
                  <span className={styles.metaValue}>
                    {inspection.disposition.type}
                  </span>
                </div>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Justification</span>
                  <span className={styles.metaValue}>
                    {inspection.disposition.justification}
                  </span>
                </div>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Approved By</span>
                  <span className={styles.metaValue}>
                    {inspection.disposition.approvedBy}
                  </span>
                </div>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Approved At</span>
                  <span className={styles.metaValue}>
                    {new Date(
                      inspection.disposition.approvedAt
                    ).toLocaleDateString()}
                  </span>
                </div>
              </div>
            </div>
          ) : canSetDisposition ? (
            !showDispositionForm ? (
              <div>
                <p className={styles.emptyText}>
                  No disposition set. This inspection failed and requires
                  disposition.
                </p>
                <div className={styles.tabHeader}>
                  <Button
                    size="sm"
                    onClick={() => setShowDispositionForm(true)}
                  >
                    Set Disposition
                  </Button>
                </div>
              </div>
            ) : (
              <div className={styles.dispositionForm}>
                <Select
                  label="Disposition Type"
                  options={dispositionOptions}
                  value={dispositionForm.type}
                  onChange={(e) =>
                    setDispositionForm((f) => ({
                      ...f,
                      type: e.target.value,
                    }))
                  }
                />
                <div className={styles.descriptionField}>
                  <label className={styles.fieldLabel}>Justification</label>
                  <textarea
                    className={styles.textarea}
                    value={dispositionForm.justification}
                    onChange={(e) =>
                      setDispositionForm((f) => ({
                        ...f,
                        justification: e.target.value,
                      }))
                    }
                    rows={3}
                    placeholder="Provide justification for the disposition decision..."
                    required
                  />
                </div>
                <div className={styles.inlineFormActions}>
                  <Button
                    size="sm"
                    onClick={() => dispositionMutation.mutate()}
                    disabled={
                      !dispositionForm.justification ||
                      dispositionMutation.isPending
                    }
                  >
                    {dispositionMutation.isPending
                      ? 'Saving...'
                      : 'Save Disposition'}
                  </Button>
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => setShowDispositionForm(false)}
                  >
                    Cancel
                  </Button>
                </div>
              </div>
            )
          ) : (
            <p className={styles.emptyText}>
              {inspection.result === 'Fail'
                ? 'Disposition can be set after the inspection is approved.'
                : 'Disposition is only required for failed inspections.'}
            </p>
          )}
        </div>
      )}
    </div>
  )
}
