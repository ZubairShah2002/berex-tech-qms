import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft, UserPlus, Search, ListChecks, ShieldCheck,
  Plus, CheckCircle, AlertTriangle,
} from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { StatusBadge } from '@/components/ui/Badge'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './CapaDetailPage.module.css'

interface RCADto {
  id: string
  methodology: string
  analysisDetails: string | null
  rootCause: string | null
  contributingFactors: string | null
  analystId: string
  startedAt: string
  completedAt: string | null
}

interface ActionDto {
  id: string
  actionType: string
  description: string
  ownerId: string
  dueDate: string
  evidenceRequirement: string | null
  completionNotes: string | null
  evidenceProvided: string | null
  completedAt: string | null
  completedBy: string | null
  isOverdue: boolean
  createdAt: string
}

interface VerificationDto {
  id: string
  scheduledDate: string
  verificationCriteria: string
  verifierId: string | null
  result: string | null
  evidence: string | null
  isEffective: boolean | null
  verifiedAt: string | null
  createdAt: string
}

interface SourceDto {
  sourceType: string
  sourceNonConformanceId: string | null
  sourceAuditFindingId: string | null
  sourceDescription: string | null
}

interface CAPADetail {
  id: string
  capaNumber: string
  title: string
  description: string
  status: string
  priority: string
  source: SourceDto
  ownerId: string
  assignedTo: string | null
  sourceNonConformanceId: string | null
  targetClosureDate: string | null
  closedAt: string | null
  closedBy: string | null
  closureNotes: string | null
  rootCauseAnalysis: RCADto | null
  actions: ActionDto[]
  verifications: VerificationDto[]
  createdAt: string
  createdBy: string
  modifiedAt: string | null
}

type Tab = 'rca' | 'actions' | 'verification'

const methodologyOptions = [
  { value: 'FiveWhy', label: '5-Why' },
  { value: 'Fishbone', label: 'Fishbone (Ishikawa)' },
  { value: 'FaultTreeAnalysis', label: 'Fault Tree Analysis' },
  { value: 'Other', label: 'Other' },
]

const actionTypeOptions = [
  { value: 'Corrective', label: 'Corrective' },
  { value: 'Preventive', label: 'Preventive' },
]

export function CapaDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('rca')
  const [actionError, setActionError] = useState('')

  const [showAssignForm, setShowAssignForm] = useState(false)
  const [assigneeId, setAssigneeId] = useState('')

  const [showStartRcaForm, setShowStartRcaForm] = useState(false)
  const [rcaStartForm, setRcaStartForm] = useState({ methodology: 'FiveWhy', analystId: '' })

  const [showSubmitRcaForm, setShowSubmitRcaForm] = useState(false)
  const [rcaSubmitForm, setRcaSubmitForm] = useState({
    rootCause: '', analysisDetails: '', contributingFactors: '',
  })

  const [showAddActionForm, setShowAddActionForm] = useState(false)
  const [actionForm, setActionForm] = useState({
    actionType: 'Corrective', description: '', ownerId: '',
    dueDate: '', evidenceRequirement: '',
  })

  const [completingActionId, setCompletingActionId] = useState<string | null>(null)
  const [completeForm, setCompleteForm] = useState({ completionNotes: '', evidenceProvided: '' })

  const [showScheduleVerification, setShowScheduleVerification] = useState(false)
  const [verificationForm, setVerificationForm] = useState({
    scheduledDate: '', verificationCriteria: '',
  })

  const [recordingVerificationId, setRecordingVerificationId] = useState<string | null>(null)
  const [recordForm, setRecordForm] = useState({
    isEffective: 'true', result: '', evidence: '',
  })

  const { data: capa, isLoading } = useQuery<CAPADetail>({
    queryKey: ['capa', id],
    queryFn: async () => {
      const res = await apiClient.get(`/capas/${id}`)
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
    queryClient.invalidateQueries({ queryKey: ['capa', id] })
    queryClient.invalidateQueries({ queryKey: ['capas'] })
  }

  const assignMutation = useMutation({
    mutationFn: () => apiClient.post(`/capas/${id}/assign`, { assigneeId }),
    onSuccess: () => { invalidate(); setShowAssignForm(false); setAssigneeId('') },
    onError: handleError,
  })

  const startRcaMutation = useMutation({
    mutationFn: () => apiClient.post(`/capas/${id}/rca`, rcaStartForm),
    onSuccess: () => {
      invalidate(); setShowStartRcaForm(false)
      setRcaStartForm({ methodology: 'FiveWhy', analystId: '' })
    },
    onError: handleError,
  })

  const submitRcaMutation = useMutation({
    mutationFn: () => apiClient.put(`/capas/${id}/rca`, {
      rootCause: rcaSubmitForm.rootCause,
      analysisDetails: rcaSubmitForm.analysisDetails || null,
      contributingFactors: rcaSubmitForm.contributingFactors || null,
    }),
    onSuccess: () => {
      invalidate(); setShowSubmitRcaForm(false)
      setRcaSubmitForm({ rootCause: '', analysisDetails: '', contributingFactors: '' })
    },
    onError: handleError,
  })

  const addActionMutation = useMutation({
    mutationFn: () => apiClient.post(`/capas/${id}/actions`, {
      actionType: actionForm.actionType,
      description: actionForm.description,
      ownerId: actionForm.ownerId,
      dueDate: actionForm.dueDate,
      evidenceRequirement: actionForm.evidenceRequirement || null,
    }),
    onSuccess: () => {
      invalidate(); setShowAddActionForm(false)
      setActionForm({ actionType: 'Corrective', description: '', ownerId: '', dueDate: '', evidenceRequirement: '' })
    },
    onError: handleError,
  })

  const completeActionMutation = useMutation({
    mutationFn: (actionId: string) => apiClient.put(`/capas/${id}/actions/${actionId}/complete`, {
      completionNotes: completeForm.completionNotes || null,
      evidenceProvided: completeForm.evidenceProvided || null,
    }),
    onSuccess: () => {
      invalidate(); setCompletingActionId(null)
      setCompleteForm({ completionNotes: '', evidenceProvided: '' })
    },
    onError: handleError,
  })

  const scheduleVerificationMutation = useMutation({
    mutationFn: () => apiClient.post(`/capas/${id}/verifications`, {
      scheduledDate: verificationForm.scheduledDate,
      verificationCriteria: verificationForm.verificationCriteria,
    }),
    onSuccess: () => {
      invalidate(); setShowScheduleVerification(false)
      setVerificationForm({ scheduledDate: '', verificationCriteria: '' })
    },
    onError: handleError,
  })

  const recordVerificationMutation = useMutation({
    mutationFn: (verificationId: string) => apiClient.put(`/capas/${id}/verify`, {
      verificationId,
      isEffective: recordForm.isEffective === 'true',
      result: recordForm.result,
      evidence: recordForm.evidence || null,
    }),
    onSuccess: () => {
      invalidate(); setRecordingVerificationId(null)
      setRecordForm({ isEffective: 'true', result: '', evidence: '' })
    },
    onError: handleError,
  })

  if (isLoading || !capa) {
    return <div className={styles.page}>Loading...</div>
  }

  const canAssign = capa.status !== 'ClosedEffective' && capa.status !== 'ClosedIneffective'
  const canStartRca = capa.status === 'Initiated' || capa.status === 'RCAInProgress'
  const canSubmitRca = capa.status === 'RCAInProgress' && capa.rootCauseAnalysis && !capa.rootCauseAnalysis.completedAt
  const canAddAction = capa.status === 'ActionPlanning' || capa.status === 'Implementation'
  const canCompleteAction = capa.status === 'Implementation'
  const canScheduleVerification = capa.status === 'Implementation' || capa.status === 'PendingVerification'
  const canRecordVerification = capa.status === 'PendingVerification'
  const isClosed = capa.status === 'ClosedEffective' || capa.status === 'ClosedIneffective'

  return (
    <div className={styles.page}>
      <button className={styles.backLink} onClick={() => navigate('/capa')}>
        <ArrowLeft size={16} />
        Back to CAPA Management
      </button>

      {actionError && <div className={styles.errorBanner}>{actionError}</div>}

      <div className={styles.header}>
        <div className={styles.headerInfo}>
          <div className={styles.titleRow}>
            <h1 className={styles.title}>{capa.capaNumber}</h1>
            <StatusBadge status={capa.status} />
            <StatusBadge status={capa.priority} />
          </div>
          <p className={styles.subtitle}>
            {capa.title}
            {capa.assignedTo ? ` — Assigned to ${capa.assignedTo}` : ''}
          </p>
        </div>
        <div className={styles.headerActions}>
          {canAssign && !showAssignForm && (
            <Button size="sm" icon={<UserPlus size={14} />} onClick={() => setShowAssignForm(true)}>
              Assign
            </Button>
          )}
        </div>
      </div>

      {showAssignForm && (
        <div className={styles.inlineForm}>
          <Input
            label="Assignee ID"
            value={assigneeId}
            onChange={(e) => setAssigneeId(e.target.value)}
            placeholder="Enter user ID"
            required
          />
          <div className={styles.inlineFormActions}>
            <Button size="sm" onClick={() => assignMutation.mutate()} disabled={!assigneeId.trim() || assignMutation.isPending}>
              {assignMutation.isPending ? 'Assigning...' : 'Assign'}
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setShowAssignForm(false)}>Cancel</Button>
          </div>
        </div>
      )}

      <div className={styles.meta}>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Source Type</span>
          <span className={styles.metaValue}>{capa.source.sourceType}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Owner</span>
          <span className={styles.metaValue}>{capa.ownerId}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Created</span>
          <span className={styles.metaValue}>{new Date(capa.createdAt).toLocaleDateString()}</span>
        </div>
        {capa.targetClosureDate && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Target Closure</span>
            <span className={styles.metaValue}>{new Date(capa.targetClosureDate).toLocaleDateString()}</span>
          </div>
        )}
        {capa.closedAt && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Closed</span>
            <span className={styles.metaValue}>{new Date(capa.closedAt).toLocaleDateString()}</span>
          </div>
        )}
        {capa.sourceNonConformanceId && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Source NC</span>
            <span className={styles.metaValue}>{capa.sourceNonConformanceId.substring(0, 8)}...</span>
          </div>
        )}
      </div>

      <div className={styles.descriptionSection}>
        <h3 className={styles.sectionTitle}>Description</h3>
        <p className={styles.descriptionText}>{capa.description}</p>
      </div>

      {capa.source.sourceDescription && (
        <div className={styles.descriptionSection}>
          <h3 className={styles.sectionTitle}>Source Description</h3>
          <p className={styles.descriptionText}>{capa.source.sourceDescription}</p>
        </div>
      )}

      <div className={styles.tabs}>
        <button className={`${styles.tab} ${tab === 'rca' ? styles.tabActive : ''}`} onClick={() => setTab('rca')}>
          <Search size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Root Cause Analysis
        </button>
        <button className={`${styles.tab} ${tab === 'actions' ? styles.tabActive : ''}`} onClick={() => setTab('actions')}>
          <ListChecks size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Actions ({capa.actions.length})
        </button>
        <button className={`${styles.tab} ${tab === 'verification' ? styles.tabActive : ''}`} onClick={() => setTab('verification')}>
          <ShieldCheck size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Verification ({capa.verifications.length})
        </button>
      </div>

      {tab === 'rca' && (
        <div className={styles.tabContent}>
          {capa.rootCauseAnalysis ? (
            <div className={styles.rcaCard}>
              <div className={styles.rcaGrid}>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Methodology</span>
                  <span className={styles.metaValue}>{capa.rootCauseAnalysis.methodology}</span>
                </div>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Analyst</span>
                  <span className={styles.metaValue}>{capa.rootCauseAnalysis.analystId}</span>
                </div>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Started</span>
                  <span className={styles.metaValue}>{new Date(capa.rootCauseAnalysis.startedAt).toLocaleDateString()}</span>
                </div>
                <div className={styles.metaItem}>
                  <span className={styles.metaLabel}>Completed</span>
                  <span className={styles.metaValue}>
                    {capa.rootCauseAnalysis.completedAt
                      ? new Date(capa.rootCauseAnalysis.completedAt).toLocaleDateString()
                      : 'In Progress'}
                  </span>
                </div>
                {capa.rootCauseAnalysis.rootCause && (
                  <div className={`${styles.metaItem} ${styles.fullWidth}`}>
                    <span className={styles.metaLabel}>Root Cause</span>
                    <span className={styles.metaValue}>{capa.rootCauseAnalysis.rootCause}</span>
                  </div>
                )}
                {capa.rootCauseAnalysis.analysisDetails && (
                  <div className={`${styles.metaItem} ${styles.fullWidth}`}>
                    <span className={styles.metaLabel}>Analysis Details</span>
                    <span className={styles.metaValue}>{capa.rootCauseAnalysis.analysisDetails}</span>
                  </div>
                )}
                {capa.rootCauseAnalysis.contributingFactors && (
                  <div className={`${styles.metaItem} ${styles.fullWidth}`}>
                    <span className={styles.metaLabel}>Contributing Factors</span>
                    <span className={styles.metaValue}>{capa.rootCauseAnalysis.contributingFactors}</span>
                  </div>
                )}
              </div>

              {canSubmitRca && !showSubmitRcaForm && (
                <div className={styles.inlineFormActions} style={{ marginTop: 'var(--spacing-4)' }}>
                  <Button size="sm" onClick={() => setShowSubmitRcaForm(true)}>
                    Submit RCA Findings
                  </Button>
                </div>
              )}

              {showSubmitRcaForm && (
                <div className={styles.actionForm} style={{ marginTop: 'var(--spacing-4)' }}>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Root Cause</label>
                    <textarea
                      className={styles.textarea}
                      value={rcaSubmitForm.rootCause}
                      onChange={(e) => setRcaSubmitForm(f => ({ ...f, rootCause: e.target.value }))}
                      rows={3} placeholder="Identified root cause..." required
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Analysis Details</label>
                    <textarea
                      className={styles.textarea}
                      value={rcaSubmitForm.analysisDetails}
                      onChange={(e) => setRcaSubmitForm(f => ({ ...f, analysisDetails: e.target.value }))}
                      rows={3} placeholder="Detailed analysis..."
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Contributing Factors</label>
                    <textarea
                      className={styles.textarea}
                      value={rcaSubmitForm.contributingFactors}
                      onChange={(e) => setRcaSubmitForm(f => ({ ...f, contributingFactors: e.target.value }))}
                      rows={2} placeholder="Any contributing factors..."
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => submitRcaMutation.mutate()}
                      disabled={!rcaSubmitForm.rootCause.trim() || submitRcaMutation.isPending}>
                      {submitRcaMutation.isPending ? 'Submitting...' : 'Submit'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowSubmitRcaForm(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </div>
          ) : canStartRca ? (
            <>
              {!showStartRcaForm ? (
                <div>
                  <p className={styles.emptyText}>No root cause analysis started yet.</p>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" icon={<Search size={14} />} onClick={() => setShowStartRcaForm(true)}>
                      Start RCA
                    </Button>
                  </div>
                </div>
              ) : (
                <div className={styles.actionForm}>
                  <div className={styles.actionFormGrid}>
                    <Select
                      label="Methodology"
                      options={methodologyOptions}
                      value={rcaStartForm.methodology}
                      onChange={(e) => setRcaStartForm(f => ({ ...f, methodology: e.target.value }))}
                    />
                    <Input
                      label="Analyst ID"
                      value={rcaStartForm.analystId}
                      onChange={(e) => setRcaStartForm(f => ({ ...f, analystId: e.target.value }))}
                      placeholder="Analyst user ID" required
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => startRcaMutation.mutate()}
                      disabled={!rcaStartForm.analystId.trim() || startRcaMutation.isPending}>
                      {startRcaMutation.isPending ? 'Starting...' : 'Start RCA'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowStartRcaForm(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </>
          ) : (
            <p className={styles.emptyText}>
              {isClosed ? 'RCA completed.' : 'RCA will be available in the appropriate lifecycle stage.'}
            </p>
          )}
        </div>
      )}

      {tab === 'actions' && (
        <div className={styles.tabContent}>
          {canAddAction && (
            <div className={styles.tabHeader}>
              {!showAddActionForm ? (
                <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowAddActionForm(true)}>
                  Add Action
                </Button>
              ) : (
                <div className={styles.actionForm}>
                  <div className={styles.actionFormGrid}>
                    <Select
                      label="Action Type"
                      options={actionTypeOptions}
                      value={actionForm.actionType}
                      onChange={(e) => setActionForm(f => ({ ...f, actionType: e.target.value }))}
                    />
                    <Input
                      label="Owner ID"
                      value={actionForm.ownerId}
                      onChange={(e) => setActionForm(f => ({ ...f, ownerId: e.target.value }))}
                      placeholder="Action owner user ID" required
                    />
                    <Input
                      label="Due Date"
                      type="date"
                      value={actionForm.dueDate}
                      onChange={(e) => setActionForm(f => ({ ...f, dueDate: e.target.value }))}
                      required
                    />
                    <Input
                      label="Evidence Requirement"
                      value={actionForm.evidenceRequirement}
                      onChange={(e) => setActionForm(f => ({ ...f, evidenceRequirement: e.target.value }))}
                      placeholder="What evidence is needed?"
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Description</label>
                    <textarea
                      className={styles.textarea}
                      value={actionForm.description}
                      onChange={(e) => setActionForm(f => ({ ...f, description: e.target.value }))}
                      rows={3} placeholder="Describe the corrective/preventive action..." required
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => addActionMutation.mutate()}
                      disabled={!actionForm.description.trim() || !actionForm.ownerId.trim() || !actionForm.dueDate || addActionMutation.isPending}>
                      {addActionMutation.isPending ? 'Adding...' : 'Add Action'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowAddActionForm(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {capa.actions.length === 0 ? (
            <p className={styles.emptyText}>
              No actions defined yet.
              {canAddAction && ' Add corrective or preventive actions.'}
            </p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Type</th>
                    <th>Description</th>
                    <th>Owner</th>
                    <th>Due Date</th>
                    <th>Status</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {capa.actions.map((a) => (
                    <tr key={a.id}>
                      <td>{a.actionType}</td>
                      <td>{a.description}</td>
                      <td>{a.ownerId}</td>
                      <td className={a.isOverdue ? styles.overdue : ''}>
                        {new Date(a.dueDate).toLocaleDateString()}
                        {a.isOverdue && (
                          <AlertTriangle size={12} style={{ marginLeft: 4, verticalAlign: 'middle' }} />
                        )}
                      </td>
                      <td>
                        {a.completedAt ? (
                          <span>
                            <CheckCircle size={14} style={{ color: 'var(--color-success)', verticalAlign: 'middle', marginRight: 4 }} />
                            {a.completedBy} ({new Date(a.completedAt).toLocaleDateString()})
                          </span>
                        ) : 'Open'}
                      </td>
                      <td>
                        {!a.completedAt && canCompleteAction && completingActionId !== a.id && (
                          <button className={styles.completeBtn} onClick={() => setCompletingActionId(a.id)}>
                            Complete
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {completingActionId && (
            <div className={styles.actionForm}>
              <div className={styles.descriptionField}>
                <label className={styles.fieldLabel}>Completion Notes</label>
                <textarea
                  className={styles.textarea}
                  value={completeForm.completionNotes}
                  onChange={(e) => setCompleteForm(f => ({ ...f, completionNotes: e.target.value }))}
                  rows={2} placeholder="Notes on how the action was completed..."
                />
              </div>
              <div className={styles.descriptionField}>
                <label className={styles.fieldLabel}>Evidence Provided</label>
                <textarea
                  className={styles.textarea}
                  value={completeForm.evidenceProvided}
                  onChange={(e) => setCompleteForm(f => ({ ...f, evidenceProvided: e.target.value }))}
                  rows={2} placeholder="Evidence of completion..."
                />
              </div>
              <div className={styles.inlineFormActions}>
                <Button size="sm" onClick={() => completeActionMutation.mutate(completingActionId)}
                  disabled={completeActionMutation.isPending}>
                  {completeActionMutation.isPending ? 'Completing...' : 'Mark Complete'}
                </Button>
                <Button size="sm" variant="ghost" onClick={() => setCompletingActionId(null)}>Cancel</Button>
              </div>
            </div>
          )}
        </div>
      )}

      {tab === 'verification' && (
        <div className={styles.tabContent}>
          {canScheduleVerification && (
            <div className={styles.tabHeader}>
              {!showScheduleVerification ? (
                <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowScheduleVerification(true)}>
                  Schedule Verification
                </Button>
              ) : (
                <div className={styles.actionForm}>
                  <div className={styles.actionFormGrid}>
                    <Input
                      label="Scheduled Date"
                      type="date"
                      value={verificationForm.scheduledDate}
                      onChange={(e) => setVerificationForm(f => ({ ...f, scheduledDate: e.target.value }))}
                      required
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Verification Criteria</label>
                    <textarea
                      className={styles.textarea}
                      value={verificationForm.verificationCriteria}
                      onChange={(e) => setVerificationForm(f => ({ ...f, verificationCriteria: e.target.value }))}
                      rows={3} placeholder="Define criteria for effectiveness verification..." required
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => scheduleVerificationMutation.mutate()}
                      disabled={!verificationForm.scheduledDate || !verificationForm.verificationCriteria.trim() || scheduleVerificationMutation.isPending}>
                      {scheduleVerificationMutation.isPending ? 'Scheduling...' : 'Schedule'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowScheduleVerification(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {capa.verifications.length === 0 ? (
            <p className={styles.emptyText}>
              No effectiveness verifications scheduled yet.
              {canScheduleVerification && ' Schedule a verification to validate CAPA effectiveness.'}
            </p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Scheduled Date</th>
                    <th>Criteria</th>
                    <th>Result</th>
                    <th>Effective</th>
                    <th>Verified By</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {capa.verifications.map((v) => (
                    <tr key={v.id}>
                      <td>{new Date(v.scheduledDate).toLocaleDateString()}</td>
                      <td>{v.verificationCriteria}</td>
                      <td>{v.result ?? '—'}</td>
                      <td>
                        {v.isEffective === null
                          ? 'Pending'
                          : v.isEffective
                            ? <span><CheckCircle size={14} style={{ color: 'var(--color-success)', verticalAlign: 'middle', marginRight: 4 }} />Yes</span>
                            : <span><AlertTriangle size={14} style={{ color: 'var(--color-error)', verticalAlign: 'middle', marginRight: 4 }} />No</span>}
                      </td>
                      <td>{v.verifierId ?? '—'}</td>
                      <td>
                        {v.verifiedAt === null && canRecordVerification && recordingVerificationId !== v.id && (
                          <button className={styles.completeBtn} onClick={() => setRecordingVerificationId(v.id)}>
                            Record
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {recordingVerificationId && (
            <div className={styles.actionForm}>
              <div className={styles.actionFormGrid}>
                <Select
                  label="Is Effective?"
                  options={[
                    { value: 'true', label: 'Yes - Effective' },
                    { value: 'false', label: 'No - Ineffective' },
                  ]}
                  value={recordForm.isEffective}
                  onChange={(e) => setRecordForm(f => ({ ...f, isEffective: e.target.value }))}
                />
              </div>
              <div className={styles.descriptionField}>
                <label className={styles.fieldLabel}>Result Description</label>
                <textarea
                  className={styles.textarea}
                  value={recordForm.result}
                  onChange={(e) => setRecordForm(f => ({ ...f, result: e.target.value }))}
                  rows={3} placeholder="Describe the verification result..." required
                />
              </div>
              <div className={styles.descriptionField}>
                <label className={styles.fieldLabel}>Evidence</label>
                <textarea
                  className={styles.textarea}
                  value={recordForm.evidence}
                  onChange={(e) => setRecordForm(f => ({ ...f, evidence: e.target.value }))}
                  rows={2} placeholder="Evidence supporting the result..."
                />
              </div>
              <div className={styles.inlineFormActions}>
                <Button size="sm" onClick={() => recordVerificationMutation.mutate(recordingVerificationId)}
                  disabled={!recordForm.result.trim() || recordVerificationMutation.isPending}>
                  {recordVerificationMutation.isPending ? 'Recording...' : 'Record Result'}
                </Button>
                <Button size="sm" variant="ghost" onClick={() => setRecordingVerificationId(null)}>Cancel</Button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}
