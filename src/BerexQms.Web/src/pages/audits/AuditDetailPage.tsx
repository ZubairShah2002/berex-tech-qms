import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft, Plus, PlayCircle, CheckCircle, XCircle,
  ClipboardList, AlertTriangle, FileText,
} from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { StatusBadge } from '@/components/ui/Badge'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './AuditDetailPage.module.css'

interface AuditRecordDto {
  id: string
  auditNumber: string
  auditType: string
  status: string
  leadAuditorId: string
  auditeeArea: string | null
  scheduledDate: string
  startedAt: string | null
  completedAt: string | null
  findingCount: number
  hasReport: boolean
}

interface AuditPlanDetail {
  id: string
  planName: string
  year: number
  description: string | null
  scope: string | null
  isActive: boolean
  audits: AuditRecordDto[]
  createdAt: string
  createdBy: string
  modifiedAt: string | null
}

interface FindingDto {
  id: string
  auditRecordId: string
  classification: string
  clauseReference: string
  description: string
  evidence: string | null
  correctiveAction: string | null
  linkedCapaId: string | null
  foundAt: string
}

interface ChecklistDto {
  id: string
  auditRecordId: string
  standard: string
  clauseReference: string
  requirement: string
  isCompliant: boolean
  evidence: string | null
  notes: string | null
}

interface AuditRecordDetail {
  id: string
  auditNumber: string
  auditType: string
  status: string
  leadAuditorId: string
  auditeeArea: string | null
  scheduledDate: string
  startedAt: string | null
  completedAt: string | null
  findings: FindingDto[]
  checklists: ChecklistDto[]
  report: { summary: string; recommendations: string; auditorNotes: string | null; generatedAt: string } | null
}

type Tab = 'audits' | 'findings' | 'checklists'

const auditTypeOptions = [
  { value: 'Internal', label: 'Internal' },
  { value: 'Supplier', label: 'Supplier' },
  { value: 'External', label: 'External' },
  { value: 'Certification', label: 'Certification' },
]

const classificationOptions = [
  { value: 'MajorNonConformance', label: 'Major NC' },
  { value: 'MinorNonConformance', label: 'Minor NC' },
  { value: 'Observation', label: 'Observation' },
  { value: 'OpportunityForImprovement', label: 'OFI' },
]

export function AuditDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('audits')
  const [actionError, setActionError] = useState('')
  const [selectedAuditId, setSelectedAuditId] = useState<string | null>(null)

  const [showAddAudit, setShowAddAudit] = useState(false)
  const [auditForm, setAuditForm] = useState({
    auditNumber: '', auditType: 'Internal', leadAuditorId: '',
    auditeeArea: '', scheduledDate: '',
  })

  const [showCompleteForm, setShowCompleteForm] = useState<string | null>(null)
  const [completeForm, setCompleteForm] = useState({
    summary: '', recommendations: '', auditorNotes: '',
  })

  const [showFindingForm, setShowFindingForm] = useState(false)
  const [findingForm, setFindingForm] = useState({
    classification: 'Observation', clauseReference: '', description: '',
    evidence: '', correctiveAction: '', linkedCapaId: '',
  })

  const [showChecklistForm, setShowChecklistForm] = useState(false)
  const [checklistForm, setChecklistForm] = useState({
    standard: '', clauseReference: '', requirement: '',
    isCompliant: 'true', evidence: '', notes: '',
  })

  const { data: plan, isLoading } = useQuery<AuditPlanDetail>({
    queryKey: ['audit-plan', id],
    queryFn: async () => {
      const res = await apiClient.get(`/audits/${id}`)
      return res.data
    },
    enabled: Boolean(id),
  })

  const { data: auditDetail } = useQuery<AuditRecordDetail>({
    queryKey: ['audit-record', selectedAuditId],
    queryFn: async () => {
      const res = await apiClient.get(`/audits/${id}`)
      const fullPlan = res.data as AuditPlanDetail
      const audit = fullPlan.audits.find(a => a.id === selectedAuditId)
      if (!audit) throw new Error('Audit not found')
      return audit as unknown as AuditRecordDetail
    },
    enabled: Boolean(selectedAuditId),
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
    queryClient.invalidateQueries({ queryKey: ['audit-plan', id] })
    queryClient.invalidateQueries({ queryKey: ['audit-record', selectedAuditId] })
    queryClient.invalidateQueries({ queryKey: ['audits'] })
  }

  const addAuditMutation = useMutation({
    mutationFn: () => apiClient.post(`/audits/${id}/audits`, {
      auditNumber: auditForm.auditNumber,
      auditType: auditForm.auditType,
      leadAuditorId: auditForm.leadAuditorId,
      auditeeArea: auditForm.auditeeArea || null,
      scheduledDate: auditForm.scheduledDate,
    }),
    onSuccess: () => {
      invalidate(); setShowAddAudit(false)
      setAuditForm({ auditNumber: '', auditType: 'Internal', leadAuditorId: '', auditeeArea: '', scheduledDate: '' })
    },
    onError: handleError,
  })

  const startMutation = useMutation({
    mutationFn: (auditId: string) => apiClient.put(`/audits/${id}/audits/${auditId}/start`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const cancelMutation = useMutation({
    mutationFn: (auditId: string) => apiClient.put(`/audits/${id}/audits/${auditId}/cancel`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const completeMutation = useMutation({
    mutationFn: (auditId: string) => apiClient.put(`/audits/${id}/audits/${auditId}/complete`, {
      summary: completeForm.summary,
      recommendations: completeForm.recommendations,
      auditorNotes: completeForm.auditorNotes || null,
    }),
    onSuccess: () => {
      invalidate(); setShowCompleteForm(null)
      setCompleteForm({ summary: '', recommendations: '', auditorNotes: '' })
    },
    onError: handleError,
  })

  const findingMutation = useMutation({
    mutationFn: (auditId: string) => apiClient.post(`/audits/${id}/audits/${auditId}/findings`, {
      classification: findingForm.classification,
      clauseReference: findingForm.clauseReference,
      description: findingForm.description,
      evidence: findingForm.evidence || null,
      correctiveAction: findingForm.correctiveAction || null,
      linkedCapaId: findingForm.linkedCapaId || null,
    }),
    onSuccess: () => {
      invalidate(); setShowFindingForm(false)
      setFindingForm({ classification: 'Observation', clauseReference: '', description: '', evidence: '', correctiveAction: '', linkedCapaId: '' })
    },
    onError: handleError,
  })

  const checklistMutation = useMutation({
    mutationFn: (auditId: string) => apiClient.post(`/audits/${id}/audits/${auditId}/checklists`, {
      standard: checklistForm.standard,
      clauseReference: checklistForm.clauseReference,
      requirement: checklistForm.requirement,
      isCompliant: checklistForm.isCompliant === 'true',
      evidence: checklistForm.evidence || null,
      notes: checklistForm.notes || null,
    }),
    onSuccess: () => {
      invalidate(); setShowChecklistForm(false)
      setChecklistForm({ standard: '', clauseReference: '', requirement: '', isCompliant: 'true', evidence: '', notes: '' })
    },
    onError: handleError,
  })

  if (isLoading || !plan) {
    return <div className={styles.page}>Loading...</div>
  }

  const selectedAudit = plan.audits.find(a => a.id === selectedAuditId)
  const canAddAudit = plan.isActive
  const canModifyAudit = (a: AuditRecordDto) => a.status !== 'Completed' && a.status !== 'Cancelled'

  return (
    <div className={styles.page}>
      <button className={styles.backLink} onClick={() => navigate('/audits')}>
        <ArrowLeft size={16} />
        Back to Audit Management
      </button>

      {actionError && <div className={styles.errorBanner}>{actionError}</div>}

      <div className={styles.header}>
        <div className={styles.headerInfo}>
          <div className={styles.titleRow}>
            <h1 className={styles.title}>{plan.planName}</h1>
            <StatusBadge status={plan.isActive ? 'Active' : 'Inactive'} />
          </div>
          <p className={styles.subtitle}>
            {plan.year} — {plan.audits.length} audit{plan.audits.length !== 1 ? 's' : ''}
          </p>
        </div>
      </div>

      <div className={styles.meta}>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Year</span>
          <span className={styles.metaValue}>{plan.year}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Created By</span>
          <span className={styles.metaValue}>{plan.createdBy}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Created</span>
          <span className={styles.metaValue}>{new Date(plan.createdAt).toLocaleDateString()}</span>
        </div>
        {plan.modifiedAt && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Modified</span>
            <span className={styles.metaValue}>{new Date(plan.modifiedAt).toLocaleDateString()}</span>
          </div>
        )}
      </div>

      {plan.description && (
        <div className={styles.descriptionSection}>
          <h3 className={styles.sectionTitle}>Description</h3>
          <p className={styles.descriptionText}>{plan.description}</p>
        </div>
      )}

      {plan.scope && (
        <div className={styles.descriptionSection}>
          <h3 className={styles.sectionTitle}>Scope</h3>
          <p className={styles.descriptionText}>{plan.scope}</p>
        </div>
      )}

      <div className={styles.tabs}>
        <button className={`${styles.tab} ${tab === 'audits' ? styles.tabActive : ''}`} onClick={() => { setTab('audits'); setSelectedAuditId(null) }}>
          <ClipboardList size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Audit Records ({plan.audits.length})
        </button>
        <button className={`${styles.tab} ${tab === 'findings' ? styles.tabActive : ''}`} onClick={() => setTab('findings')} disabled={!selectedAuditId}>
          <AlertTriangle size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Findings
        </button>
        <button className={`${styles.tab} ${tab === 'checklists' ? styles.tabActive : ''}`} onClick={() => setTab('checklists')} disabled={!selectedAuditId}>
          <FileText size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Checklists
        </button>
      </div>

      {tab === 'audits' && (
        <div className={styles.tabContent}>
          {canAddAudit && (
            <div className={styles.tabHeader}>
              {!showAddAudit ? (
                <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowAddAudit(true)}>
                  Add Audit
                </Button>
              ) : (
                <div className={styles.actionForm}>
                  <div className={styles.actionFormGrid}>
                    <Input
                      label="Audit Number"
                      value={auditForm.auditNumber}
                      onChange={(e) => setAuditForm(f => ({ ...f, auditNumber: e.target.value }))}
                      placeholder="e.g. AUD-2026-001" required
                    />
                    <Select
                      label="Audit Type"
                      options={auditTypeOptions}
                      value={auditForm.auditType}
                      onChange={(e) => setAuditForm(f => ({ ...f, auditType: e.target.value }))}
                    />
                    <Input
                      label="Lead Auditor ID"
                      value={auditForm.leadAuditorId}
                      onChange={(e) => setAuditForm(f => ({ ...f, leadAuditorId: e.target.value }))}
                      placeholder="Auditor user ID" required
                    />
                    <Input
                      label="Auditee Area"
                      value={auditForm.auditeeArea}
                      onChange={(e) => setAuditForm(f => ({ ...f, auditeeArea: e.target.value }))}
                      placeholder="e.g. Manufacturing"
                    />
                    <Input
                      label="Scheduled Date"
                      type="date"
                      value={auditForm.scheduledDate}
                      onChange={(e) => setAuditForm(f => ({ ...f, scheduledDate: e.target.value }))}
                      required
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => addAuditMutation.mutate()}
                      disabled={!auditForm.auditNumber.trim() || !auditForm.leadAuditorId.trim() || !auditForm.scheduledDate || addAuditMutation.isPending}>
                      {addAuditMutation.isPending ? 'Adding...' : 'Add Audit'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowAddAudit(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {plan.audits.length === 0 ? (
            <p className={styles.emptyText}>
              No audits scheduled yet.{canAddAudit && ' Add an audit record to get started.'}
            </p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Audit #</th>
                    <th>Type</th>
                    <th>Status</th>
                    <th>Lead Auditor</th>
                    <th>Area</th>
                    <th>Scheduled</th>
                    <th>Findings</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {plan.audits.map((a) => (
                    <tr key={a.id} className={selectedAuditId === a.id ? styles.selectedRow : ''}>
                      <td>
                        <button className={styles.linkButton} onClick={() => { setSelectedAuditId(a.id); setTab('findings') }}>
                          {a.auditNumber}
                        </button>
                      </td>
                      <td>{a.auditType}</td>
                      <td><StatusBadge status={a.status} /></td>
                      <td>{a.leadAuditorId}</td>
                      <td>{a.auditeeArea ?? '—'}</td>
                      <td>{new Date(a.scheduledDate).toLocaleDateString()}</td>
                      <td>{a.findingCount}</td>
                      <td className={styles.actionCell}>
                        {a.status === 'Planned' && (
                          <>
                            <button className={styles.actionBtn} title="Start" onClick={() => startMutation.mutate(a.id)}>
                              <PlayCircle size={14} />
                            </button>
                            <button className={styles.actionBtnDanger} title="Cancel" onClick={() => cancelMutation.mutate(a.id)}>
                              <XCircle size={14} />
                            </button>
                          </>
                        )}
                        {a.status === 'InProgress' && (
                          <>
                            <button className={styles.actionBtn} title="Complete" onClick={() => setShowCompleteForm(a.id)}>
                              <CheckCircle size={14} />
                            </button>
                            <button className={styles.actionBtnDanger} title="Cancel" onClick={() => cancelMutation.mutate(a.id)}>
                              <XCircle size={14} />
                            </button>
                          </>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {showCompleteForm && (
            <div className={styles.actionForm}>
              <h3 className={styles.sectionTitle}>Complete Audit Report</h3>
              <div className={styles.descriptionField}>
                <label className={styles.fieldLabel}>Summary</label>
                <textarea
                  className={styles.textarea}
                  value={completeForm.summary}
                  onChange={(e) => setCompleteForm(f => ({ ...f, summary: e.target.value }))}
                  rows={3} placeholder="Audit summary..." required
                />
              </div>
              <div className={styles.descriptionField}>
                <label className={styles.fieldLabel}>Recommendations</label>
                <textarea
                  className={styles.textarea}
                  value={completeForm.recommendations}
                  onChange={(e) => setCompleteForm(f => ({ ...f, recommendations: e.target.value }))}
                  rows={3} placeholder="Recommendations..." required
                />
              </div>
              <div className={styles.descriptionField}>
                <label className={styles.fieldLabel}>Auditor Notes</label>
                <textarea
                  className={styles.textarea}
                  value={completeForm.auditorNotes}
                  onChange={(e) => setCompleteForm(f => ({ ...f, auditorNotes: e.target.value }))}
                  rows={2} placeholder="Additional notes..."
                />
              </div>
              <div className={styles.inlineFormActions}>
                <Button size="sm" onClick={() => completeMutation.mutate(showCompleteForm)}
                  disabled={!completeForm.summary.trim() || !completeForm.recommendations.trim() || completeMutation.isPending}>
                  {completeMutation.isPending ? 'Completing...' : 'Complete Audit'}
                </Button>
                <Button size="sm" variant="ghost" onClick={() => setShowCompleteForm(null)}>Cancel</Button>
              </div>
            </div>
          )}
        </div>
      )}

      {tab === 'findings' && selectedAuditId && (
        <div className={styles.tabContent}>
          <div className={styles.auditContext}>
            Viewing findings for: <strong>{selectedAudit?.auditNumber}</strong>
            <StatusBadge status={selectedAudit?.status ?? ''} />
          </div>

          {canModifyAudit(selectedAudit!) && (
            <div className={styles.tabHeader}>
              {!showFindingForm ? (
                <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowFindingForm(true)}>
                  Record Finding
                </Button>
              ) : (
                <div className={styles.actionForm}>
                  <div className={styles.actionFormGrid}>
                    <Select
                      label="Classification"
                      options={classificationOptions}
                      value={findingForm.classification}
                      onChange={(e) => setFindingForm(f => ({ ...f, classification: e.target.value }))}
                    />
                    <Input
                      label="Clause Reference"
                      value={findingForm.clauseReference}
                      onChange={(e) => setFindingForm(f => ({ ...f, clauseReference: e.target.value }))}
                      placeholder="e.g. 8.5.1" required
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Description</label>
                    <textarea
                      className={styles.textarea}
                      value={findingForm.description}
                      onChange={(e) => setFindingForm(f => ({ ...f, description: e.target.value }))}
                      rows={3} placeholder="Describe the finding..." required
                    />
                  </div>
                  <div className={styles.actionFormGrid}>
                    <div className={styles.descriptionField}>
                      <label className={styles.fieldLabel}>Evidence</label>
                      <textarea
                        className={styles.textarea}
                        value={findingForm.evidence}
                        onChange={(e) => setFindingForm(f => ({ ...f, evidence: e.target.value }))}
                        rows={2} placeholder="Supporting evidence..."
                      />
                    </div>
                    <div className={styles.descriptionField}>
                      <label className={styles.fieldLabel}>Corrective Action</label>
                      <textarea
                        className={styles.textarea}
                        value={findingForm.correctiveAction}
                        onChange={(e) => setFindingForm(f => ({ ...f, correctiveAction: e.target.value }))}
                        rows={2} placeholder="Recommended corrective action..."
                      />
                    </div>
                  </div>
                  <Input
                    label="Linked CAPA ID"
                    value={findingForm.linkedCapaId}
                    onChange={(e) => setFindingForm(f => ({ ...f, linkedCapaId: e.target.value }))}
                    placeholder="Optional CAPA reference"
                  />
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => findingMutation.mutate(selectedAuditId)}
                      disabled={!findingForm.clauseReference.trim() || !findingForm.description.trim() || findingMutation.isPending}>
                      {findingMutation.isPending ? 'Recording...' : 'Record Finding'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowFindingForm(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {!auditDetail || (auditDetail as unknown as AuditRecordDetail).findings === undefined ? (
            <p className={styles.emptyText}>
              Select an audit from the records tab to view findings, or findings data is not yet available with the current detail level.
            </p>
          ) : (auditDetail as unknown as AuditRecordDetail).findings.length === 0 ? (
            <p className={styles.emptyText}>No findings recorded for this audit.</p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Classification</th>
                    <th>Clause</th>
                    <th>Description</th>
                    <th>Evidence</th>
                    <th>Found</th>
                  </tr>
                </thead>
                <tbody>
                  {(auditDetail as unknown as AuditRecordDetail).findings.map((f: FindingDto) => (
                    <tr key={f.id}>
                      <td><StatusBadge status={f.classification} /></td>
                      <td>{f.clauseReference}</td>
                      <td>{f.description.length > 80 ? `${f.description.substring(0, 80)}...` : f.description}</td>
                      <td>{f.evidence ? (f.evidence.length > 40 ? `${f.evidence.substring(0, 40)}...` : f.evidence) : '—'}</td>
                      <td>{new Date(f.foundAt).toLocaleDateString()}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {tab === 'checklists' && selectedAuditId && (
        <div className={styles.tabContent}>
          <div className={styles.auditContext}>
            Viewing checklists for: <strong>{selectedAudit?.auditNumber}</strong>
            <StatusBadge status={selectedAudit?.status ?? ''} />
          </div>

          {canModifyAudit(selectedAudit!) && (
            <div className={styles.tabHeader}>
              {!showChecklistForm ? (
                <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowChecklistForm(true)}>
                  Add Checklist Item
                </Button>
              ) : (
                <div className={styles.actionForm}>
                  <div className={styles.actionFormGrid}>
                    <Input
                      label="Standard"
                      value={checklistForm.standard}
                      onChange={(e) => setChecklistForm(f => ({ ...f, standard: e.target.value }))}
                      placeholder="e.g. ISO 9001:2015" required
                    />
                    <Input
                      label="Clause Reference"
                      value={checklistForm.clauseReference}
                      onChange={(e) => setChecklistForm(f => ({ ...f, clauseReference: e.target.value }))}
                      placeholder="e.g. 7.1.5" required
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Requirement</label>
                    <textarea
                      className={styles.textarea}
                      value={checklistForm.requirement}
                      onChange={(e) => setChecklistForm(f => ({ ...f, requirement: e.target.value }))}
                      rows={2} placeholder="Requirement description..." required
                    />
                  </div>
                  <Select
                    label="Compliance"
                    options={[
                      { value: 'true', label: 'Compliant' },
                      { value: 'false', label: 'Non-Compliant' },
                    ]}
                    value={checklistForm.isCompliant}
                    onChange={(e) => setChecklistForm(f => ({ ...f, isCompliant: e.target.value }))}
                  />
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Evidence</label>
                    <textarea
                      className={styles.textarea}
                      value={checklistForm.evidence}
                      onChange={(e) => setChecklistForm(f => ({ ...f, evidence: e.target.value }))}
                      rows={2} placeholder="Objective evidence..."
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Notes</label>
                    <textarea
                      className={styles.textarea}
                      value={checklistForm.notes}
                      onChange={(e) => setChecklistForm(f => ({ ...f, notes: e.target.value }))}
                      rows={2} placeholder="Additional notes..."
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => checklistMutation.mutate(selectedAuditId)}
                      disabled={!checklistForm.standard.trim() || !checklistForm.clauseReference.trim() || !checklistForm.requirement.trim() || checklistMutation.isPending}>
                      {checklistMutation.isPending ? 'Adding...' : 'Add Item'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowChecklistForm(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {!auditDetail || (auditDetail as unknown as AuditRecordDetail).checklists === undefined ? (
            <p className={styles.emptyText}>
              Select an audit from the records tab to view checklists.
            </p>
          ) : (auditDetail as unknown as AuditRecordDetail).checklists.length === 0 ? (
            <p className={styles.emptyText}>No checklist items for this audit.</p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Standard</th>
                    <th>Clause</th>
                    <th>Requirement</th>
                    <th>Compliant</th>
                    <th>Evidence</th>
                  </tr>
                </thead>
                <tbody>
                  {(auditDetail as unknown as AuditRecordDetail).checklists.map((c: ChecklistDto) => (
                    <tr key={c.id}>
                      <td>{c.standard}</td>
                      <td>{c.clauseReference}</td>
                      <td>{c.requirement.length > 80 ? `${c.requirement.substring(0, 80)}...` : c.requirement}</td>
                      <td>
                        {c.isCompliant
                          ? <CheckCircle size={14} style={{ color: 'var(--color-success)' }} />
                          : <XCircle size={14} style={{ color: 'var(--color-error)' }} />}
                      </td>
                      <td>{c.evidence ? (c.evidence.length > 40 ? `${c.evidence.substring(0, 40)}...` : c.evidence) : '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {!selectedAuditId && (tab === 'findings' || tab === 'checklists') && (
        <p className={styles.emptyText}>Select an audit record first to view {tab}.</p>
      )}
    </div>
  )
}
