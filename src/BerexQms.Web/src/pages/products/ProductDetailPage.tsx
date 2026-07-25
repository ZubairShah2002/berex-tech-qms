import { useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Edit, Archive, Plus } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Badge, StatusBadge } from '@/components/ui/Badge'
import styles from './ProductDetailPage.module.css'

interface SpecParam {
  id: string
  name: string
  type: string
  unit: string | null
  nominalValue: number | null
  upperTolerance: number | null
  lowerTolerance: number | null
  textValue: string | null
  isCritical: boolean
  sortOrder: number
}

interface Revision {
  id: string
  revisionCode: string
  status: string
  description: string | null
  changeReason: string | null
  releasedAt: string | null
  releasedBy: string | null
  obsoletedAt: string | null
  specificationParameters: SpecParam[]
  createdAt: string
}

interface BomRef {
  id: string
  childPartId: string
  childPartNumber: string
  childPartName: string
  quantity: number
  referenceDesignator: string | null
  sortOrder: number
}

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
  revisions: Revision[]
  bomReferences: BomRef[]
  createdAt: string
  createdBy: string
  modifiedAt: string | null
}

type Tab = 'revisions' | 'bom'

export function ProductDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('revisions')
  const [expandedRevision, setExpandedRevision] = useState<string | null>(null)
  const [showRevisionForm, setShowRevisionForm] = useState(false)
  const [revisionForm, setRevisionForm] = useState({ revisionCode: '', description: '', changeReason: '' })
  const [formError, setFormError] = useState('')
  const [actionError, setActionError] = useState('')

  const { data: part, isLoading } = useQuery<PartDetail>({
    queryKey: ['part', id],
    queryFn: async () => {
      const res = await apiClient.get(`/parts/${id}`)
      return res.data
    },
    enabled: Boolean(id),
  })

  const obsoleteMutation = useMutation({
    mutationFn: () => apiClient.post(`/parts/${id}/obsolete`),
    onSuccess: () => {
      setActionError('')
      queryClient.invalidateQueries({ queryKey: ['part', id] })
      queryClient.invalidateQueries({ queryKey: ['parts'] })
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setActionError(axiosErr.response?.data?.detail ?? axiosErr.response?.data?.error ?? 'Failed to obsolete part.')
    },
  })

  const createRevisionMutation = useMutation({
    mutationFn: async () => {
      const res = await apiClient.post(`/parts/${id}/revisions`, {
        revisionCode: revisionForm.revisionCode,
        description: revisionForm.description || null,
        changeReason: revisionForm.changeReason || null,
      })
      return res.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['part', id] })
      setShowRevisionForm(false)
      setRevisionForm({ revisionCode: '', description: '', changeReason: '' })
      setFormError('')
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setFormError(axiosErr.response?.data?.detail ?? axiosErr.response?.data?.error ?? 'Failed to create revision.')
    },
  })

  const releaseRevisionMutation = useMutation({
    mutationFn: (revisionId: string) =>
      apiClient.post(`/parts/${id}/revisions/${revisionId}/release`),
    onSuccess: () => {
      setActionError('')
      queryClient.invalidateQueries({ queryKey: ['part', id] })
      queryClient.invalidateQueries({ queryKey: ['parts'] })
    },
    onError: (err: unknown) => {
      const axiosErr = err as { response?: { data?: { detail?: string; error?: string } } }
      setActionError(axiosErr.response?.data?.detail ?? axiosErr.response?.data?.error ?? 'Failed to release revision.')
    },
  })

  if (isLoading || !part) {
    return <div className={styles.page}>Loading...</div>
  }

  const currentRevision = part.revisions.find((r) => r.status === 'Released')

  return (
    <div className={styles.page}>
      <button className={styles.backLink} onClick={() => navigate('/products')}>
        <ArrowLeft size={16} />
        Back to Product Catalog
      </button>

      {actionError && (
        <div style={{ padding: 'var(--spacing-3) var(--spacing-4)', background: 'var(--color-error-bg)', color: 'var(--color-error)', border: '1px solid var(--color-error-border)', borderRadius: 'var(--radius-md)', fontSize: 'var(--font-size-sm)' }}>
          {actionError}
        </div>
      )}

      <div className={styles.header}>
        <div className={styles.headerInfo}>
          <div className={styles.titleRow}>
            <h1 className={styles.title}>{part.partNumber}</h1>
            <StatusBadge status={part.status} />
          </div>
          <p className={styles.name}>{part.name}</p>
          {part.description && <p className={styles.description}>{part.description}</p>}
        </div>
        <div className={styles.headerActions}>
          {part.status !== 'Obsolete' && (
            <>
              <Button
                variant="secondary"
                icon={<Edit size={14} />}
                size="sm"
                onClick={() => navigate(`/products/${id}/edit`)}
              >
                Edit
              </Button>
              <Button
                variant="danger"
                icon={<Archive size={14} />}
                size="sm"
                onClick={() => {
                  if (window.confirm('Are you sure you want to obsolete this part? This action cannot be undone.')) {
                    obsoleteMutation.mutate()
                  }
                }}
              >
                Obsolete
              </Button>
            </>
          )}
        </div>
      </div>

      <div className={styles.meta}>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Product Family</span>
          <span className={styles.metaValue}>{part.productFamily ?? '—'}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Category</span>
          <span className={styles.metaValue}>{part.category ?? '—'}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Serialization</span>
          <span className={styles.metaValue}>{part.serializationMode}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>UOM</span>
          <span className={styles.metaValue}>{part.unitOfMeasure ?? '—'}</span>
        </div>
        <div className={styles.metaItem}>
          <span className={styles.metaLabel}>Current Revision</span>
          <span className={styles.metaValue}>{currentRevision?.revisionCode ?? '—'}</span>
        </div>
      </div>

      <div className={styles.tabs}>
        <button
          className={`${styles.tab} ${tab === 'revisions' ? styles.tabActive : ''}`}
          onClick={() => setTab('revisions')}
        >
          Revisions ({part.revisions.length})
        </button>
        <button
          className={`${styles.tab} ${tab === 'bom' ? styles.tabActive : ''}`}
          onClick={() => setTab('bom')}
        >
          BOM ({part.bomReferences.length})
        </button>
      </div>

      {tab === 'revisions' && (
        <div className={styles.tabContent}>
          {part.status !== 'Obsolete' && (
            <div className={styles.tabHeader}>
              {!showRevisionForm ? (
                <Button
                  size="sm"
                  icon={<Plus size={14} />}
                  onClick={() => setShowRevisionForm(true)}
                >
                  New Revision
                </Button>
              ) : (
                <div className={styles.inlineForm}>
                  {formError && <div className={styles.formError}>{formError}</div>}
                  <div className={styles.inlineFormFields}>
                    <input
                      className={styles.inlineInput}
                      placeholder="Rev. code (e.g. A, B, 1.0)"
                      value={revisionForm.revisionCode}
                      onChange={(e) => setRevisionForm((f) => ({ ...f, revisionCode: e.target.value }))}
                    />
                    <input
                      className={styles.inlineInput}
                      placeholder="Description (optional)"
                      value={revisionForm.description}
                      onChange={(e) => setRevisionForm((f) => ({ ...f, description: e.target.value }))}
                    />
                    <input
                      className={styles.inlineInput}
                      placeholder="Change reason (optional)"
                      value={revisionForm.changeReason}
                      onChange={(e) => setRevisionForm((f) => ({ ...f, changeReason: e.target.value }))}
                    />
                  </div>
                  <div className={styles.inlineFormActions}>
                    <Button size="sm" onClick={() => createRevisionMutation.mutate()}
                      disabled={!revisionForm.revisionCode || createRevisionMutation.isPending}>
                      {createRevisionMutation.isPending ? 'Creating...' : 'Create'}
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => {
                      setShowRevisionForm(false)
                      setFormError('')
                    }}>
                      Cancel
                    </Button>
                  </div>
                </div>
              )}
            </div>
          )}

          {part.revisions.length === 0 ? (
            <p className={styles.emptyText}>No revisions yet. Create one to define specification parameters.</p>
          ) : (
            <div className={styles.revisionList}>
              {part.revisions.map((rev) => (
                <div key={rev.id} className={styles.revisionCard}>
                  <div className={styles.revisionHeader}
                    onClick={() => setExpandedRevision(expandedRevision === rev.id ? null : rev.id)}>
                    <div className={styles.revisionInfo}>
                      <span className={styles.revisionCode}>{rev.revisionCode}</span>
                      <StatusBadge status={rev.status} />
                      {rev.specificationParameters.some((p) => p.isCritical) && (
                        <Badge variant="warning">CTQ</Badge>
                      )}
                    </div>
                    <div className={styles.revisionActions}>
                      {rev.status === 'Draft' && (
                        <Button
                          size="sm"
                          variant="secondary"
                          onClick={(e) => {
                            e.stopPropagation()
                            if (window.confirm('Release this revision? Any currently released revision will be obsoleted.')) {
                              releaseRevisionMutation.mutate(rev.id)
                            }
                          }}
                        >
                          Release
                        </Button>
                      )}
                      <span className={styles.revisionDate}>
                        {new Date(rev.createdAt).toLocaleDateString()}
                      </span>
                    </div>
                  </div>

                  {expandedRevision === rev.id && (
                    <div className={styles.revisionBody}>
                      {rev.description && (
                        <p className={styles.revisionDescription}>{rev.description}</p>
                      )}
                      {rev.changeReason && (
                        <p className={styles.revisionChange}>Change reason: {rev.changeReason}</p>
                      )}
                      {rev.releasedAt && (
                        <p className={styles.revisionMeta}>
                          Released {new Date(rev.releasedAt).toLocaleDateString()} by {rev.releasedBy}
                        </p>
                      )}

                      <div className={styles.specSection}>
                        <h4 className={styles.specTitle}>
                          Specification Parameters ({rev.specificationParameters.length})
                        </h4>
                        {rev.specificationParameters.length === 0 ? (
                          <p className={styles.emptyText}>No specification parameters defined.</p>
                        ) : (
                          <div className={styles.specTableWrapper}>
                            <table className={styles.specTable}>
                              <thead>
                                <tr>
                                  <th>Name</th>
                                  <th>Type</th>
                                  <th>Nominal</th>
                                  <th>Tolerance</th>
                                  <th>Unit</th>
                                  <th>Critical</th>
                                </tr>
                              </thead>
                              <tbody>
                                {rev.specificationParameters.map((sp) => (
                                  <tr key={sp.id}>
                                    <td>{sp.name}</td>
                                    <td>{sp.type}</td>
                                    <td>{sp.nominalValue ?? sp.textValue ?? '—'}</td>
                                    <td>
                                      {sp.lowerTolerance != null && sp.upperTolerance != null
                                        ? `${sp.lowerTolerance} / ${sp.upperTolerance}`
                                        : '—'}
                                    </td>
                                    <td>{sp.unit ?? '—'}</td>
                                    <td>{sp.isCritical ? 'Yes' : 'No'}</td>
                                  </tr>
                                ))}
                              </tbody>
                            </table>
                          </div>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {tab === 'bom' && (
        <div className={styles.tabContent}>
          {part.bomReferences.length === 0 ? (
            <p className={styles.emptyText}>No BOM references defined for this part.</p>
          ) : (
            <div className={styles.specTableWrapper}>
              <table className={styles.specTable}>
                <thead>
                  <tr>
                    <th>Child Part Number</th>
                    <th>Name</th>
                    <th>Quantity</th>
                    <th>Ref. Designator</th>
                  </tr>
                </thead>
                <tbody>
                  {part.bomReferences.map((bom) => (
                    <tr key={bom.id}>
                      <td>
                        <button
                          className={styles.linkButton}
                          onClick={() => navigate(`/products/${bom.childPartId}`)}
                        >
                          {bom.childPartNumber}
                        </button>
                      </td>
                      <td>{bom.childPartName}</td>
                      <td>{bom.quantity}</td>
                      <td>{bom.referenceDesignator ?? '—'}</td>
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
