import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  ArrowLeft, Plus, FileText, CheckCircle, Send,
  ShieldCheck, Users, Archive,
} from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { StatusBadge } from '@/components/ui/Badge'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import styles from './DocumentDetailPage.module.css'

interface AttachmentDto {
  fileName: string
  contentType: string
  sizeBytes: number
  storagePath: string
}

interface VersionDto {
  id: string
  versionNumber: string
  status: string
  content: string
  changeDescription: string | null
  authorId: string
  effectiveDate: string | null
  attachment: AttachmentDto | null
  createdAt: string
  releasedAt: string | null
  releasedBy: string | null
}

interface DocumentDetail {
  id: string
  documentNumber: string
  title: string
  description: string | null
  documentType: string
  ownerId: string
  department: string | null
  isActive: boolean
  versions: VersionDto[]
  createdAt: string
  createdBy: string
  modifiedAt: string | null
}

type Tab = 'versions' | 'approval' | 'distribution'

const approvalDecisionOptions = [
  { value: 'Approved', label: 'Approve' },
  { value: 'ApprovedWithComments', label: 'Approve with Comments' },
  { value: 'Rejected', label: 'Reject' },
]

export function DocumentDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('versions')
  const [actionError, setActionError] = useState('')

  const [showCreateVersion, setShowCreateVersion] = useState(false)
  const [versionForm, setVersionForm] = useState({
    versionNumber: '', content: '', changeDescription: '',
  })

  const [selectedVersionId, setSelectedVersionId] = useState<string | null>(null)

  const [showStartApproval, setShowStartApproval] = useState(false)
  const [approvalForm, setApprovalForm] = useState({ approverIds: '' })

  const [showRecordApproval, setShowRecordApproval] = useState(false)
  const [recordApprovalForm, setRecordApprovalForm] = useState({
    decision: 'Approved', comments: '', signature: '',
  })

  const [showReleaseForm, setShowReleaseForm] = useState(false)
  const [releaseForm, setReleaseForm] = useState({ effectiveDate: '' })

  const [showAddDistribution, setShowAddDistribution] = useState(false)
  const [distributionForm, setDistributionForm] = useState({
    recipientId: '', complianceDeadline: '',
  })

  const { data: doc, isLoading } = useQuery<DocumentDetail>({
    queryKey: ['document', id],
    queryFn: async () => {
      const res = await apiClient.get(`/documents/${id}`)
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
    queryClient.invalidateQueries({ queryKey: ['document', id] })
    queryClient.invalidateQueries({ queryKey: ['documents'] })
  }

  const createVersionMutation = useMutation({
    mutationFn: () => apiClient.post(`/documents/${id}/versions`, {
      versionNumber: versionForm.versionNumber,
      content: versionForm.content,
      changeDescription: versionForm.changeDescription || null,
    }),
    onSuccess: () => {
      invalidate(); setShowCreateVersion(false)
      setVersionForm({ versionNumber: '', content: '', changeDescription: '' })
    },
    onError: handleError,
  })

  const submitForReviewMutation = useMutation({
    mutationFn: (versionId: string) =>
      apiClient.post(`/documents/${id}/versions/${versionId}/submit-for-review`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const startApprovalMutation = useMutation({
    mutationFn: (versionId: string) =>
      apiClient.post(`/documents/${id}/versions/${versionId}/start-approval`, {
        approverIds: approvalForm.approverIds.split(',').map(s => s.trim()).filter(Boolean),
      }),
    onSuccess: () => {
      invalidate(); setShowStartApproval(false)
      setApprovalForm({ approverIds: '' })
    },
    onError: handleError,
  })

  const recordApprovalMutation = useMutation({
    mutationFn: (versionId: string) =>
      apiClient.post(`/documents/${id}/versions/${versionId}/record-approval`, {
        decision: recordApprovalForm.decision,
        comments: recordApprovalForm.comments || null,
        signature: recordApprovalForm.signature || null,
      }),
    onSuccess: () => {
      invalidate(); setShowRecordApproval(false)
      setRecordApprovalForm({ decision: 'Approved', comments: '', signature: '' })
    },
    onError: handleError,
  })

  const releaseMutation = useMutation({
    mutationFn: (versionId: string) =>
      apiClient.post(`/documents/${id}/versions/${versionId}/release`, {
        effectiveDate: releaseForm.effectiveDate,
      }),
    onSuccess: () => {
      invalidate(); setShowReleaseForm(false)
      setReleaseForm({ effectiveDate: '' })
    },
    onError: handleError,
  })

  const addDistributionMutation = useMutation({
    mutationFn: (versionId: string) =>
      apiClient.post(`/documents/${id}/versions/${versionId}/distributions`, {
        recipientId: distributionForm.recipientId,
        complianceDeadline: distributionForm.complianceDeadline,
      }),
    onSuccess: () => {
      invalidate(); setShowAddDistribution(false)
      setDistributionForm({ recipientId: '', complianceDeadline: '' })
    },
    onError: handleError,
  })

  const acknowledgeMutation = useMutation({
    mutationFn: (distributionId: string) =>
      apiClient.post(`/documents/${id}/distributions/${distributionId}/acknowledge`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const makeObsoleteMutation = useMutation({
    mutationFn: () => apiClient.post(`/documents/${id}/make-obsolete`),
    onSuccess: invalidate,
    onError: handleError,
  })

  if (isLoading || !doc) {
    return <div className={styles.page}>Loading...</div>
  }

  const latestVersion = doc.versions[0] ?? null
  const releasedVersion = doc.versions.find(v => v.status === 'Released') ?? null
  const activeVersion = releasedVersion ?? latestVersion

  return (
    <div className={styles.page}>
      <button className={styles.backLink} onClick={() => navigate('/documents')}>
        <ArrowLeft size={16} />
        Back to Document Control
      </button>

      {actionError && <div className={styles.errorBanner}>{actionError}</div>}

      <div className={styles.header}>
        <div className={styles.headerInfo}>
          <div className={styles.titleRow}>
            <h1 className={styles.title}>{doc.documentNumber}</h1>
            {activeVersion && <StatusBadge status={activeVersion.status} />}
            {!doc.isActive && <StatusBadge status="Inactive" />}
          </div>
          <p className={styles.subtitle}>{doc.title}</p>
        </div>
        <div className={styles.headerActions}>
          {doc.isActive && (
            <Button
              size="sm"
              variant="secondary"
              icon={<Archive size={14} />}
              onClick={() => makeObsoleteMutation.mutate()}
              disabled={makeObsoleteMutation.isPending}
            >
              Make Obsolete
            </Button>
          )}
        </div>
      </div>

      <div className={styles.meta}>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Type</span>
          <span className={styles.metaValue}>{doc.documentType}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Owner</span>
          <span className={styles.metaValue}>{doc.ownerId}</span>
        </div>
        {doc.department && (
          <div className={styles.metaItem}>
            <span className={styles.metaLabel}>Department</span>
            <span className={styles.metaValue}>{doc.department}</span>
          </div>
        )}
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Created</span>
          <span className={styles.metaValue}>{new Date(doc.createdAt).toLocaleDateString()}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Versions</span>
          <span className={styles.metaValue}>{doc.versions.length}</span>
        </div>
      </div>

      {doc.description && (
        <div className={styles.descriptionSection}>
          <h3 className={styles.sectionTitle}>Description</h3>
          <p className={styles.descriptionText}>{doc.description}</p>
        </div>
      )}

      <div className={styles.tabs}>
        <button className={`${styles.tab} ${tab === 'versions' ? styles.tabActive : ''}`} onClick={() => setTab('versions')}>
          <FileText size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Versions ({doc.versions.length})
        </button>
        <button className={`${styles.tab} ${tab === 'approval' ? styles.tabActive : ''}`} onClick={() => setTab('approval')}>
          <ShieldCheck size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Approval
        </button>
        <button className={`${styles.tab} ${tab === 'distribution' ? styles.tabActive : ''}`} onClick={() => setTab('distribution')}>
          <Users size={14} style={{ marginRight: 4, verticalAlign: 'middle' }} />
          Distribution
        </button>
      </div>

      {tab === 'versions' && (
        <div className={styles.tabContent}>
          {doc.isActive && (
            <div className={styles.tabHeader}>
              {!showCreateVersion ? (
                <Button size="sm" icon={<Plus size={14} />} onClick={() => setShowCreateVersion(true)}>
                  New Version
                </Button>
              ) : (
                <div className={styles.actionForm}>
                  <div className={styles.actionFormGrid}>
                    <Input
                      label="Version Number"
                      value={versionForm.versionNumber}
                      onChange={(e) => setVersionForm(f => ({ ...f, versionNumber: e.target.value }))}
                      placeholder="e.g. 1.0, 2.0"
                      required
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Content</label>
                    <textarea
                      className={styles.textarea}
                      value={versionForm.content}
                      onChange={(e) => setVersionForm(f => ({ ...f, content: e.target.value }))}
                      rows={6}
                      placeholder="Document content..."
                      required
                    />
                  </div>
                  <div className={styles.descriptionField}>
                    <label className={styles.fieldLabel}>Change Description</label>
                    <textarea
                      className={styles.textarea}
                      value={versionForm.changeDescription}
                      onChange={(e) => setVersionForm(f => ({ ...f, changeDescription: e.target.value }))}
                      rows={2}
                      placeholder="What changed in this version..."
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => createVersionMutation.mutate()}
                      disabled={!versionForm.versionNumber.trim() || !versionForm.content.trim() || createVersionMutation.isPending}>
                      {createVersionMutation.isPending ? 'Creating...' : 'Create Version'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setShowCreateVersion(false)}>Cancel</Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {doc.versions.length === 0 ? (
            <p className={styles.emptyText}>No versions yet. Create the first version.</p>
          ) : (
            <div className={styles.tableWrapper}>
              <table className={styles.dataTable}>
                <thead>
                  <tr>
                    <th>Version</th>
                    <th>Status</th>
                    <th>Author</th>
                    <th>Created</th>
                    <th>Effective</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {doc.versions.map((v) => (
                    <tr key={v.id} className={selectedVersionId === v.id ? styles.selectedRow : ''}>
                      <td>
                        <button className={styles.versionLink} onClick={() => setSelectedVersionId(selectedVersionId === v.id ? null : v.id)}>
                          {v.versionNumber}
                        </button>
                      </td>
                      <td><StatusBadge status={v.status} /></td>
                      <td>{v.authorId}</td>
                      <td>{new Date(v.createdAt).toLocaleDateString()}</td>
                      <td>{v.effectiveDate ? new Date(v.effectiveDate).toLocaleDateString() : '—'}</td>
                      <td>
                        <div className={styles.versionActions}>
                          {v.status === 'Draft' && (
                            <button className={styles.actionBtn} onClick={() => submitForReviewMutation.mutate(v.id)}
                              disabled={submitForReviewMutation.isPending}>
                              <Send size={12} /> Review
                            </button>
                          )}
                          {v.status === 'UnderReview' && (
                            <button className={styles.actionBtn} onClick={() => { setSelectedVersionId(v.id); setShowStartApproval(true); setTab('approval') }}>
                              <ShieldCheck size={12} /> Approval
                            </button>
                          )}
                          {v.status === 'Released' && (
                            <button className={styles.actionBtn} onClick={() => { setSelectedVersionId(v.id); setTab('distribution') }}>
                              <Users size={12} /> Distribute
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {selectedVersionId && doc.versions.find(v => v.id === selectedVersionId) && (
            <VersionDetail version={doc.versions.find(v => v.id === selectedVersionId)!} />
          )}
        </div>
      )}

      {tab === 'approval' && (
        <ApprovalTab
          docId={id!}
          versions={doc.versions}
          selectedVersionId={selectedVersionId}
          setSelectedVersionId={setSelectedVersionId}
          showStartApproval={showStartApproval}
          setShowStartApproval={setShowStartApproval}
          approvalForm={approvalForm}
          setApprovalForm={setApprovalForm}
          startApprovalMutation={startApprovalMutation}
          showRecordApproval={showRecordApproval}
          setShowRecordApproval={setShowRecordApproval}
          recordApprovalForm={recordApprovalForm}
          setRecordApprovalForm={setRecordApprovalForm}
          recordApprovalMutation={recordApprovalMutation}
          showReleaseForm={showReleaseForm}
          setShowReleaseForm={setShowReleaseForm}
          releaseForm={releaseForm}
          setReleaseForm={setReleaseForm}
          releaseMutation={releaseMutation}
          handleError={handleError}
        />
      )}

      {tab === 'distribution' && (
        <DistributionTab
          docId={id!}
          versions={doc.versions}
          selectedVersionId={selectedVersionId}
          setSelectedVersionId={setSelectedVersionId}
          showAddDistribution={showAddDistribution}
          setShowAddDistribution={setShowAddDistribution}
          distributionForm={distributionForm}
          setDistributionForm={setDistributionForm}
          addDistributionMutation={addDistributionMutation}
          acknowledgeMutation={acknowledgeMutation}
        />
      )}
    </div>
  )
}

function VersionDetail({ version }: { version: VersionDto }) {
  return (
    <div className={styles.versionDetail}>
      <h4 className={styles.sectionTitle}>Version {version.versionNumber} — Content</h4>
      <pre className={styles.contentPre}>{version.content}</pre>
      {version.changeDescription && (
        <>
          <h4 className={styles.sectionTitle}>Change Description</h4>
          <p className={styles.descriptionText}>{version.changeDescription}</p>
        </>
      )}
      {version.releasedAt && (
        <p className={styles.releaseMeta}>
          Released by {version.releasedBy} on {new Date(version.releasedAt).toLocaleDateString()}
        </p>
      )}
    </div>
  )
}

function ApprovalTab({
  docId: _docIdApproval, versions, selectedVersionId, setSelectedVersionId,
  showStartApproval, setShowStartApproval, approvalForm, setApprovalForm,
  startApprovalMutation,
  showRecordApproval, setShowRecordApproval, recordApprovalForm, setRecordApprovalForm,
  recordApprovalMutation,
  showReleaseForm, setShowReleaseForm, releaseForm, setReleaseForm, releaseMutation,
}: {
  docId: string
  versions: VersionDto[]
  selectedVersionId: string | null
  setSelectedVersionId: (id: string | null) => void
  showStartApproval: boolean
  setShowStartApproval: (v: boolean) => void
  approvalForm: { approverIds: string }
  setApprovalForm: (fn: (f: { approverIds: string }) => { approverIds: string }) => void
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  startApprovalMutation: any
  showRecordApproval: boolean
  setShowRecordApproval: (v: boolean) => void
  recordApprovalForm: { decision: string; comments: string; signature: string }
  setRecordApprovalForm: (fn: (f: { decision: string; comments: string; signature: string }) => { decision: string; comments: string; signature: string }) => void
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  recordApprovalMutation: any
  showReleaseForm: boolean
  setShowReleaseForm: (v: boolean) => void
  releaseForm: { effectiveDate: string }
  setReleaseForm: (fn: (f: { effectiveDate: string }) => { effectiveDate: string }) => void
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  releaseMutation: any
  handleError: (err: unknown) => void
}) {
  void _docIdApproval
  const pendingVersion = versions.find(v => v.status === 'PendingApproval')
  const underReviewVersion = versions.find(v => v.status === 'UnderReview')
  const targetVersion = pendingVersion ?? underReviewVersion

  return (
    <div className={styles.tabContent}>
      {underReviewVersion && !showStartApproval && (
        <div className={styles.tabHeader}>
          <Button size="sm" icon={<ShieldCheck size={14} />}
            onClick={() => {
              setSelectedVersionId(underReviewVersion.id)
              setShowStartApproval(true)
            }}>
            Start Approval for v{underReviewVersion.versionNumber}
          </Button>
        </div>
      )}

      {showStartApproval && (
        <div className={styles.actionForm}>
          <Input
            label="Approver IDs (comma-separated)"
            value={approvalForm.approverIds}
            onChange={(e) => setApprovalForm(() => ({ approverIds: e.target.value }))}
            placeholder="user-id-1, user-id-2"
            required
          />
          <div className={styles.inlineFormActions}>
            <Button size="sm"
              onClick={() => startApprovalMutation.mutate(selectedVersionId ?? underReviewVersion?.id)}
              disabled={!approvalForm.approverIds.trim() || startApprovalMutation.isPending}>
              {startApprovalMutation.isPending ? 'Starting...' : 'Start Approval'}
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setShowStartApproval(false)}>Cancel</Button>
          </div>
        </div>
      )}

      {pendingVersion && !showRecordApproval && !showReleaseForm && (
        <div className={styles.tabHeader}>
          <div className={styles.approvalActions}>
            <Button size="sm" icon={<CheckCircle size={14} />}
              onClick={() => {
                setSelectedVersionId(pendingVersion.id)
                setShowRecordApproval(true)
              }}>
              Record Decision
            </Button>
            <Button size="sm" variant="secondary" icon={<Send size={14} />}
              onClick={() => {
                setSelectedVersionId(pendingVersion.id)
                setShowReleaseForm(true)
              }}>
              Release
            </Button>
          </div>
        </div>
      )}

      {showRecordApproval && (
        <div className={styles.actionForm}>
          <div className={styles.actionFormGrid}>
            <Select
              label="Decision"
              options={approvalDecisionOptions}
              value={recordApprovalForm.decision}
              onChange={(e) => setRecordApprovalForm(f => ({ ...f, decision: e.target.value }))}
            />
            <Input
              label="E-Signature"
              value={recordApprovalForm.signature}
              onChange={(e) => setRecordApprovalForm(f => ({ ...f, signature: e.target.value }))}
              placeholder="Your digital signature"
            />
          </div>
          <div className={styles.descriptionField}>
            <label className={styles.fieldLabel}>Comments</label>
            <textarea
              className={styles.textarea}
              value={recordApprovalForm.comments}
              onChange={(e) => setRecordApprovalForm(f => ({ ...f, comments: e.target.value }))}
              rows={3}
              placeholder="Approval comments..."
            />
          </div>
          <div className={styles.inlineFormActions}>
            <Button size="sm"
              onClick={() => recordApprovalMutation.mutate(selectedVersionId ?? pendingVersion?.id)}
              disabled={recordApprovalMutation.isPending}>
              {recordApprovalMutation.isPending ? 'Recording...' : 'Submit Decision'}
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setShowRecordApproval(false)}>Cancel</Button>
          </div>
        </div>
      )}

      {showReleaseForm && (
        <div className={styles.actionForm}>
          <Input
            label="Effective Date"
            type="date"
            value={releaseForm.effectiveDate}
            onChange={(e) => setReleaseForm(() => ({ effectiveDate: e.target.value }))}
            required
          />
          <div className={styles.inlineFormActions}>
            <Button size="sm"
              onClick={() => releaseMutation.mutate(selectedVersionId ?? pendingVersion?.id)}
              disabled={!releaseForm.effectiveDate || releaseMutation.isPending}>
              {releaseMutation.isPending ? 'Releasing...' : 'Release Version'}
            </Button>
            <Button size="sm" variant="ghost" onClick={() => setShowReleaseForm(false)}>Cancel</Button>
          </div>
        </div>
      )}

      {!targetVersion && !showStartApproval && (
        <p className={styles.emptyText}>
          No versions currently under review or pending approval.
          Submit a draft version for review to begin the approval process.
        </p>
      )}

    </div>
  )
}

function DistributionTab({
  docId: _docId, versions, selectedVersionId, setSelectedVersionId,
  showAddDistribution, setShowAddDistribution,
  distributionForm, setDistributionForm,
  addDistributionMutation, acknowledgeMutation: _acknowledgeMutation,
}: {
  docId: string
  versions: VersionDto[]
  selectedVersionId: string | null
  setSelectedVersionId: (id: string | null) => void
  showAddDistribution: boolean
  setShowAddDistribution: (v: boolean) => void
  distributionForm: { recipientId: string; complianceDeadline: string }
  setDistributionForm: (fn: (f: { recipientId: string; complianceDeadline: string }) => { recipientId: string; complianceDeadline: string }) => void
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  addDistributionMutation: any
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  acknowledgeMutation: any
}) {
  void _acknowledgeMutation; void _docId
  const releasedVersion = versions.find(v => v.status === 'Released')
  const targetVersionId = selectedVersionId ?? releasedVersion?.id

  return (
    <div className={styles.tabContent}>
      {releasedVersion && (
        <div className={styles.tabHeader}>
          {!showAddDistribution ? (
            <Button size="sm" icon={<Plus size={14} />}
              onClick={() => {
                setSelectedVersionId(releasedVersion.id)
                setShowAddDistribution(true)
              }}>
              Add Distribution
            </Button>
          ) : (
            <div className={styles.actionForm}>
              <div className={styles.actionFormGrid}>
                <Input
                  label="Recipient ID"
                  value={distributionForm.recipientId}
                  onChange={(e) => setDistributionForm(f => ({ ...f, recipientId: e.target.value }))}
                  placeholder="User ID of the recipient"
                  required
                />
                <Input
                  label="Compliance Deadline"
                  type="date"
                  value={distributionForm.complianceDeadline}
                  onChange={(e) => setDistributionForm(f => ({ ...f, complianceDeadline: e.target.value }))}
                  required
                />
              </div>
              <div className={styles.inlineFormActions}>
                <Button size="sm"
                  onClick={() => addDistributionMutation.mutate(targetVersionId)}
                  disabled={!distributionForm.recipientId.trim() || !distributionForm.complianceDeadline || addDistributionMutation.isPending}>
                  {addDistributionMutation.isPending ? 'Adding...' : 'Add Distribution'}
                </Button>
                <Button size="sm" variant="ghost" onClick={() => setShowAddDistribution(false)}>Cancel</Button>
              </div>
            </div>
          )}
        </div>
      )}

      {!releasedVersion && (
        <p className={styles.emptyText}>
          No released versions available. Release a version to begin distribution.
        </p>
      )}

      {releasedVersion && (
        <p className={styles.emptyText}>
          Distribution tracking for v{releasedVersion.versionNumber}.
          Add recipients to distribute this document.
        </p>
      )}
    </div>
  )
}
