import { useState, useCallback } from 'react'
import { Brain, Activity, ToggleLeft } from 'lucide-react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { Select } from '@/components/ui/Select'
import { StatusBadge } from '@/components/ui/Badge'
import styles from './AiDashboardPage.module.css'

// ---- Types ------------------------------------------------------------------

interface AiCapabilityConfigDto {
  id: string
  capability: string
  isEnabled: boolean
  lowConfidenceThreshold: number
  moderateConfidenceThreshold: number
  highConfidenceThreshold: number
}

interface AiInteractionDto {
  id: string
  capability: string
  userId: string
  modelId: string | null
  outputSummary: string | null
  confidenceScore: number | null
  confidenceLevel: string | null
  status: string
  userAction: string | null
  requestedAt: string
  completedAt: string | null
  responseTimeMs: number | null
}

interface AiModelDto {
  id: string
  name: string
  version: string
  capability: string
  status: string
  description: string | null
  trainingSampleCount: number | null
  trainedAt: string | null
  promotedAt: string | null
  createdAt: string
}

interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

// ---- Constants --------------------------------------------------------------

type TabId = 'capabilities' | 'interactions' | 'models'

const capabilityLabels: Record<string, string> = {
  DefectPrediction: 'Defect Prediction',
  AnomalyDetection: 'Anomaly Detection',
  RootCauseSuggestion: 'Root Cause Suggestion',
  DocumentClassification: 'Document Classification',
  SupplierRiskScoring: 'Supplier Risk Scoring',
  InspectionOptimization: 'Inspection Optimization',
}

const capabilityDescriptions: Record<string, string> = {
  DefectPrediction: 'Predict probability of defect occurrence based on process parameters.',
  AnomalyDetection: 'Identify unusual patterns in quality data streams.',
  RootCauseSuggestion: 'Suggest probable root causes for non-conformances.',
  DocumentClassification: 'Auto-classify and tag documents based on content analysis.',
  SupplierRiskScoring: 'Predict supplier quality risk based on performance data.',
  InspectionOptimization: 'Recommend sampling plan adjustments based on quality history.',
}

const capabilityOptions = [
  { value: '', label: 'All capabilities' },
  { value: 'DefectPrediction', label: 'Defect Prediction' },
  { value: 'AnomalyDetection', label: 'Anomaly Detection' },
  { value: 'RootCauseSuggestion', label: 'Root Cause Suggestion' },
  { value: 'DocumentClassification', label: 'Document Classification' },
  { value: 'SupplierRiskScoring', label: 'Supplier Risk Scoring' },
  { value: 'InspectionOptimization', label: 'Inspection Optimization' },
]

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Pending', label: 'Pending' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Failed', label: 'Failed' },
  { value: 'TimedOut', label: 'Timed Out' },
]

const modelStatusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Training', label: 'Training' },
  { value: 'Validating', label: 'Validating' },
  { value: 'Shadow', label: 'Shadow' },
  { value: 'Active', label: 'Active' },
  { value: 'Deprecated', label: 'Deprecated' },
  { value: 'Retired', label: 'Retired' },
]

// ---- Component --------------------------------------------------------------

export function AiDashboardPage() {
  const [activeTab, setActiveTab] = useState<TabId>('capabilities')
  const [capFilter, setCapFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [modelCapFilter, setModelCapFilter] = useState('')
  const [modelStatusFilter, setModelStatusFilter] = useState('')
  const [interactionPage, setInteractionPage] = useState(1)
  const [modelPage, setModelPage] = useState(1)
  const pageSize = 20
  const queryClient = useQueryClient()

  // ---- Queries ----

  const configsQuery = useQuery({
    queryKey: ['ai', 'configs'],
    queryFn: () => apiClient.get<AiCapabilityConfigDto[]>('/api/v1/ai/capabilities').then(r => r.data),
  })

  const interactionsQuery = useQuery({
    queryKey: ['ai', 'interactions', capFilter, statusFilter, interactionPage],
    queryFn: () => {
      const params = new URLSearchParams()
      params.set('page', String(interactionPage))
      params.set('pageSize', String(pageSize))
      if (capFilter) params.set('capability', capFilter)
      if (statusFilter) params.set('status', statusFilter)
      return apiClient.get<PagedResult<AiInteractionDto>>(`/api/v1/ai/interactions?${params}`).then(r => r.data)
    },
    enabled: activeTab === 'interactions',
  })

  const modelsQuery = useQuery({
    queryKey: ['ai', 'models', modelCapFilter, modelStatusFilter, modelPage],
    queryFn: () => {
      const params = new URLSearchParams()
      params.set('page', String(modelPage))
      params.set('pageSize', String(pageSize))
      if (modelCapFilter) params.set('capability', modelCapFilter)
      if (modelStatusFilter) params.set('status', modelStatusFilter)
      return apiClient.get<PagedResult<AiModelDto>>(`/api/v1/ai/models?${params}`).then(r => r.data)
    },
    enabled: activeTab === 'models',
  })

  // ---- Mutations ----

  const toggleMutation = useMutation({
    mutationFn: (data: { capability: string; enable: boolean }) =>
      apiClient.post('/api/v1/ai/capabilities/toggle', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['ai', 'configs'] }),
  })

  const handleToggle = useCallback((capability: string, currentEnabled: boolean) => {
    toggleMutation.mutate({ capability, enable: !currentEnabled })
  }, [toggleMutation])

  // ---- Render helpers ----

  function renderConfidenceLevel(level: string | null, score: number | null) {
    if (level === null || score === null) return '—'
    const formatted = (score * 100).toFixed(1) + '%'
    const levelClass = level === 'Low' ? styles.confidenceLow
      : level === 'Moderate' ? styles.confidenceModerate
      : level === 'High' ? styles.confidenceHigh
      : styles.confidenceVeryHigh
    return <span className={`${styles.confidenceIndicator} ${levelClass}`}>{formatted} ({level})</span>
  }

  function formatDate(iso: string | null) {
    if (!iso) return '—'
    return new Date(iso).toLocaleString()
  }

  // ---- Capabilities tab ----

  function renderCapabilities() {
    if (configsQuery.isLoading) return <div className={styles.loadingSkeleton} />
    if (configsQuery.isError) return <div className={styles.errorBanner}>Failed to load AI capability configurations.</div>

    const configs = configsQuery.data ?? []

    // Build a map of existing configs
    const configMap = new Map(configs.map(c => [c.capability, c]))

    // Show all 6 capabilities, using config if exists or defaults
    const allCapabilities = Object.keys(capabilityLabels)

    return (
      <div className={styles.section}>
        <h3 className={styles.sectionTitle}>AI Capability Configuration</h3>
        <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
          AI assists, it never decides. All capabilities require explicit human confirmation before any action is taken.
        </p>
        <div className={styles.capabilitiesGrid}>
          {allCapabilities.map(cap => {
            const config = configMap.get(cap)
            const isEnabled = config?.isEnabled ?? false
            return (
              <div key={cap} className={styles.capabilityCard}>
                <div className={styles.capabilityHeader}>
                  <h4 className={styles.capabilityName}>{capabilityLabels[cap]}</h4>
                  <button
                    className={`${styles.toggle} ${isEnabled ? styles.toggleEnabled : ''}`}
                    onClick={() => handleToggle(cap, isEnabled)}
                    disabled={toggleMutation.isPending}
                    title={isEnabled ? 'Disable capability' : 'Enable capability'}
                  />
                </div>
                <p className={styles.capabilityDescription}>{capabilityDescriptions[cap]}</p>
                {config && (
                  <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--spacing-1)' }}>
                    <div className={styles.thresholdBar}>
                      <span className={styles.thresholdLabel}>Low ≤</span>
                      <span className={styles.thresholdValue}>{(config.lowConfidenceThreshold * 100).toFixed(0)}%</span>
                    </div>
                    <div className={styles.thresholdBar}>
                      <span className={styles.thresholdLabel}>Moderate ≤</span>
                      <span className={styles.thresholdValue}>{(config.moderateConfidenceThreshold * 100).toFixed(0)}%</span>
                    </div>
                    <div className={styles.thresholdBar}>
                      <span className={styles.thresholdLabel}>High ≤</span>
                      <span className={styles.thresholdValue}>{(config.highConfidenceThreshold * 100).toFixed(0)}%</span>
                    </div>
                  </div>
                )}
                <StatusBadge status={isEnabled ? 'Active' : 'Inactive'} />
              </div>
            )
          })}
        </div>
      </div>
    )
  }

  // ---- Interactions tab ----

  function renderInteractions() {
    if (interactionsQuery.isLoading) return <div className={styles.loadingSkeleton} />
    if (interactionsQuery.isError) return <div className={styles.errorBanner}>Failed to load AI interactions.</div>

    const data = interactionsQuery.data
    const items = data?.items ?? []

    const columns = [
      { key: 'capability', header: 'Capability', render: (row: AiInteractionDto) => capabilityLabels[row.capability] ?? row.capability },
      { key: 'status', header: 'Status', render: (row: AiInteractionDto) => <StatusBadge status={row.status} /> },
      { key: 'confidenceScore', header: 'Confidence', render: (row: AiInteractionDto) => renderConfidenceLevel(row.confidenceLevel, row.confidenceScore) },
      { key: 'userAction', header: 'User Action', render: (row: AiInteractionDto) => row.userAction ? <StatusBadge status={row.userAction} /> : '—' },
      { key: 'responseTimeMs', header: 'Response', render: (row: AiInteractionDto) => row.responseTimeMs !== null ? `${row.responseTimeMs}ms` : '—' },
      { key: 'requestedAt', header: 'Requested', render: (row: AiInteractionDto) => formatDate(row.requestedAt) },
    ]

    return (
      <div className={styles.section}>
        <h3 className={styles.sectionTitle}>AI Interaction Audit Trail</h3>
        <div className={styles.filters}>
          <div className={styles.filterSelect}>
            <Select
              label=""
              value={capFilter}
              onChange={(e) => { setCapFilter(e.target.value); setInteractionPage(1) }}
              options={capabilityOptions}
            />
          </div>
          <div className={styles.filterSelect}>
            <Select
              label=""
              value={statusFilter}
              onChange={(e) => { setStatusFilter(e.target.value); setInteractionPage(1) }}
              options={statusOptions}
            />
          </div>
        </div>
        {items.length === 0 ? (
          <div className={styles.emptyState}>
            <Activity size={48} className={styles.emptyIcon} />
            <p>No AI interactions recorded yet.</p>
          </div>
        ) : (
          <DataTable
            data={items as unknown as Record<string, unknown>[]}
            columns={columns as never}
            keyExtractor={(row) => (row as unknown as AiInteractionDto).id}
            page={data?.page ?? 1}
            totalCount={data?.totalCount ?? 0}
            pageSize={pageSize}
            onPageChange={setInteractionPage}
          />
        )}
      </div>
    )
  }

  // ---- Models tab ----

  function renderModels() {
    if (modelsQuery.isLoading) return <div className={styles.loadingSkeleton} />
    if (modelsQuery.isError) return <div className={styles.errorBanner}>Failed to load AI models.</div>

    const data = modelsQuery.data
    const items = data?.items ?? []

    return (
      <div className={styles.section}>
        <h3 className={styles.sectionTitle}>Model Registry</h3>
        <div className={styles.filters}>
          <div className={styles.filterSelect}>
            <Select
              label=""
              value={modelCapFilter}
              onChange={(e) => { setModelCapFilter(e.target.value); setModelPage(1) }}
              options={capabilityOptions}
            />
          </div>
          <div className={styles.filterSelect}>
            <Select
              label=""
              value={modelStatusFilter}
              onChange={(e) => { setModelStatusFilter(e.target.value); setModelPage(1) }}
              options={modelStatusOptions}
            />
          </div>
        </div>
        {items.length === 0 ? (
          <div className={styles.emptyState}>
            <Brain size={48} className={styles.emptyIcon} />
            <p>No AI models registered yet.</p>
          </div>
        ) : (
          <div className={styles.modelGrid}>
            {items.map(model => (
              <div key={model.id} className={styles.modelCard}>
                <h4 className={styles.modelName}>{model.name}</h4>
                <div className={styles.modelMeta}>
                  <span>v{model.version}</span>
                  <span>{capabilityLabels[model.capability] ?? model.capability}</span>
                </div>
                <StatusBadge status={model.status} />
                {model.description && (
                  <p className={styles.capabilityDescription}>{model.description}</p>
                )}
                <div className={styles.modelMeta}>
                  {model.trainingSampleCount !== null && <span>{model.trainingSampleCount.toLocaleString()} samples</span>}
                  {model.trainedAt && <span>Trained {formatDate(model.trainedAt)}</span>}
                </div>
              </div>
            ))}
          </div>
        )}
        {(data?.totalCount ?? 0) > pageSize && (
          <div style={{ display: 'flex', justifyContent: 'center', gap: 'var(--spacing-2)', marginTop: 'var(--spacing-4)' }}>
            <Button variant="secondary" size="sm" disabled={modelPage <= 1} onClick={() => setModelPage(p => p - 1)}>Previous</Button>
            <span style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', padding: 'var(--spacing-2)' }}>
              Page {modelPage} of {Math.ceil((data?.totalCount ?? 0) / pageSize)}
            </span>
            <Button variant="secondary" size="sm" disabled={modelPage >= Math.ceil((data?.totalCount ?? 0) / pageSize)} onClick={() => setModelPage(p => p + 1)}>Next</Button>
          </div>
        )}
      </div>
    )
  }

  // ---- Main render ----

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div className={styles.headerLeft}>
          <h1 className={styles.title}>AI Engine</h1>
          <p className={styles.subtitle}>AI-assisted quality intelligence — all suggestions require human confirmation</p>
        </div>
      </div>

      <div className={styles.tabs}>
        <button className={`${styles.tab} ${activeTab === 'capabilities' ? styles.tabActive : ''}`} onClick={() => setActiveTab('capabilities')}>
          <ToggleLeft size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          Capabilities
        </button>
        <button className={`${styles.tab} ${activeTab === 'interactions' ? styles.tabActive : ''}`} onClick={() => setActiveTab('interactions')}>
          <Activity size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          Interactions
        </button>
        <button className={`${styles.tab} ${activeTab === 'models' ? styles.tabActive : ''}`} onClick={() => setActiveTab('models')}>
          <Brain size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          Models
        </button>
      </div>

      {activeTab === 'capabilities' && renderCapabilities()}
      {activeTab === 'interactions' && renderInteractions()}
      {activeTab === 'models' && renderModels()}
    </div>
  )
}
