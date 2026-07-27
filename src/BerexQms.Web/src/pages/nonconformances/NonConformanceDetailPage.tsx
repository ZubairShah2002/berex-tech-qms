import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft, UserPlus, Shield, FileSearch, ClipboardCheck,
  RotateCcw, Copy, Link, Plus, CheckCircle,
} from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { StatusBadge } from '@/components/ui/Badge'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './NonConformanceDetailPage.module.css'

interface ContainmentActionDto {
  id: string
  description: string
  actionTakenBy: string
  actionTakenAt: string
  isVerified: boolean
  verifiedBy: string | null
  verifiedAt: string | null
}

interface InvestigationDto {
  id: string
  investigatorId: string
  methodology: string | null
  rootCause: string | null
  findings: string | null
  startedAt: string
  completedAt: string | null
}

interface ClassificationDto {
  category: string
  defectType: string
  defectCode: string | null
}

interface DispositionDto {
  type: string
  justification: string
  approvedBy: string
  approvedAt: string
}

interface ImpactAssessmentDto {
  affectedQuantity: number
  shippedProductAffected: boolean
  customerImpactDescription: string | null
}

interface NonConformanceDetail {
  id: string
  ncrNumber: string
  status: string
  severity: string
  source: string
  detectionPoint: string
  description: string
  partId: string
  partRevisionId: string | null
  lotNumber: string | null
  serialNumber: string | null
  supplierId: string | null
  supplierLotNumber: string | null
  workOrderNumber: string | null
  customerId: string | null
  sourceInspectionId: string | null
  quantityAffected: number
  quantityDefective: number
  classification: ClassificationDto | null
  disposition: DispositionDto | null
  impactAssessment: ImpactAssessmentDto | null
  assignedTo: string | null
  capaId: string | null
  closedAt: string | null
  closedBy: string | null
  reopenedAt: string | null
  reopenedBy: string | null
  reopenReason: string | null
  closureNotes: string | null
  containmentActions: ContainmentActionDto[]
  investigations: InvestigationDto[]
  createdAt: string
  createdBy: string
  modifiedAt: string | null
}

type Tab = 'containment' | 'investigation' | 'disposition'

const dispositionOptions = [
  { value: 'UseAsIs', label: 'Use As-Is' },
  { value: 'Rework', label: 'Rework' },
  { value: 'Scrap', label: 'Scrap' },
  { value: 'ReturnToSupplier', label: 'Return to Supplier' },
]

export function NonConformanceDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('containment')
  const [actionError, setActionError] = useState('')

  const [showContainmentForm, setShowContainmentForm] = useState(false)
  const [containmentDesc, setContainmentDesc] = useState('')

  const [showAssignForm, setShowAssignForm] = useState(false)
  const [investigatorId, setInvestigatorId] = useState('')

  const [showInvestigationForm, setShowInvestigationForm] = useState(false)
  const [investigationForm, setInvestigationForm] = useState({
    rootCause: '',
    findings: '',
    methodology: '',
  })

  const [showDispositionForm, setShowDispositionForm] = useState(false)
  const [dispositionForm, setDispositionForm] = useState({
    type: 'UseAsIs',
    justification: '',
  })

  const [showMoreInfoForm, setShowMoreInfoForm] = useState(false)
  const [moreInfoReason, setMoreInfoReason] = useState('')

  const [showDuplicateForm, setShowDuplicateForm] = useState(false)
  const [duplicateNotes, setDuplicateNotes] = useState('')

  const [showReopenForm, setShowReopenForm] = useState(false)
  const [reopenReason, setReopenReason] = useState('')

  const [showLinkCapaForm, setShowLinkCapaForm] = useState(false)
  const [capaId, setCapaId] = useState('')

  const { data: nc, isLoading } = useQuery<NonConformanceDetail>({
    queryKey: ['nonconformance', id],
    queryFn: async () => {
      const res = await apiClient.get(`/non-conformances/${id}`)
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
    queryClient.invalidateQueries({ queryKey: ['nonconformance', id] })
    queryClient.invalidateQueries({ queryKey: ['nonconformances'] })
  }

  const assignMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/non-conformances/${id}/assign-investigator`, { investigatorId }),
    onSuccess: () => {
      invalidate()
      setShowAssignForm(false)
      setInvestigatorId('')
    },
    onError: handleError,
  })

  const containmentMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/non-conformances/${id}/containment-actions`, { description: containmentDesc }),
    onSuccess: () => {
      invalidate()
      setShowContainmentForm(false)
      setContainmentDesc('')
    },
    onError: handleError,
  })

  const verifyMutation = useMutation({
    mutationFn: (actionId: string) =>
      apiClient.post(`/non-conformances/${id}/containment-actions/${actionId}/verify`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const investigationMutation = useMutation({
    mutationFn: () =>
      apiClient.put(`/non-conformances/${id}/investigation`, {
        rootCause: investigationForm.rootCause,
        findings: investigationForm.findings,
        methodology: investigationForm.methodology || null,
      }),
    onSuccess: () => {
      invalidate()
      setShowInvestigationForm(false)
      setInvestigationForm({ rootCause: '', findings: '', methodology: '' })
    },
    onError: handleError,
  })

  const dispositionMutation = useMutation({
    mutationFn: () =>
      apiClient.put(`/non-conformances/${id}/disposition`, {
        type: dispositionForm.type,
        justification: dispositionForm.justification,
      }),
    onSuccess: () => {
      invalidate()
      setShowDispositionForm(false)
      setDispositionForm({ type: 'UseAsIs', justification: '' })
    },
    onError: handleError,
  })

  const moreInfoMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/non-conformances/${id}/request-more-info`, { reason: moreInfoReason }),
    onSuccess: () => {
      invalidate()
      setShowMoreInfoForm(false)
      setMoreInfoReason('')
    },
    onError: handleError,
  })

  const duplicateMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/non-conformances/${id}/close-as-duplicate`, { notes: duplicateNotes }),
    onSuccess: () => {
      invalidate()
      setShowDuplicateForm(false)
      setDuplicateNotes('')
    },
    onError: handleError,
  })

  const reopenMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/non-conformances/${id}/reopen`, { reason: reopenReason }),
    onSuccess: () => {
      invalidate()
      setShowReopenForm(false)
      setReopenReason('')
    },
    onError: handleError,
  })

  const linkCapaMutation = useMutation({
    mutationFn: () =>
      apiClient.post(`/non-conformances/${id}/link-capa`, { capaId }),
    onSuccess: () => {
      invalidate()
      setShowLinkCapaForm(false)
      setCapaId('')
    },
    onError: handleError,
  })

  if (isLoading || !nc) {
    return <div className={styles.page}>Loading...</div>
  }

  const canAssign = nc.status === 'Open' || nc.status === 'Reopened'
  const canAddContainment = nc.status !== 'Closed'
  const canSubmitInvestigation = nc.status === 'UnderInvestigation'
  const canDisposition = nc.status === 'PendingDisposition'
  const canCloseAsDuplicate = nc.status === 'Open'
  const canReopen = nc.status === 'Closed'
  const canLinkCapa = !nc.capaId

  return (
    <div className={styles.page}>
      <button className={styles.backLink} onClick={() => navigate('/nonconformances')}>
        <ArrowLeft size={16} />
        Back to Non-Conformances
      </button>

      {actionError && <div className={styles.errorBanner}>{actionError}</div>}

      <div className={styles.header}>
        <div className={styles.headerInfo}>
          <div className={styles.titleRow}>
            <h1 className={styles.title}>{nc.ncrNumber}</h1>
            <StatusBadge status={nc.status} />
            <StatusBadge status={nc.severity} />
          </div>
          <p className={styles.subtitle}>
            {nc.source} — {nc.detectionPoint}
            {nc.assignedTo ? ` — Assigned to ${nc.assignedTo}` : ''}
          </p>
        </div>
        <div className={styles.headerActions}>
          {canAssign && !showAssignForm && (
            <Button size="sm" icon={<UserPlus size={14} />} onClick={() => setShowAssignForm(true)}>
              Assign Investigator
            </Button>
          )}
          {canCloseAsDuplicate && !showDuplicateForm && (
            <Button size="sm" variant="secondary" icon={<Copy size={14} />} onClick={() => setShowDuplicateForm(true)}>
              Close as Duplicate
            </Button>
          )}
          {canReopen && !showReopenForm && (
            <Button size="sm" variant="secondary" icon={<RotateCcw size={14} />} onClick={() => setShowReopenForm(true)}>
              Reopen
            </Button>
          )}
          {canLinkCapa && !showLinkCapaForm && (
            <Button size="sm" variant="secondary" icon={<Link size={14} />} onClick={() => setShowLinkCapaForm(true)}>
              Link CAPA
            </Button>
          )}
        </div>
      </div>

      {showAssignForm && (
        <div className={styles.inlineForm}>
          <Input
            label="Investigator ID"
            value={investigatorId}
            onChange={(e) => setInvestigatorId(e.target.value)}
            placeholder="Enter investigator user ID"
            required
          />
          <div className={styles.inlineFormActions}>
            <Button size="sm" onClick={() => assignMutation.mutate()} disabled={!investigatorId.trim() || assignMutation.isPending}>
              {assignMutation.isPending ? 'Assigning...' : 'Assign'}
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setShowAssignForm(false)}>Cancel</Button>
          </div>
        </div>
      )}

      {showDuplicateForm && (
        <div className={styles.inlineForm}>
          <div className={styles.descriptionField}>
            <label className={styles.fieldLabel}>Closure Notes</label>
            <textarea
              className={styles.textarea}
              value={duplicateNotes}
              onChange={(e) => setDuplicateNotes(e.target.value)}
              rows={3}
              placeholder="Explain why this is a duplicate or invalid..."
              required
            />
          </div>
          <div className={styles.inlineFormActions}>
            <Button size="sm" variant="danger" onClick={() => duplicateMutation.mutate()} disabled={!duplicateNotes.trim() || duplicateMutation.isPending}>
              {duplicateMutation.isPending ? 'Closing...' : 'Close as Duplicate'}
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setShowDuplicateForm(false)}>Cancel</Button>
          </div>
        </div>
      )}

      {showReopenForm && (
        <div className={styles.inlineForm}>
          <div className={styles.descriptionField}>
            <label className={styles.fieldLabel}>Reopen Reason</label>
            <textarea
              className={styles.textarea}
              value={reopenReason}
              onChange={(e) => setReopenReason(e.target.value)}
              rows={3}
              placeholder="Reason for reopening this NCR..."
              required
            />
          </div>
          <div className={styles.inlineFormActions}>
            <Button size="sm" onClick={() => reopenMutation.mutate()} disabled={!reopenReason.trim() || reopenMutation.isPending}>
              {reopenMutation.isPending ? 'Reopening...' : 'Confirm Reopen'}
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setShowReopenForm(false)}>Cancel</Button>
          </div>
        </div>
      )}

      {showLinkCapaForm && (
        <div className={styles.inlineForm}>
          <Input
            label="CAPA ID"
            value={capaId}
            onChange={(e) => setCapaId(e.target.value)}
            placeholder="CAPA UUID"
            required
          />
          <div className={styles.inlineFormActions}>
            <Button size="sm" onClick={() => linkCapaMutation.mutate()} disabled={!capaId.trim() || linkCapaMutation.isPending}>
              {linkCapaMutation.isPending ? 'Linking...' : 'Link CAPA'}
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setShowLinkCapaForm(false)}>Cancel</Button>
          </div>
        </div>
      )}

      <div className={styles.meta}>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Source</span>
          <span className={styles.metaValue}>{nc.source}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Detection Point</span>
          <span className={styles.metaValue}>{nc.detectionPoint}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Qty Affected</span>
          <span className={styles.metaValue}>{nc.quantityAffected}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Qty Defective</span>
          <span className={styles.metaValue}>{nc.quantityDefective}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Lot #</span>
          <span className={styles.metaValue}>{nc.lotNumber ?? '—'}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Created</span>
          <span className={styles.metaValue}>{new Date(nc.createdAt).toLocaleDateString()}</span>
        </div>
        {nc.closedAt && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Closed</span>
            <span className={styles.metaValue}>{new Date(nc.closedAt).toLocaleDateString()}</span>
          </div>
        )}
        {nc.capaId && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>CAPA</span>
            <span className={styles.metaValue}>{nc.capaId.substring(0, 8)}...</span>
          </div>
        )}
      </div>

      {nc.classification && (
        <div className={styles.descriptionSection}>
          <h3 className={styles.sectionTitle}>Classification</h3>
          <div className={styles.meta} style={{ background: 'transparent', border: 'none', padding: 0 }}>
            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Category</span>
              <span className={styles.metaValue}>{nc.classification.category}</span>
            </div>
            <div className={styles.metaItem}>
              <span className={styles.metaLabel}>Defect Type</span>
              <span className={styles.metaValue}>{nc.classification.defectType}</span>
            </div>
            {nc.classification.defectCode && (
              <div className={styles.metaItem}>
                <span className={styles.metaLabel}>Defect Code</span>
                <span className={styles.metaValue}>{nc.classification.defectCode}</span>
              </div>
            )}
          </div>
        </div>
      )}

      <div className={styles.descriptionSection}>
        <h3 className={styles.sectionTitle}>Description</h3>
        <p className={styles.descriptionText}>{nc.description}</p>
      </div>

      {nc.closureNotes && (
        <div className={styles.descriptionSection}>
          <h3 className={styles.sectionTitle}>Closure Notes</h3>
          <p className={styles.descriptionText}>{nc.closureNotes}</p>
        </div>
      )}

      {nc.reopenReason && (
        <div className={styles.descriptionSection}>
          <h3 className={styles.sectionTitle}>Reopen Reason</h3>
          <p className={styles.descriptionText}>{nc.reopenReason}</p>
        </div>
      )}

      <div className={styles.tabs}>
        <button
          className={`${styles.tab} ${tab === 'containment' ? styles.tabActive : ''}`}
          onClick={() => setTab('containment')}
        >
          <Shield size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Containment ({nc.containmentActions.length})
        </button>
        <button
          className={`${styles.tab} ${tab === 'investigation' ? styles.tabActive : ''}`}
          onClick={() => setTab('investigation')}
        >
          <FileSearch size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Investigation ({nc.investigations.length})
        </button>
        <button
          className={`${styles.tab} ${tab === 'disposition' ? styles.tabActive : ''}`}
          onClick={() => setTab('disposition')}
        >
          <ClipboardCheck size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Disposition
        </button>
      </div>

      {tab === 'containment' && (
        <div className={styles.tabContent}>
          {canAddContainment && (
            <div className={styles.tabHeader}>
              {!showContainmentForm ? (
                <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowContainmentForm(true)}>
                  Add Containment Action
                </Button>
              ) : (
                <div className={styles.actionForm}>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Description</label>
                    <textarea
                      className={styles.textarea}
                      value={containmentDesc}
                      onChange={(e) => setContainmentDesc(e.target.value)}
                      rows={3}
                      placeholder="Describe the containment action taken..."
                      required
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => containmentMutation.mutate()} disabled={!containmentDesc.trim() || containmentMutation.isPending}>
                      {containmentMutation.isPending ? 'Saving...' : 'Save'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => { setShowContainmentForm(false); setContainmentDesc('') }}>
                      Cancel
                    </Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {nc.containmentActions.length === 0 ? (
            <p className={styles.emptyText}>
              No containment actions recorded yet.
              {canAddContainment && ' Add one to contain the non-conformity.'}
            </p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Description</th>
                    <th>Taken By</th>
                    <th>Date</th>
                    <th>Verified</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {nc.containmentActions.map((a) => (
                    <tr key={a.id}>
                      <td>{a.description}</td>
                      <td>{a.actionTakenBy}</td>
                      <td>{new Date(a.actionTakenAt).toLocaleDateString()}</td>
                      <td>
                        {a.isVerified ? (
                          <span>
                            <CheckCircle size={14} style={{ color: 'var(--color-success)', verticalAlign: 'middle', marginRight: 4 }} />
                            {a.verifiedBy} ({a.verifiedAt ? new Date(a.verifiedAt).toLocaleDateString() : ''})
                          </span>
                        ) : (
                          'No'
                        )}
                      </td>
                      <td>
                        {!a.isVerified && nc.status !== 'Closed' && (
                          <button
                            className={styles.verifyBtn}
                            onClick={() => verifyMutation.mutate(a.id)}
                            disabled={verifyMutation.isPending}
                          >
                            Verify
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {tab === 'investigation' && (
        <div className={styles.tabContent}>
          {canSubmitInvestigation && (
            <div className={styles.tabHeader}>
              {!showInvestigationForm ? (
                <Button size="sm" icon={<FileSearch size={14} />} onClick={() => setShowInvestigationForm(true)}>
                  Submit Investigation
                </Button>
              ) : (
                <div className={styles.actionForm}>
                  <div className={styles.actionFormGrid}>
                    <Input
                      label="Methodology"
                      value={investigationForm.methodology}
                      onChange={(e) => setInvestigationForm((f) => ({ ...f, methodology: e.target.value }))}
                      placeholder="e.g. 5-Why, Fishbone"
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Root Cause</label>
                    <textarea
                      className={styles.textarea}
                      value={investigationForm.rootCause}
                      onChange={(e) => setInvestigationForm((f) => ({ ...f, rootCause: e.target.value }))}
                      rows={3}
                      placeholder="Describe the root cause..."
                      required
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Findings</label>
                    <textarea
                      className={styles.textarea}
                      value={investigationForm.findings}
                      onChange={(e) => setInvestigationForm((f) => ({ ...f, findings: e.target.value }))}
                      rows={3}
                      placeholder="Summarize investigation findings..."
                      required
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button
                      size="sm"
                      onClick={() => investigationMutation.mutate()}
                      disabled={!investigationForm.rootCause.trim() || !investigationForm.findings.trim() || investigationMutation.isPending}
                    >
                      {investigationMutation.isPending ? 'Submitting...' : 'Submit'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowInvestigationForm(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {nc.investigations.length === 0 ? (
            <p className={styles.emptyText}>No investigations recorded yet.</p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Investigator</th>
                    <th>Methodology</th>
                    <th>Root Cause</th>
                    <th>Findings</th>
                    <th>Started</th>
                    <th>Completed</th>
                  </tr>
                </thead>
                <tbody>
                  {nc.investigations.map((inv) => (
                    <tr key={inv.id}>
                      <td>{inv.investigatorId}</td>
                      <td>{inv.methodology ?? '—'}</td>
                      <td>{inv.rootCause ?? '—'}</td>
                      <td>{inv.findings ?? '—'}</td>
                      <td>{new Date(inv.startedAt).toLocaleDateString()}</td>
                      <td>{inv.completedAt ? new Date(inv.completedAt).toLocaleDateString() : 'Active'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {tab === 'disposition' && (
        <div className={styles.tabContent}>
          {nc.disposition ? (
            <div className={styles.dispositionCard}>
              <div className={styles.dispositionGrid}>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Type</span>
                  <span className={styles.metaValue}>{nc.disposition.type}</span>
                </div>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Approved By</span>
                  <span className={styles.metaValue}>{nc.disposition.approvedBy}</span>
                </div>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Approved At</span>
                  <span className={styles.metaValue}>{new Date(nc.disposition.approvedAt).toLocaleDateString()}</span>
                </div>
                <div className={`${styles.metaItem} ${styles.fullWidth}`}>
                  <span className={styles.metaLabel}>Justification</span>
                  <span className={styles.metaValue}>{nc.disposition.justification}</span>
                </div>
              </div>
            </div>
          ) : canDisposition ? (
            <>
              {!showDispositionForm && !showMoreInfoForm ? (
                <div>
                  <p className={styles.emptyText}>Investigation complete. Record disposition or request more information.</p>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" icon={<ClipboardCheck size={14} />} onClick={() => setShowDispositionForm(true)}>
                      Record Disposition
                    </Button>
                    <Button size="sm" variant="secondary" onClick={() => setShowMoreInfoForm(true)}>
                      Request More Info
                    </Button>
                  </div>
                </div>
              ) : showDispositionForm ? (
                <div className={styles.dispositionForm}>
                  <Select
                    label="Disposition Type"
                    options={dispositionOptions}
                    value={dispositionForm.type}
                    onChange={(e) => setDispositionForm((f) => ({ ...f, type: e.target.value }))}
                  />
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Justification</label>
                    <textarea
                      className={styles.textarea}
                      value={dispositionForm.justification}
                      onChange={(e) => setDispositionForm((f) => ({ ...f, justification: e.target.value }))}
                      rows={3}
                      placeholder="Provide justification for the disposition decision..."
                      required
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button
                      size="sm"
                      onClick={() => dispositionMutation.mutate()}
                      disabled={!dispositionForm.justification.trim() || dispositionMutation.isPending}
                    >
                      {dispositionMutation.isPending ? 'Saving...' : 'Save Disposition'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowDispositionForm(false)}>Cancel</Button>
                  </div>
                </div>
              ) : (
                <div className={styles.inlineForm}>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Reason for Requesting More Information</label>
                    <textarea
                      className={styles.textarea}
                      value={moreInfoReason}
                      onChange={(e) => setMoreInfoReason(e.target.value)}
                      rows={3}
                      placeholder="What additional information is needed..."
                      required
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button
                      size="sm"
                      onClick={() => moreInfoMutation.mutate()}
                      disabled={!moreInfoReason.trim() || moreInfoMutation.isPending}
                    >
                      {moreInfoMutation.isPending ? 'Sending...' : 'Request More Info'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowMoreInfoForm(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </>
          ) : (
            <p className={styles.emptyText}>
              {nc.status === 'UnderInvestigation'
                ? 'Disposition will be available after investigation is submitted.'
                : nc.status === 'Open' || nc.status === 'Reopened'
                  ? 'Assign an investigator to begin the investigation process.'
                  : 'No disposition recorded.'}
            </p>
          )}
        </div>
      )}
    </div>
  )
}
