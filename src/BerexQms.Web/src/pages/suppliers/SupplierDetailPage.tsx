import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft, Plus, Shield, BarChart3, AlertTriangle, Package,
  CheckCircle, XCircle,
} from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { StatusBadge } from '@/components/ui/Badge'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './SupplierDetailPage.module.css'

interface ApprovalDto {
  id: string
  scopeDescription: string
  approvedDate: string
  expiryDate: string | null
  conditions: string | null
  isActive: boolean
}

interface ScorecardDto {
  id: string
  periodStart: string
  periodEnd: string
  qualityScore: number
  deliveryScore: number
  responsivenessScore: number
  costScore: number
  overallScore: number
  status: string
}

interface ScarDto {
  id: string
  scarNumber: string
  nonConformanceId: string | null
  defectDescription: string
  severity: string
  issuedDate: string
  responseDeadline: string
  status: string
  responseRootCause: string | null
  responseCorrectiveActions: string | null
  responseEvidenceRefs: string | null
  responseDate: string | null
}

interface ApprovedPartDto {
  id: string
  partId: string
  revisionScope: string | null
  approvalDate: string
  isActive: boolean
}

interface SupplierDetail {
  id: string
  code: string
  name: string
  status: string
  riskLevel: string
  tier: string | null
  approvedSince: string | null
  contactName: string | null
  contactRole: string | null
  contactEmail: string | null
  contactPhone: string | null
  riskAssessmentLevel: string | null
  riskAssessmentFactors: string | null
  riskAssessedAt: string | null
  approvals: ApprovalDto[]
  scorecards: ScorecardDto[]
  scars: ScarDto[]
  approvedParts: ApprovedPartDto[]
  createdAt: string
  createdBy: string
  modifiedAt: string | null
}

type Tab = 'approvals' | 'scorecards' | 'scars' | 'parts'

const severityOptions = [
  { value: 'Minor', label: 'Minor' },
  { value: 'Major', label: 'Major' },
  { value: 'Critical', label: 'Critical' },
]

export function SupplierDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('approvals')
  const [actionError, setActionError] = useState('')

  const [showApprovalForm, setShowApprovalForm] = useState(false)
  const [approvalForm, setApprovalForm] = useState({
    scopeDescription: '', approvedDate: '', expiryDate: '', conditions: '',
  })

  const [showScorecardForm, setShowScorecardForm] = useState(false)
  const [scorecardForm, setScorecardForm] = useState({
    periodStart: '', periodEnd: '', qualityScore: '',
    deliveryScore: '', responsivenessScore: '', costScore: '',
  })

  const [showScarForm, setShowScarForm] = useState(false)
  const [scarForm, setScarForm] = useState({
    scarNumber: '', nonConformanceId: '', defectDescription: '',
    severity: 'Major', responseDays: '14',
  })

  const [showRespondForm, setShowRespondForm] = useState<string | null>(null)
  const [respondForm, setRespondForm] = useState({
    rootCause: '', correctiveActions: '', evidenceRefs: '',
  })

  const [showPartForm, setShowPartForm] = useState(false)
  const [partForm, setPartForm] = useState({
    partId: '', revisionScope: '', approvalDate: '',
  })

  const { data: supplier, isLoading } = useQuery<SupplierDetail>({
    queryKey: ['supplier', id],
    queryFn: async () => {
      const res = await apiClient.get(`/suppliers/${id}`)
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
    queryClient.invalidateQueries({ queryKey: ['supplier', id] })
    queryClient.invalidateQueries({ queryKey: ['suppliers'] })
  }

  const approvalMutation = useMutation({
    mutationFn: () => apiClient.post(`/suppliers/${id}/approvals`, {
      scopeDescription: approvalForm.scopeDescription,
      approvedDate: approvalForm.approvedDate,
      expiryDate: approvalForm.expiryDate || null,
      conditions: approvalForm.conditions || null,
    }),
    onSuccess: () => {
      invalidate(); setShowApprovalForm(false)
      setApprovalForm({ scopeDescription: '', approvedDate: '', expiryDate: '', conditions: '' })
    },
    onError: handleError,
  })

  const scorecardMutation = useMutation({
    mutationFn: () => apiClient.post(`/suppliers/${id}/scorecards`, {
      periodStart: scorecardForm.periodStart,
      periodEnd: scorecardForm.periodEnd,
      qualityScore: Number(scorecardForm.qualityScore),
      deliveryScore: Number(scorecardForm.deliveryScore),
      responsivenessScore: Number(scorecardForm.responsivenessScore),
      costScore: Number(scorecardForm.costScore),
    }),
    onSuccess: () => {
      invalidate(); setShowScorecardForm(false)
      setScorecardForm({ periodStart: '', periodEnd: '', qualityScore: '', deliveryScore: '', responsivenessScore: '', costScore: '' })
    },
    onError: handleError,
  })

  const scarMutation = useMutation({
    mutationFn: () => apiClient.post(`/suppliers/${id}/scars`, {
      scarNumber: scarForm.scarNumber,
      nonConformanceId: scarForm.nonConformanceId || null,
      defectDescription: scarForm.defectDescription,
      severity: scarForm.severity,
      responseDays: Number(scarForm.responseDays),
    }),
    onSuccess: () => {
      invalidate(); setShowScarForm(false)
      setScarForm({ scarNumber: '', nonConformanceId: '', defectDescription: '', severity: 'Major', responseDays: '14' })
    },
    onError: handleError,
  })

  const respondMutation = useMutation({
    mutationFn: (scarId: string) => apiClient.put(`/suppliers/${id}/scars/${scarId}/respond`, {
      rootCause: respondForm.rootCause,
      correctiveActions: respondForm.correctiveActions,
      evidenceRefs: respondForm.evidenceRefs || null,
    }),
    onSuccess: () => {
      invalidate(); setShowRespondForm(null)
      setRespondForm({ rootCause: '', correctiveActions: '', evidenceRefs: '' })
    },
    onError: handleError,
  })

  const reviewMutation = useMutation({
    mutationFn: ({ scarId, decision }: { scarId: string; decision: string }) =>
      apiClient.put(`/suppliers/${id}/scars/${scarId}/review`, { decision }),
    onSuccess: invalidate,
    onError: handleError,
  })

  const verifyMutation = useMutation({
    mutationFn: ({ scarId, action }: { scarId: string; action: string }) =>
      apiClient.put(`/suppliers/${id}/scars/${scarId}/verify`, { action }),
    onSuccess: invalidate,
    onError: handleError,
  })

  const partMutation = useMutation({
    mutationFn: () => apiClient.post(`/suppliers/${id}/approved-parts`, {
      partId: partForm.partId,
      revisionScope: partForm.revisionScope || null,
      approvalDate: partForm.approvalDate,
    }),
    onSuccess: () => {
      invalidate(); setShowPartForm(false)
      setPartForm({ partId: '', revisionScope: '', approvalDate: '' })
    },
    onError: handleError,
  })

  if (isLoading || !supplier) {
    return <div className={styles.page}>Loading...</div>
  }

  return (
    <div className={styles.page}>
      <button className={styles.backLink} onClick={() => navigate('/suppliers')}>
        <ArrowLeft size={16} />
        Back to Supplier Quality
      </button>

      {actionError && <div className={styles.errorBanner}>{actionError}</div>}

      <div className={styles.header}>
        <div className={styles.headerInfo}>
          <div className={styles.titleRow}>
            <h1 className={styles.title}>{supplier.code} — {supplier.name}</h1>
            <StatusBadge status={supplier.status} />
            <StatusBadge status={supplier.riskLevel} />
          </div>
          <p className={styles.subtitle}>
            {supplier.tier ?? 'No tier'} — {supplier.approvals.length} approval{supplier.approvals.length !== 1 ? 's' : ''},
            {' '}{supplier.scars.length} SCAR{supplier.scars.length !== 1 ? 's' : ''}
          </p>
        </div>
      </div>

      <div className={styles.meta}>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Status</span>
          <span className={styles.metaValue}>{supplier.status}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Risk Level</span>
          <span className={styles.metaValue}>{supplier.riskLevel}</span>
        </div>
        {supplier.approvedSince && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Approved Since</span>
            <span className={styles.metaValue}>{new Date(supplier.approvedSince).toLocaleDateString()}</span>
          </div>
        )}
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Created By</span>
          <span className={styles.metaValue}>{supplier.createdBy}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Created</span>
          <span className={styles.metaValue}>{new Date(supplier.createdAt).toLocaleDateString()}</span>
        </div>
        {supplier.modifiedAt && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Modified</span>
            <span className={styles.metaValue}>{new Date(supplier.modifiedAt).toLocaleDateString()}</span>
          </div>
        )}
      </div>

      {supplier.contactName && (
        <div className={styles.contactSection}>
          <h3 className={styles.sectionTitle}>Primary Contact</h3>
          <div className={styles.contactGrid}>
            <div><span className={styles.contactLabel}>Name:</span> {supplier.contactName}</div>
            {supplier.contactRole && <div><span className={styles.contactLabel}>Role:</span> {supplier.contactRole}</div>}
            {supplier.contactEmail && <div><span className={styles.contactLabel}>Email:</span> {supplier.contactEmail}</div>}
            {supplier.contactPhone && <div><span className={styles.contactLabel}>Phone:</span> {supplier.contactPhone}</div>}
          </div>
        </div>
      )}

      <div className={styles.tabs}>
        <button className={`${styles.tab} ${tab === 'approvals' ? styles.tabActive : ''}`} onClick={() => setTab('approvals')}>
          <Shield size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Approvals ({supplier.approvals.length})
        </button>
        <button className={`${styles.tab} ${tab === 'scorecards' ? styles.tabActive : ''}`} onClick={() => setTab('scorecards')}>
          <BarChart3 size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Scorecards ({supplier.scorecards.length})
        </button>
        <button className={`${styles.tab} ${tab === 'scars' ? styles.tabActive : ''}`} onClick={() => setTab('scars')}>
          <AlertTriangle size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          SCARs ({supplier.scars.length})
        </button>
        <button className={`${styles.tab} ${tab === 'parts' ? styles.tabActive : ''}`} onClick={() => setTab('parts')}>
          <Package size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Approved Parts ({supplier.approvedParts.length})
        </button>
      </div>

      {tab === 'approvals' && (
        <div className={styles.tabContent}>
          <div className={styles.tabHeader}>
            {!showApprovalForm ? (
              <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowApprovalForm(true)}>
                Add Approval
              </Button>
            ) : (
              <div className={styles.actionForm}>
                <div className={styles.descriptionField}>
                  <label className={styles.fieldLabel}>Scope Description</label>
                  <textarea
                    className={styles.textarea}
                    value={approvalForm.scopeDescription}
                    onChange={(e) => setApprovalForm(f => ({ ...f, scopeDescription: e.target.value }))}
                    rows={3} placeholder="Describe the scope of approval..." required
                  />
                </div>
                <div className={styles.actionFormGrid}>
                  <Input
                    label="Approved Date"
                    type="date"
                    value={approvalForm.approvedDate}
                    onChange={(e) => setApprovalForm(f => ({ ...f, approvedDate: e.target.value }))}
                    required
                  />
                  <Input
                    label="Expiry Date"
                    type="date"
                    value={approvalForm.expiryDate}
                    onChange={(e) => setApprovalForm(f => ({ ...f, expiryDate: e.target.value }))}
                  />
                </div>
                <div className={styles.descriptionField}>
                  <label className={styles.fieldLabel}>Conditions</label>
                  <textarea
                    className={styles.textarea}
                    value={approvalForm.conditions}
                    onChange={(e) => setApprovalForm(f => ({ ...f, conditions: e.target.value }))}
                    rows={2} placeholder="Any conditions on the approval..."
                  />
                </div>
                <div className={styles.inlineFormActions}>
                  <Button size="sm" onClick={() => approvalMutation.mutate()}
                    disabled={!approvalForm.scopeDescription.trim() || !approvalForm.approvedDate || approvalMutation.isPending}>
                    {approvalMutation.isPending ? 'Adding...' : 'Add Approval'}
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => setShowApprovalForm(false)}>Cancel</Button>
                </div>
              </div>
            )}
          </div>

          {supplier.approvals.length === 0 ? (
            <p className={styles.emptyText}>No approvals recorded.</p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Scope</th>
                    <th>Approved</th>
                    <th>Expiry</th>
                    <th>Active</th>
                    <th>Conditions</th>
                  </tr>
                </thead>
                <tbody>
                  {supplier.approvals.map((a) => (
                    <tr key={a.id}>
                      <td>{a.scopeDescription.length > 80 ? `${a.scopeDescription.substring(0, 80)}...` : a.scopeDescription}</td>
                      <td>{new Date(a.approvedDate).toLocaleDateString()}</td>
                      <td>{a.expiryDate ? new Date(a.expiryDate).toLocaleDateString() : '—'}</td>
                      <td>
                        {a.isActive
                          ? <CheckCircle size={14} style={{ color: 'var(--color-success)' }} />
                          : <XCircle size={14} style={{ color: 'var(--color-error)' }} />}
                      </td>
                      <td>{a.conditions ? (a.conditions.length > 40 ? `${a.conditions.substring(0, 40)}...` : a.conditions) : '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {tab === 'scorecards' && (
        <div className={styles.tabContent}>
          <div className={styles.tabHeader}>
            {!showScorecardForm ? (
              <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowScorecardForm(true)}>
                Create Scorecard
              </Button>
            ) : (
              <div className={styles.actionForm}>
                <div className={styles.actionFormGrid}>
                  <Input
                    label="Period Start"
                    type="date"
                    value={scorecardForm.periodStart}
                    onChange={(e) => setScorecardForm(f => ({ ...f, periodStart: e.target.value }))}
                    required
                  />
                  <Input
                    label="Period End"
                    type="date"
                    value={scorecardForm.periodEnd}
                    onChange={(e) => setScorecardForm(f => ({ ...f, periodEnd: e.target.value }))}
                    required
                  />
                  <Input
                    label="Quality Score (40%)"
                    type="number"
                    value={scorecardForm.qualityScore}
                    onChange={(e) => setScorecardForm(f => ({ ...f, qualityScore: e.target.value }))}
                    min={0} max={100} step={0.01} required
                  />
                  <Input
                    label="Delivery Score (25%)"
                    type="number"
                    value={scorecardForm.deliveryScore}
                    onChange={(e) => setScorecardForm(f => ({ ...f, deliveryScore: e.target.value }))}
                    min={0} max={100} step={0.01} required
                  />
                  <Input
                    label="Responsiveness Score (20%)"
                    type="number"
                    value={scorecardForm.responsivenessScore}
                    onChange={(e) => setScorecardForm(f => ({ ...f, responsivenessScore: e.target.value }))}
                    min={0} max={100} step={0.01} required
                  />
                  <Input
                    label="Cost Score (15%)"
                    type="number"
                    value={scorecardForm.costScore}
                    onChange={(e) => setScorecardForm(f => ({ ...f, costScore: e.target.value }))}
                    min={0} max={100} step={0.01} required
                  />
                </div>
                <div className={styles.inlineFormActions}>
                  <Button size="sm" onClick={() => scorecardMutation.mutate()}
                    disabled={!scorecardForm.periodStart || !scorecardForm.periodEnd || !scorecardForm.qualityScore || scorecardMutation.isPending}>
                    {scorecardMutation.isPending ? 'Creating...' : 'Create Scorecard'}
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => setShowScorecardForm(false)}>Cancel</Button>
                </div>
              </div>
            )}
          </div>

          {supplier.scorecards.length === 0 ? (
            <p className={styles.emptyText}>No scorecards recorded.</p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Period</th>
                    <th>Quality</th>
                    <th>Delivery</th>
                    <th>Responsive</th>
                    <th>Cost</th>
                    <th>Overall</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {supplier.scorecards.map((sc) => (
                    <tr key={sc.id}>
                      <td>{new Date(sc.periodStart).toLocaleDateString()} — {new Date(sc.periodEnd).toLocaleDateString()}</td>
                      <td>{sc.qualityScore.toFixed(1)}</td>
                      <td>{sc.deliveryScore.toFixed(1)}</td>
                      <td>{sc.responsivenessScore.toFixed(1)}</td>
                      <td>{sc.costScore.toFixed(1)}</td>
                      <td className={styles.scoreHighlight}>{sc.overallScore.toFixed(1)}</td>
                      <td><StatusBadge status={sc.status} /></td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {tab === 'scars' && (
        <div className={styles.tabContent}>
          <div className={styles.tabHeader}>
            {!showScarForm ? (
              <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowScarForm(true)}>
                Issue SCAR
              </Button>
            ) : (
              <div className={styles.actionForm}>
                <div className={styles.actionFormGrid}>
                  <Input
                    label="SCAR Number"
                    value={scarForm.scarNumber}
                    onChange={(e) => setScarForm(f => ({ ...f, scarNumber: e.target.value }))}
                    placeholder="e.g. SCAR-2026-001" required
                  />
                  <Select
                    label="Severity"
                    options={severityOptions}
                    value={scarForm.severity}
                    onChange={(e) => setScarForm(f => ({ ...f, severity: e.target.value }))}
                  />
                  <Input
                    label="Response Days"
                    type="number"
                    value={scarForm.responseDays}
                    onChange={(e) => setScarForm(f => ({ ...f, responseDays: e.target.value }))}
                    min={1} max={90}
                  />
                  <Input
                    label="NC ID (optional)"
                    value={scarForm.nonConformanceId}
                    onChange={(e) => setScarForm(f => ({ ...f, nonConformanceId: e.target.value }))}
                    placeholder="Link to NCR..."
                  />
                </div>
                <div className={styles.descriptionField}>
                  <label className={styles.fieldLabel}>Defect Description</label>
                  <textarea
                    className={styles.textarea}
                    value={scarForm.defectDescription}
                    onChange={(e) => setScarForm(f => ({ ...f, defectDescription: e.target.value }))}
                    rows={3} placeholder="Describe the defect or quality issue..." required
                  />
                </div>
                <div className={styles.inlineFormActions}>
                  <Button size="sm" onClick={() => scarMutation.mutate()}
                    disabled={!scarForm.scarNumber.trim() || !scarForm.defectDescription.trim() || scarMutation.isPending}>
                    {scarMutation.isPending ? 'Issuing...' : 'Issue SCAR'}
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => setShowScarForm(false)}>Cancel</Button>
                </div>
              </div>
            )}
          </div>

          {supplier.scars.length === 0 ? (
            <p className={styles.emptyText}>No SCAR records.</p>
          ) : (
            <div className={styles.scarList}>
              {supplier.scars.map((scar) => (
                <div key={scar.id} className={styles.scarCard}>
                  <div className={styles.scarCardHeader}>
                    <span className={styles.scarNumber}>{scar.scarNumber}</span>
                    <StatusBadge status={scar.status} />
                    <StatusBadge status={scar.severity} />
                  </div>
                  <p className={styles.scarDescription}>
                    {scar.defectDescription.length > 200
                      ? `${scar.defectDescription.substring(0, 200)}...`
                      : scar.defectDescription}
                  </p>
                  <div className={styles.scarMeta}>
                    <span>Issued: {new Date(scar.issuedDate).toLocaleDateString()}</span>
                    <span>Deadline: {new Date(scar.responseDeadline).toLocaleDateString()}</span>
                  </div>

                  {scar.responseRootCause && (
                    <div className={styles.scarResponse}>
                      <strong>Root Cause:</strong> {scar.responseRootCause}
                      <br />
                      <strong>Corrective Actions:</strong> {scar.responseCorrectiveActions}
                    </div>
                  )}

                  <div className={styles.scarActions}>
                    {(scar.status === 'AwaitingResponse' || scar.status === 'Overdue') && (
                      showRespondForm === scar.id ? (
                        <div className={styles.actionForm}>
                          <div className={styles.descriptionField}>
                            <label className={styles.fieldLabel}>Root Cause</label>
                            <textarea
                              className={styles.textarea}
                              value={respondForm.rootCause}
                              onChange={(e) => setRespondForm(f => ({ ...f, rootCause: e.target.value }))}
                              rows={2} placeholder="Root cause analysis..." required
                            />
                          </div>
                          <div className={styles.descriptionField}>
                            <label className={styles.fieldLabel}>Corrective Actions</label>
                            <textarea
                              className={styles.textarea}
                              value={respondForm.correctiveActions}
                              onChange={(e) => setRespondForm(f => ({ ...f, correctiveActions: e.target.value }))}
                              rows={2} placeholder="Planned corrective actions..." required
                            />
                          </div>
                          <Input
                            label="Evidence References"
                            value={respondForm.evidenceRefs}
                            onChange={(e) => setRespondForm(f => ({ ...f, evidenceRefs: e.target.value }))}
                            placeholder="Optional evidence references..."
                          />
                          <div className={styles.inlineFormActions}>
                            <Button size="sm" onClick={() => respondMutation.mutate(scar.id)}
                              disabled={!respondForm.rootCause.trim() || !respondForm.correctiveActions.trim() || respondMutation.isPending}>
                              {respondMutation.isPending ? 'Submitting...' : 'Submit Response'}
                            </Button>
                            <Button size="sm" variant="ghost" onClick={() => setShowRespondForm(null)}>Cancel</Button>
                          </div>
                        </div>
                      ) : (
                        <Button size="sm" variant="secondary" onClick={() => setShowRespondForm(scar.id)}>
                          Submit Response
                        </Button>
                      )
                    )}
                    {scar.status === 'UnderReview' && (
                      <>
                        <Button size="sm" onClick={() => reviewMutation.mutate({ scarId: scar.id, decision: 'ACCEPT' })}>
                          Accept
                        </Button>
                        <Button size="sm" variant="secondary" onClick={() => reviewMutation.mutate({ scarId: scar.id, decision: 'REJECT' })}>
                          Reject
                        </Button>
                      </>
                    )}
                    {scar.status === 'Accepted' && (
                      <>
                        <Button size="sm" onClick={() => verifyMutation.mutate({ scarId: scar.id, action: 'CLOSE' })}>
                          Close
                        </Button>
                        <Button size="sm" variant="secondary" onClick={() => verifyMutation.mutate({ scarId: scar.id, action: 'FOLLOWUP' })}>
                          Require Follow-Up
                        </Button>
                      </>
                    )}
                    {scar.status === 'FollowUp' && (
                      <Button size="sm" onClick={() => verifyMutation.mutate({ scarId: scar.id, action: 'CLOSE' })}>
                        Close
                      </Button>
                    )}
                    {scar.status === 'Rejected' && (
                      <Button size="sm" variant="secondary" onClick={() => verifyMutation.mutate({ scarId: scar.id, action: 'REISSUE' })}>
                        Reissue
                      </Button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {tab === 'parts' && (
        <div className={styles.tabContent}>
          <div className={styles.tabHeader}>
            {!showPartForm ? (
              <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowPartForm(true)}>
                Add Approved Part
              </Button>
            ) : (
              <div className={styles.actionForm}>
                <div className={styles.actionFormGrid}>
                  <Input
                    label="Part ID"
                    value={partForm.partId}
                    onChange={(e) => setPartForm(f => ({ ...f, partId: e.target.value }))}
                    placeholder="Part UUID" required
                  />
                  <Input
                    label="Approval Date"
                    type="date"
                    value={partForm.approvalDate}
                    onChange={(e) => setPartForm(f => ({ ...f, approvalDate: e.target.value }))}
                    required
                  />
                  <Input
                    label="Revision Scope"
                    value={partForm.revisionScope}
                    onChange={(e) => setPartForm(f => ({ ...f, revisionScope: e.target.value }))}
                    placeholder="e.g. Rev A through Rev C"
                  />
                </div>
                <div className={styles.inlineFormActions}>
                  <Button size="sm" onClick={() => partMutation.mutate()}
                    disabled={!partForm.partId.trim() || !partForm.approvalDate || partMutation.isPending}>
                    {partMutation.isPending ? 'Adding...' : 'Add Part'}
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => setShowPartForm(false)}>Cancel</Button>
                </div>
              </div>
            )}
          </div>

          {supplier.approvedParts.length === 0 ? (
            <p className={styles.emptyText}>No approved parts.</p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Part ID</th>
                    <th>Revision Scope</th>
                    <th>Approval Date</th>
                    <th>Active</th>
                  </tr>
                </thead>
                <tbody>
                  {supplier.approvedParts.map((p) => (
                    <tr key={p.id}>
                      <td>{p.partId}</td>
                      <td>{p.revisionScope ?? '—'}</td>
                      <td>{new Date(p.approvalDate).toLocaleDateString()}</td>
                      <td>
                        {p.isActive
                          ? <CheckCircle size={14} style={{ color: 'var(--color-success)' }} />
                          : <XCircle size={14} style={{ color: 'var(--color-error)' }} />}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
