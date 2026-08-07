import { useState, useCallback } from 'react'
import { Brain, Activity, ToggleLeft, Shield, ClipboardList, Workflow, AlertTriangle, Database, Search } from 'lucide-react'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { StatusBadge } from '@/components/ui/Badge'
import { useAuthStore } from '@/stores/auth-store'
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

interface AiUserPermissionsDto {
  userId: string
  permissionLevel: string
  permissionLevelNumber: number
  allowedActionTypes: string[]
  allowedCategories: string[]
  hasExplicitPolicy: boolean
}

interface AiActionLogDto {
  id: string
  userId: string
  userRole: string
  permissionLevel: string
  actionType: string
  actionCategory: string
  prompt: string | null
  reasoningSummary: string | null
  affectedModules: string | null
  affectedRecords: string | null
  riskLevel: string
  confirmationStatus: string
  requiresConfirmation: boolean
  executionResult: string
  errorDetail: string | null
  requestedAt: string
  completedAt: string | null
  durationMs: number | null
  modelVersion: string | null
  confidenceScore: number | null
  isRollbackPossible: boolean
}

interface AiWorkflowDefinitionDto {
  id: string
  name: string
  description: string | null
  minimumPermissionLevel: string
  category: string
  isActive: boolean
  affectedModules: string
  createdAt: string
}

interface AiWorkflowExecutionDto {
  id: string
  workflowDefinitionId: string
  workflowName: string
  userId: string
  status: string
  totalSteps: number
  completedSteps: number
  failedSteps: number
  output: string | null
  startedAt: string
  completedAt: string | null
  totalDurationMs: number | null
  errorSummary: string | null
}

interface AiConfirmationRequestDto {
  actionLogId: string
  actionType: string
  actionCategory: string
  riskLevel: string
  actionSummary: string
  affectedRecords: string | null
  isRollbackPossible: boolean
  confirmationPrompt: string
}

interface ContextStatsDto {
  totalDocuments: number
  indexedDocuments: number
  pendingDocuments: number
  failedDocuments: number
  staleDocuments: number
  activeSources: number
  totalSources: number
  lastSyncedAt: string | null
}

interface ContextSearchResultDto {
  documentId: string
  sourceModule: string
  contextType: string
  title: string
  contentSnippet: string
  relevanceScore: number
  indexedAt: string | null
}

interface KnowledgeSourceDto {
  id: string
  name: string
  module: string
  description: string | null
  isActive: boolean
  lastSyncedAt: string | null
  documentCount: number
  createdAt: string
}


// ---- Constants --------------------------------------------------------------

type TabId = 'capabilities' | 'interactions' | 'models' | 'permissions' | 'actionLog' | 'workflows' | 'knowledgeContext'

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

const permissionLevelLabels: Record<string, string> = {
  Assistant: 'Level 1 — Assistant',
  Manager: 'Level 2 — Manager',
  Administrator: 'Level 3 — Administrator',
  SuperAdministrator: 'Level 4 — Super Administrator',
}

const permissionLevelOptions = [
  { value: 'Assistant', label: 'Level 1 — Assistant' },
  { value: 'Manager', label: 'Level 2 — Manager' },
  { value: 'Administrator', label: 'Level 3 — Administrator' },
  { value: 'SuperAdministrator', label: 'Level 4 — Super Administrator' },
]

const actionLogResultOptions = [
  { value: '', label: 'All results' },
  { value: 'Success', label: 'Success' },
  { value: 'Failed', label: 'Failed' },
  { value: 'AwaitingConfirmation', label: 'Awaiting Confirmation' },
  { value: 'Rejected', label: 'Rejected' },
  { value: 'Expired', label: 'Expired' },
]

const actionLogPermLevelOptions = [
  { value: '', label: 'All levels' },
  { value: 'Assistant', label: 'Assistant' },
  { value: 'Manager', label: 'Manager' },
  { value: 'Administrator', label: 'Administrator' },
  { value: 'SuperAdministrator', label: 'Super Admin' },
]

const workflowStatusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'PendingConfirmation', label: 'Pending Confirmation' },
  { value: 'Running', label: 'Running' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Failed', label: 'Failed' },
  { value: 'Cancelled', label: 'Cancelled' },
]

const contextModuleOptions = [
  { value: '', label: 'All modules' },
  { value: 'ProductCatalog', label: 'Product Catalog' },
  { value: 'Inspection', label: 'Inspection' },
  { value: 'NonConformance', label: 'Non-Conformance' },
  { value: 'Capa', label: 'CAPA' },
  { value: 'DocumentControl', label: 'Document Control' },
  { value: 'AuditManagement', label: 'Audit Management' },
  { value: 'SupplierQuality', label: 'Supplier Quality' },
  { value: 'Calibration', label: 'Calibration' },
  { value: 'Training', label: 'Training' },
  { value: 'Spc', label: 'SPC' },
]

const riskColors: Record<string, string> = {
  None: 'var(--color-text-secondary)',
  Low: 'var(--color-success)',
  Medium: 'var(--color-warning)',
  High: 'var(--color-error)',
  Critical: 'var(--color-error)',
}

const permissionLevelDescriptions: Record<string, string> = {
  Assistant: 'Read-only AI access. View predictions, suggestions, and reports.',
  Manager: 'Generate content and draft workflows. Can request AI-powered analysis.',
  Administrator: 'Full write access to AI actions. Can execute cross-module operations.',
  SuperAdministrator: 'Unrestricted access including dangerous operations. JARVIS MODE.',
}

// ---- Component --------------------------------------------------------------

export function AiDashboardPage() {
  const user = useAuthStore(s => s.user)
  const [activeTab, setActiveTab] = useState<TabId>('capabilities')
  const [capFilter, setCapFilter] = useState('')
  const [statusFilter, setStatusFilter] = useState('')
  const [modelCapFilter, setModelCapFilter] = useState('')
  const [modelStatusFilter, setModelStatusFilter] = useState('')
  const [interactionPage, setInteractionPage] = useState(1)
  const [modelPage, setModelPage] = useState(1)

  // Action log filters
  const [logResultFilter, setLogResultFilter] = useState('')
  const [logLevelFilter, setLogLevelFilter] = useState('')
  const [logPage, setLogPage] = useState(1)

  // Workflow filters
  const [wfStatusFilter, setWfStatusFilter] = useState('')
  const [wfPage, setWfPage] = useState(1)

  // Knowledge context
  const [contextSearchTerm, setContextSearchTerm] = useState('')
  const [contextModuleFilter, setContextModuleFilter] = useState('')
  const [searchSubmitted, setSearchSubmitted] = useState('')

  // Confirmation dialog
  const [confirmationRequest, setConfirmationRequest] = useState<AiConfirmationRequestDto | null>(null)

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

  const myPermissionsQuery = useQuery({
    queryKey: ['ai', 'permissions', user?.id],
    queryFn: () => apiClient.get<AiUserPermissionsDto>(`/api/v1/ai/permissions/${user!.id}`).then(r => r.data),
    enabled: activeTab === 'permissions' && !!user?.id,
  })

  const actionLogsQuery = useQuery({
    queryKey: ['ai', 'actionLogs', logResultFilter, logLevelFilter, logPage],
    queryFn: () => {
      const params = new URLSearchParams()
      params.set('page', String(logPage))
      params.set('pageSize', String(pageSize))
      if (logResultFilter) params.set('executionResult', logResultFilter)
      if (logLevelFilter) params.set('permissionLevel', logLevelFilter)
      return apiClient.get<PagedResult<AiActionLogDto>>(`/api/v1/ai/actions/logs?${params}`).then(r => r.data)
    },
    enabled: activeTab === 'actionLog',
  })

  const workflowDefinitionsQuery = useQuery({
    queryKey: ['ai', 'workflowDefinitions'],
    queryFn: () => apiClient.get<AiWorkflowDefinitionDto[]>('/api/v1/ai/workflows/definitions').then(r => r.data),
    enabled: activeTab === 'workflows',
  })

  const workflowExecutionsQuery = useQuery({
    queryKey: ['ai', 'workflowExecutions', wfStatusFilter, wfPage],
    queryFn: () => {
      const params = new URLSearchParams()
      params.set('page', String(wfPage))
      params.set('pageSize', String(pageSize))
      if (wfStatusFilter) params.set('status', wfStatusFilter)
      return apiClient.get<PagedResult<AiWorkflowExecutionDto>>(`/api/v1/ai/workflows/executions?${params}`).then(r => r.data)
    },
    enabled: activeTab === 'workflows',
  })

  const contextStatsQuery = useQuery({
    queryKey: ['ai', 'contextStats'],
    queryFn: () => apiClient.get<ContextStatsDto>('/api/v1/ai/context/stats').then(r => r.data),
    enabled: activeTab === 'knowledgeContext',
  })

  const knowledgeSourcesQuery = useQuery({
    queryKey: ['ai', 'knowledgeSources'],
    queryFn: () => apiClient.get<KnowledgeSourceDto[]>('/api/v1/ai/knowledge-sources').then(r => r.data),
    enabled: activeTab === 'knowledgeContext',
  })

  const contextSearchQuery = useQuery({
    queryKey: ['ai', 'contextSearch', searchSubmitted, contextModuleFilter],
    queryFn: () => {
      const params = new URLSearchParams()
      params.set('searchTerm', searchSubmitted)
      if (contextModuleFilter) params.set('sourceModule', contextModuleFilter)
      params.set('maxResults', '20')
      return apiClient.get<ContextSearchResultDto[]>(`/api/v1/ai/context/search?${params}`).then(r => r.data)
    },
    enabled: activeTab === 'knowledgeContext' && searchSubmitted.length >= 2,
  })

  // ---- Mutations ----

  const toggleMutation = useMutation({
    mutationFn: (data: { capability: string; enable: boolean }) =>
      apiClient.post('/api/v1/ai/capabilities/toggle', data),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['ai', 'configs'] }),
  })

  const executeWorkflowMutation = useMutation({
    mutationFn: (workflowDefinitionId: string) =>
      apiClient.post<AiWorkflowExecutionDto>('/api/v1/ai/workflows/execute', { workflowDefinitionId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['ai', 'workflowExecutions'] }),
  })

  const confirmWorkflowMutation = useMutation({
    mutationFn: (data: { executionId: string; confirm: boolean }) =>
      apiClient.post<AiWorkflowExecutionDto>(`/api/v1/ai/workflows/executions/${data.executionId}/confirm`, { confirm: data.confirm }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ['ai', 'workflowExecutions'] }),
  })

  const confirmActionMutation = useMutation({
    mutationFn: (data: { actionLogId: string; confirm: boolean }) =>
      apiClient.post<AiActionLogDto>(`/api/v1/ai/actions/${data.actionLogId}/confirm`, { confirm: data.confirm }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ai', 'actionLogs'] })
      setConfirmationRequest(null)
    },
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

  // ---- Permissions tab ----

  function renderPermissions() {
    if (myPermissionsQuery.isLoading) return <div className={styles.loadingSkeleton} />
    if (myPermissionsQuery.isError) return <div className={styles.errorBanner}>Failed to load AI permissions.</div>

    const perms = myPermissionsQuery.data

    return (
      <div className={styles.section}>
        <h3 className={styles.sectionTitle}>AI Permission Summary</h3>
        <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
          Your current AI permission level and authorized actions within this tenant.
        </p>

        {perms && (
          <>
            {/* Current Level Card */}
            <div className={styles.permissionLevelCard}>
              <div className={styles.permissionLevelHeader}>
                <Shield size={20} />
                <span className={styles.permissionLevelTitle}>
                  {permissionLevelLabels[perms.permissionLevel] ?? perms.permissionLevel}
                </span>
                {!perms.hasExplicitPolicy && (
                  <span className={styles.defaultBadge}>Default</span>
                )}
              </div>
              <p className={styles.permissionLevelDesc}>
                {permissionLevelDescriptions[perms.permissionLevel] ?? ''}
              </p>
            </div>

            {/* Permission Level Reference */}
            <div className={styles.permissionLevelsGrid}>
              {permissionLevelOptions.map(opt => {
                const isActive = opt.value === perms.permissionLevel
                return (
                  <div
                    key={opt.value}
                    className={`${styles.permissionTierCard} ${isActive ? styles.permissionTierActive : ''}`}
                  >
                    <div className={styles.permissionTierHeader}>
                      <span className={styles.permissionTierName}>{opt.label}</span>
                      {isActive && <StatusBadge status="Active" />}
                    </div>
                    <p className={styles.capabilityDescription}>
                      {permissionLevelDescriptions[opt.value]}
                    </p>
                  </div>
                )
              })}
            </div>

            {/* Authorized Categories */}
            <div style={{ marginTop: 'var(--spacing-4)' }}>
              <h4 className={styles.subsectionTitle}>Authorized Action Categories</h4>
              <div className={styles.tagList}>
                {perms.allowedCategories.map(cat => (
                  <span key={cat} className={styles.tag}>{cat}</span>
                ))}
              </div>
            </div>
          </>
        )}
      </div>
    )
  }

  // ---- Action Log tab ----

  function renderActionLog() {
    if (actionLogsQuery.isLoading) return <div className={styles.loadingSkeleton} />
    if (actionLogsQuery.isError) return <div className={styles.errorBanner}>Failed to load AI action logs.</div>

    const data = actionLogsQuery.data
    const items = data?.items ?? []

    const columns = [
      { key: 'actionType', header: 'Action', render: (row: AiActionLogDto) => (
        <span className={styles.actionTypeLabel}>{row.actionType.replace(/([A-Z])/g, ' $1').trim()}</span>
      )},
      { key: 'actionCategory', header: 'Category', render: (row: AiActionLogDto) => (
        <span className={styles.tag}>{row.actionCategory}</span>
      )},
      { key: 'permissionLevel', header: 'Level', render: (row: AiActionLogDto) => row.permissionLevel },
      { key: 'riskLevel', header: 'Risk', render: (row: AiActionLogDto) => (
        <span style={{ color: riskColors[row.riskLevel] ?? 'inherit', fontWeight: 500 }}>{row.riskLevel}</span>
      )},
      { key: 'confirmationStatus', header: 'Confirmation', render: (row: AiActionLogDto) => (
        <StatusBadge status={row.confirmationStatus} />
      )},
      { key: 'executionResult', header: 'Result', render: (row: AiActionLogDto) => (
        <StatusBadge status={row.executionResult} />
      )},
      { key: 'durationMs', header: 'Duration', render: (row: AiActionLogDto) => row.durationMs !== null ? `${row.durationMs}ms` : '—' },
      { key: 'requestedAt', header: 'Requested', render: (row: AiActionLogDto) => formatDate(row.requestedAt) },
    ]

    return (
      <div className={styles.section}>
        <h3 className={styles.sectionTitle}>AI Action Audit Log</h3>
        <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
          Immutable record of every AI-initiated action, including confirmation status and execution outcome.
        </p>
        <div className={styles.filters}>
          <div className={styles.filterSelect}>
            <Select
              label=""
              value={logResultFilter}
              onChange={(e) => { setLogResultFilter(e.target.value); setLogPage(1) }}
              options={actionLogResultOptions}
            />
          </div>
          <div className={styles.filterSelect}>
            <Select
              label=""
              value={logLevelFilter}
              onChange={(e) => { setLogLevelFilter(e.target.value); setLogPage(1) }}
              options={actionLogPermLevelOptions}
            />
          </div>
        </div>
        {items.length === 0 ? (
          <div className={styles.emptyState}>
            <ClipboardList size={48} className={styles.emptyIcon} />
            <p>No AI actions recorded yet.</p>
          </div>
        ) : (
          <DataTable
            data={items as unknown as Record<string, unknown>[]}
            columns={columns as never}
            keyExtractor={(row) => (row as unknown as AiActionLogDto).id}
            page={data?.page ?? 1}
            totalCount={data?.totalCount ?? 0}
            pageSize={pageSize}
            onPageChange={setLogPage}
          />
        )}
      </div>
    )
  }

  // ---- Workflows tab ----

  function renderWorkflows() {
    const defsLoading = workflowDefinitionsQuery.isLoading
    const execsLoading = workflowExecutionsQuery.isLoading
    if (defsLoading && execsLoading) return <div className={styles.loadingSkeleton} />

    const definitions = workflowDefinitionsQuery.data ?? []
    const executionsData = workflowExecutionsQuery.data
    const executions = executionsData?.items ?? []

    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--spacing-6)' }}>
        {/* Workflow Definitions */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Workflow Templates</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            Pre-defined multi-step AI workflows. All executions require explicit confirmation before proceeding.
          </p>
          {definitions.length === 0 ? (
            <div className={styles.emptyState}>
              <Workflow size={48} className={styles.emptyIcon} />
              <p>No workflow templates defined yet.</p>
            </div>
          ) : (
            <div className={styles.modelGrid}>
              {definitions.map(def => (
                <div key={def.id} className={styles.workflowCard}>
                  <div className={styles.workflowCardHeader}>
                    <h4 className={styles.modelName}>{def.name}</h4>
                    <StatusBadge status={def.isActive ? 'Active' : 'Inactive'} />
                  </div>
                  {def.description && (
                    <p className={styles.capabilityDescription}>{def.description}</p>
                  )}
                  <div className={styles.workflowMeta}>
                    <span>Min Level: {permissionLevelLabels[def.minimumPermissionLevel] ?? def.minimumPermissionLevel}</span>
                  </div>
                  <div className={styles.workflowMeta}>
                    <span>Modules: {def.affectedModules}</span>
                  </div>
                  <div className={styles.workflowMeta}>
                    <span>Category: {def.category}</span>
                  </div>
                  {def.isActive && (
                    <Button
                      variant="secondary"
                      size="sm"
                      disabled={executeWorkflowMutation.isPending}
                      onClick={() => executeWorkflowMutation.mutate(def.id)}
                      style={{ marginTop: 'var(--spacing-2)' }}
                    >
                      Execute Workflow
                    </Button>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Workflow Executions */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Workflow Executions</h3>
          <div className={styles.filters}>
            <div className={styles.filterSelect}>
              <Select
                label=""
                value={wfStatusFilter}
                onChange={(e) => { setWfStatusFilter(e.target.value); setWfPage(1) }}
                options={workflowStatusOptions}
              />
            </div>
          </div>
          {executions.length === 0 ? (
            <div className={styles.emptyState}>
              <Workflow size={48} className={styles.emptyIcon} />
              <p>No workflow executions recorded yet.</p>
            </div>
          ) : (
            <div className={styles.executionsList}>
              {executions.map(exec => (
                <div key={exec.id} className={styles.executionCard}>
                  <div className={styles.executionHeader}>
                    <h4 className={styles.modelName}>{exec.workflowName}</h4>
                    <StatusBadge status={exec.status} />
                  </div>
                  <div className={styles.executionProgress}>
                    <div className={styles.progressBar}>
                      <div
                        className={styles.progressFill}
                        style={{ width: exec.totalSteps > 0 ? `${(exec.completedSteps / exec.totalSteps) * 100}%` : '0%' }}
                      />
                    </div>
                    <span className={styles.progressLabel}>
                      {exec.completedSteps}/{exec.totalSteps} steps
                      {exec.failedSteps > 0 && ` (${exec.failedSteps} failed)`}
                    </span>
                  </div>
                  <div className={styles.workflowMeta}>
                    <span>Started: {formatDate(exec.startedAt)}</span>
                    {exec.completedAt && <span>Completed: {formatDate(exec.completedAt)}</span>}
                    {exec.totalDurationMs !== null && <span>{exec.totalDurationMs}ms</span>}
                  </div>
                  {exec.errorSummary && (
                    <div className={styles.errorBanner}>{exec.errorSummary}</div>
                  )}
                  {exec.status === 'PendingConfirmation' && (
                    <div className={styles.executionActions}>
                      <Button
                        variant="primary"
                        size="sm"
                        disabled={confirmWorkflowMutation.isPending}
                        onClick={() => confirmWorkflowMutation.mutate({ executionId: exec.id, confirm: true })}
                      >
                        Confirm &amp; Execute
                      </Button>
                      <Button
                        variant="secondary"
                        size="sm"
                        disabled={confirmWorkflowMutation.isPending}
                        onClick={() => confirmWorkflowMutation.mutate({ executionId: exec.id, confirm: false })}
                      >
                        Cancel
                      </Button>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
          {(executionsData?.totalCount ?? 0) > pageSize && (
            <div style={{ display: 'flex', justifyContent: 'center', gap: 'var(--spacing-2)', marginTop: 'var(--spacing-4)' }}>
              <Button variant="secondary" size="sm" disabled={wfPage <= 1} onClick={() => setWfPage(p => p - 1)}>Previous</Button>
              <span style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', padding: 'var(--spacing-2)' }}>
                Page {wfPage} of {Math.ceil((executionsData?.totalCount ?? 0) / pageSize)}
              </span>
              <Button variant="secondary" size="sm" disabled={wfPage >= Math.ceil((executionsData?.totalCount ?? 0) / pageSize)} onClick={() => setWfPage(p => p + 1)}>Next</Button>
            </div>
          )}
        </div>
      </div>
    )
  }

  // ---- Knowledge Context tab ----

  function handleContextSearch() {
    if (contextSearchTerm.trim().length >= 2) {
      setSearchSubmitted(contextSearchTerm.trim())
    }
  }

  function renderKnowledgeContext() {
    const statsLoading = contextStatsQuery.isLoading
    const sourcesLoading = knowledgeSourcesQuery.isLoading

    const stats = contextStatsQuery.data
    const sources = knowledgeSourcesQuery.data ?? []
    const searchResults = contextSearchQuery.data ?? []

    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--spacing-6)' }}>
        {/* Context Statistics */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Knowledge Context Overview</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            Structured knowledge foundation for AI-powered quality analysis and recommendations.
          </p>
          {statsLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 100 }} />
          ) : stats ? (
            <div className={styles.contextStatsGrid}>
              <div className={styles.contextStatCard}>
                <p className={styles.contextStatValue}>{stats.totalDocuments}</p>
                <p className={styles.contextStatLabel}>Total Documents</p>
              </div>
              <div className={`${styles.contextStatCard} ${stats.indexedDocuments > 0 ? styles.contextStatSuccess : ''}`}>
                <p className={styles.contextStatValue}>{stats.indexedDocuments}</p>
                <p className={styles.contextStatLabel}>Indexed</p>
              </div>
              <div className={`${styles.contextStatCard} ${stats.pendingDocuments > 0 ? styles.contextStatWarning : ''}`}>
                <p className={styles.contextStatValue}>{stats.pendingDocuments}</p>
                <p className={styles.contextStatLabel}>Pending</p>
              </div>
              <div className={`${styles.contextStatCard} ${stats.failedDocuments > 0 ? styles.contextStatError : ''}`}>
                <p className={styles.contextStatValue}>{stats.failedDocuments}</p>
                <p className={styles.contextStatLabel}>Failed</p>
              </div>
              <div className={`${styles.contextStatCard} ${stats.staleDocuments > 0 ? styles.contextStatWarning : ''}`}>
                <p className={styles.contextStatValue}>{stats.staleDocuments}</p>
                <p className={styles.contextStatLabel}>Stale</p>
              </div>
              <div className={styles.contextStatCard}>
                <p className={styles.contextStatValue}>{stats.activeSources}/{stats.totalSources}</p>
                <p className={styles.contextStatLabel}>Active Sources</p>
              </div>
            </div>
          ) : null}
          {stats?.lastSyncedAt && (
            <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', margin: 0 }}>
              Last synced: {formatDate(stats.lastSyncedAt)}
            </p>
          )}
        </div>

        {/* Context Search */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Context Search</h3>
          <div className={styles.searchRow}>
            <div className={styles.searchField}>
              <Input
                placeholder="Search knowledge context..."
                value={contextSearchTerm}
                onChange={(e) => setContextSearchTerm(e.target.value)}
                onKeyDown={(e) => e.key === 'Enter' && handleContextSearch()}
              />
            </div>
            <div className={styles.filterSelect}>
              <Select
                label=""
                value={contextModuleFilter}
                onChange={(e) => setContextModuleFilter(e.target.value)}
                options={contextModuleOptions}
              />
            </div>
            <Button
              variant="primary"
              onClick={handleContextSearch}
              disabled={contextSearchTerm.trim().length < 2}
            >
              <Search size={14} style={{ marginRight: 4 }} />
              Search
            </Button>
          </div>

          {contextSearchQuery.isLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 200 }} />
          ) : searchSubmitted && searchResults.length === 0 ? (
            <div className={styles.emptyState}>
              <Search size={48} className={styles.emptyIcon} />
              <p>No matching context documents found.</p>
            </div>
          ) : searchResults.length > 0 ? (
            <div className={styles.searchResultsList}>
              {searchResults.map(result => (
                <div key={result.documentId} className={styles.searchResultCard}>
                  <div className={styles.searchResultHeader}>
                    <h4 className={styles.searchResultTitle}>{result.title}</h4>
                    <div className={styles.relevanceBar}>
                      <div className={styles.relevanceTrack}>
                        <div className={styles.relevanceFill} style={{ width: `${result.relevanceScore * 100}%` }} />
                      </div>
                      <span className={styles.relevanceLabel}>{(result.relevanceScore * 100).toFixed(0)}%</span>
                    </div>
                  </div>
                  <p className={styles.searchResultSnippet}>{result.contentSnippet}</p>
                  <div className={styles.searchResultMeta}>
                    <span className={styles.tag}>{result.sourceModule}</span>
                    <span className={styles.tag}>{result.contextType}</span>
                    {result.indexedAt && <span>Indexed: {formatDate(result.indexedAt)}</span>}
                  </div>
                </div>
              ))}
            </div>
          ) : !searchSubmitted ? (
            <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', margin: 0 }}>
              Enter a search term to find relevant context documents across the QMS knowledge foundation.
            </p>
          ) : null}
        </div>

        {/* Knowledge Sources */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Knowledge Sources</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            Registered QMS modules that contribute structured context to the AI knowledge foundation.
          </p>
          {sourcesLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 200 }} />
          ) : sources.length === 0 ? (
            <div className={styles.emptyState}>
              <Database size={48} className={styles.emptyIcon} />
              <p>No knowledge sources registered yet.</p>
            </div>
          ) : (
            <div className={styles.sourcesList}>
              {sources.map(source => (
                <div key={source.id} className={styles.sourceCard}>
                  <div className={styles.sourceHeader}>
                    <h4 className={styles.sourceName}>{source.name}</h4>
                    <StatusBadge status={source.isActive ? 'Active' : 'Inactive'} />
                  </div>
                  {source.description && (
                    <p className={styles.capabilityDescription}>{source.description}</p>
                  )}
                  <div className={styles.sourceMeta}>
                    <span className={styles.tag}>{source.module}</span>
                    <span>{source.documentCount} document{source.documentCount !== 1 ? 's' : ''}</span>
                    {source.lastSyncedAt && <span>Synced: {formatDate(source.lastSyncedAt)}</span>}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    )
  }

  // ---- Confirmation Dialog ----

  function renderConfirmationDialog() {
    if (!confirmationRequest) return null

    return (
      <div className={styles.overlay} onClick={() => setConfirmationRequest(null)} role="dialog" aria-modal="true" aria-label="Confirm AI Action">
        <div className={styles.confirmDialog} onClick={(e) => e.stopPropagation()}>
          <div className={styles.confirmHeader}>
            <AlertTriangle size={20} style={{ color: riskColors[confirmationRequest.riskLevel] ?? 'var(--color-warning)' }} />
            <span className={styles.confirmTitle}>AI Action Confirmation Required</span>
          </div>

          <div className={styles.confirmBody}>
            <div className={styles.confirmField}>
              <span className={styles.confirmFieldLabel}>Action</span>
              <span>{confirmationRequest.actionType.replace(/([A-Z])/g, ' $1').trim()}</span>
            </div>
            <div className={styles.confirmField}>
              <span className={styles.confirmFieldLabel}>Category</span>
              <span className={styles.tag}>{confirmationRequest.actionCategory}</span>
            </div>
            <div className={styles.confirmField}>
              <span className={styles.confirmFieldLabel}>Risk Level</span>
              <span style={{ color: riskColors[confirmationRequest.riskLevel], fontWeight: 600 }}>
                {confirmationRequest.riskLevel}
              </span>
            </div>
            {confirmationRequest.affectedRecords && (
              <div className={styles.confirmField}>
                <span className={styles.confirmFieldLabel}>Affected Records</span>
                <span>{confirmationRequest.affectedRecords}</span>
              </div>
            )}
            <div className={styles.confirmField}>
              <span className={styles.confirmFieldLabel}>Rollback</span>
              <span>{confirmationRequest.isRollbackPossible ? 'Possible' : 'Not possible — this action cannot be undone'}</span>
            </div>
          </div>

          <div className={styles.confirmSummary}>
            {confirmationRequest.actionSummary}
          </div>

          <div className={styles.confirmPrompt}>
            {confirmationRequest.confirmationPrompt}
          </div>

          <div className={styles.confirmActions}>
            <Button
              variant="secondary"
              onClick={() => {
                confirmActionMutation.mutate({ actionLogId: confirmationRequest.actionLogId, confirm: false })
              }}
              disabled={confirmActionMutation.isPending}
            >
              Reject
            </Button>
            <Button
              variant="danger"
              onClick={() => {
                confirmActionMutation.mutate({ actionLogId: confirmationRequest.actionLogId, confirm: true })
              }}
              disabled={confirmActionMutation.isPending}
            >
              Confirm Execution
            </Button>
          </div>
        </div>
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
        <button className={`${styles.tab} ${activeTab === 'permissions' ? styles.tabActive : ''}`} onClick={() => setActiveTab('permissions')}>
          <Shield size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          Permissions
        </button>
        <button className={`${styles.tab} ${activeTab === 'actionLog' ? styles.tabActive : ''}`} onClick={() => setActiveTab('actionLog')}>
          <ClipboardList size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          Action Log
        </button>
        <button className={`${styles.tab} ${activeTab === 'workflows' ? styles.tabActive : ''}`} onClick={() => setActiveTab('workflows')}>
          <Workflow size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          Workflows
        </button>
        <button className={`${styles.tab} ${activeTab === 'knowledgeContext' ? styles.tabActive : ''}`} onClick={() => setActiveTab('knowledgeContext')}>
          <Database size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          Knowledge Context
        </button>
      </div>

      {activeTab === 'capabilities' && renderCapabilities()}
      {activeTab === 'interactions' && renderInteractions()}
      {activeTab === 'models' && renderModels()}
      {activeTab === 'permissions' && renderPermissions()}
      {activeTab === 'actionLog' && renderActionLog()}
      {activeTab === 'workflows' && renderWorkflows()}
      {activeTab === 'knowledgeContext' && renderKnowledgeContext()}

      {renderConfirmationDialog()}
    </div>
  )
}
