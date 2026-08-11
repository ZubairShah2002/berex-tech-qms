import { useState, useCallback, useEffect } from 'react'
import { Brain, Activity, ToggleLeft, Shield, ClipboardList, Workflow, AlertTriangle, Database, Search, TrendingUp, BarChart3, CheckCircle, XCircle, Eye, Server, Gauge, Settings } from 'lucide-react'
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

interface AiRecommendationDto {
  id: string
  recommendationType: string
  title: string
  description: string
  severity: string
  sourceContextIds: string | null
  relatedModule: string
  relatedEntityId: string | null
  confidenceScore: number
  status: string
  reason: string
  supportingData: string | null
  recommendedAction: string | null
  reviewedAt: string | null
  reviewedBy: string | null
  reviewNotes: string | null
  createdAt: string
  modifiedAt: string | null
}

interface QualityInsightDto {
  category: string
  title: string
  description: string
  severity: string
  confidenceScore: number
  relatedModule: string
  supportingEvidence: string | null
  generatedAt: string
}

interface RiskSummaryDto {
  totalRecommendations: number
  criticalCount: number
  highCount: number
  mediumCount: number
  lowCount: number
  pendingReview: number
  acceptedCount: number
  rejectedCount: number
  riskByModule: RiskByModuleDto[]
  riskByType: RiskByTypeDto[]
}

interface RiskByModuleDto {
  module: string
  count: number
  criticalCount: number
  highCount: number
}

interface RiskByTypeDto {
  recommendationType: string
  count: number
  averageConfidence: number
}

// Sprint 16: AI Provider Integration types

interface AiProviderStatusDto {
  provider: string
  isEnabled: boolean
  isHealthy: boolean
  model: string
  timeoutSeconds: number
  maxRetries: number
  supportedTaskTypes: string[]
  lastErrorMessage: string | null
  lastSuccessAt: string | null
  lastErrorAt: string | null
  hasApiCost: boolean
}

interface AiLocalModelDto {
  name: string
  available: boolean
}

interface AiTaskMappingDto {
  taskType: string
  primaryProvider: string
  fallbackProvider: string | null
}

interface AiUsageSummaryDto {
  totalRequests: number
  successfulRequests: number
  failedRequests: number
  fallbackRequests: number
  totalInputTokens: number
  totalOutputTokens: number
  totalEstimatedCostUsd: number
  averageProcessingTimeMs: number
  usageByProvider: AiUsageByProviderDto[]
  usageByTaskType: AiUsageByTaskTypeDto[]
  localRequests: number
  cloudRequests: number
  localUsagePercent: number
  cloudUsagePercent: number
  estimatedAvoidedApiCostUsd: number
}

interface AiUsageByProviderDto {
  provider: string
  requestCount: number
  successCount: number
  failedCount: number
  totalTokens: number
  estimatedCostUsd: number
  averageProcessingTimeMs: number
  successRate: number
  fallbackCount: number
}

interface AiUsageByTaskTypeDto {
  taskType: string
  requestCount: number
  totalTokens: number
  averageProcessingTimeMs: number
}

// Sprint 18: AI Governance & User Preferences types

interface AiUserPreferenceDto {
  aiEnabled: boolean
  preferredProvider: string
  confirmationRequired: boolean
  preferredLanguage: string | null
  enabledTaskTypes: string[]
}

interface AiGovernancePolicyDto {
  aiEnabled: boolean
  allowedProviders: string[]
  defaultProvider: string | null
  allowedTaskTypes: string[]
  maxDailyRequestsPerUser: number | null
  maxMonthlyRequestsPerTenant: number | null
  requireHumanConfirmation: boolean
}

interface AiEffectivePolicyDto {
  aiEnabled: boolean
  allowedProviders: string[]
  preferredProvider: string | null
  allowedTaskTypes: string[]
  confirmationRequired: boolean
  preferredLanguage: string | null
  dailyRequestsUsed: number
  dailyRequestsLimit: number | null
  monthlyRequestsUsed: number
  monthlyRequestsLimit: number | null
}

// ---- Constants --------------------------------------------------------------

type TabId = 'capabilities' | 'interactions' | 'models' | 'permissions' | 'actionLog' | 'workflows' | 'knowledgeContext' | 'aiInsights' | 'providers' | 'settings'

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

const recommendationTypeLabels: Record<string, string> = {
  DefectTrend: 'Defect Trend',
  SupplierRisk: 'Supplier Risk',
  ProcessRisk: 'Process Risk',
  DocumentGap: 'Document Gap',
  AuditRisk: 'Audit Risk',
  CAPARecommendation: 'CAPA Recommendation',
}

const recommendationTypeOptions = [
  { value: '', label: 'All types' },
  { value: 'DefectTrend', label: 'Defect Trend' },
  { value: 'SupplierRisk', label: 'Supplier Risk' },
  { value: 'ProcessRisk', label: 'Process Risk' },
  { value: 'DocumentGap', label: 'Document Gap' },
  { value: 'AuditRisk', label: 'Audit Risk' },
  { value: 'CAPARecommendation', label: 'CAPA Recommendation' },
]

const recommendationStatusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Generated', label: 'Generated' },
  { value: 'Reviewed', label: 'Reviewed' },
  { value: 'Accepted', label: 'Accepted' },
  { value: 'Rejected', label: 'Rejected' },
  { value: 'Expired', label: 'Expired' },
]

const insightModuleOptions = [
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

const taskTypeLabels: Record<string, string> = {
  DocumentAnalysis: 'Document Analysis',
  QualityAnalysis: 'Quality Analysis',
  RecommendationGeneration: 'Recommendation',
  Summarization: 'Summarization',
  RiskAnalysis: 'Risk Analysis',
  CAPAAnalysis: 'CAPA Analysis',
  SupplierAnalysis: 'Supplier Analysis',
  AuditAnalysis: 'Audit Analysis',
  DefectTrendAnalysis: 'Defect Trend',
  StructuredDataExtraction: 'Data Extraction',
}

const providerOptions = [
  { value: 'Automatic', label: 'Automatic (system default)' },
  { value: 'Local', label: 'Local (Ollama)' },
  { value: 'Claude', label: 'Claude' },
  { value: 'OpenAi', label: 'OpenAI' },
]

const allTaskTypes = [
  'DocumentAnalysis', 'QualityAnalysis', 'RecommendationGeneration',
  'Summarization', 'RiskAnalysis', 'CAPAAnalysis', 'SupplierAnalysis',
  'AuditAnalysis', 'DefectTrendAnalysis', 'StructuredDataExtraction',
]

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

  // AI Insights filters
  const [recTypeFilter, setRecTypeFilter] = useState('')
  const [recStatusFilter, setRecStatusFilter] = useState('')
  const [insightModuleFilter, setInsightModuleFilter] = useState('')

  // Confirmation dialog
  const [confirmationRequest, setConfirmationRequest] = useState<AiConfirmationRequestDto | null>(null)

  // Settings state (Sprint 18)
  const [prefAiEnabled, setPrefAiEnabled] = useState(true)
  const [prefProvider, setPrefProvider] = useState('Automatic')
  const [prefConfirmation, setPrefConfirmation] = useState(true)
  const [prefLanguage, setPrefLanguage] = useState('')
  const [prefTaskTypes, setPrefTaskTypes] = useState<string[]>([])
  const [prefSaved, setPrefSaved] = useState(false)

  // Governance state (Sprint 18)
  const [govAiEnabled, setGovAiEnabled] = useState(true)
  const [govAllowedProviders, setGovAllowedProviders] = useState<string[]>([])
  const [govDefaultProvider, setGovDefaultProvider] = useState('')
  const [govAllowedTaskTypes, setGovAllowedTaskTypes] = useState<string[]>([])
  const [govDailyLimit, setGovDailyLimit] = useState('')
  const [govMonthlyLimit, setGovMonthlyLimit] = useState('')
  const [govSaved, setGovSaved] = useState(false)

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

  // AI Insights queries
  const recommendationsQuery = useQuery({
    queryKey: ['ai', 'recommendations', recTypeFilter, recStatusFilter],
    queryFn: () => {
      const params = new URLSearchParams()
      if (recTypeFilter) params.set('recommendationType', recTypeFilter)
      if (recStatusFilter) params.set('status', recStatusFilter)
      return apiClient.get<AiRecommendationDto[]>(`/api/v1/ai/recommendations?${params}`).then(r => r.data)
    },
    enabled: activeTab === 'aiInsights',
  })

  const riskSummaryQuery = useQuery({
    queryKey: ['ai', 'riskSummary'],
    queryFn: () => apiClient.get<RiskSummaryDto>('/api/v1/ai/risk-summary').then(r => r.data),
    enabled: activeTab === 'aiInsights',
  })

  const insightsQuery = useQuery({
    queryKey: ['ai', 'insights', insightModuleFilter],
    queryFn: () => {
      const params = new URLSearchParams()
      if (insightModuleFilter) params.set('module', insightModuleFilter)
      return apiClient.get<QualityInsightDto[]>(`/api/v1/ai/insights?${params}`).then(r => r.data)
    },
    enabled: activeTab === 'aiInsights',
  })

  // Sprint 16: Provider queries
  const providerStatusQuery = useQuery({
    queryKey: ['ai', 'providerStatus'],
    queryFn: () => apiClient.get<AiProviderStatusDto[]>('/api/v1/ai/providers/status').then(r => r.data),
    enabled: activeTab === 'providers',
  })

  const taskMappingsQuery = useQuery({
    queryKey: ['ai', 'taskMappings'],
    queryFn: () => apiClient.get<AiTaskMappingDto[]>('/api/v1/ai/providers/task-mappings').then(r => r.data),
    enabled: activeTab === 'providers',
  })

  const usageSummaryQuery = useQuery({
    queryKey: ['ai', 'usageSummary'],
    queryFn: () => apiClient.get<AiUsageSummaryDto>('/api/v1/ai/usage/summary').then(r => r.data),
    enabled: activeTab === 'providers',
  })

  const localModelsQuery = useQuery({
    queryKey: ['ai', 'localModels'],
    queryFn: () => apiClient.get<AiLocalModelDto[]>('/api/v1/ai/providers/local/models').then(r => r.data),
    enabled: activeTab === 'providers',
  })

  // Sprint 18: Settings queries
  const userPreferencesQuery = useQuery({
    queryKey: ['ai', 'preferences'],
    queryFn: () => apiClient.get<AiUserPreferenceDto>('/api/v1/ai/preferences').then(r => r.data),
    enabled: activeTab === 'settings',
  })

  const governancePolicyQuery = useQuery({
    queryKey: ['ai', 'governance'],
    queryFn: () => apiClient.get<AiGovernancePolicyDto>('/api/v1/ai/governance').then(r => r.data).catch(() => null),
    enabled: activeTab === 'settings',
  })

  const effectivePolicyQuery = useQuery({
    queryKey: ['ai', 'effectivePolicy'],
    queryFn: () => apiClient.get<AiEffectivePolicyDto>('/api/v1/ai/policy/effective').then(r => r.data),
    enabled: activeTab === 'settings',
  })

  // Sync form state when query data arrives
  useEffect(() => {
    if (userPreferencesQuery.data) {
      const p = userPreferencesQuery.data
      setPrefAiEnabled(p.aiEnabled)
      setPrefProvider(p.preferredProvider ?? 'Automatic')
      setPrefConfirmation(p.confirmationRequired)
      setPrefLanguage(p.preferredLanguage ?? '')
      setPrefTaskTypes(p.enabledTaskTypes ?? [])
    }
  }, [userPreferencesQuery.data])

  useEffect(() => {
    if (governancePolicyQuery.data) {
      const g = governancePolicyQuery.data
      setGovAiEnabled(g.aiEnabled)
      setGovAllowedProviders(g.allowedProviders ?? [])
      setGovDefaultProvider(g.defaultProvider ?? '')
      setGovAllowedTaskTypes(g.allowedTaskTypes ?? [])
      setGovDailyLimit(g.maxDailyRequestsPerUser?.toString() ?? '')
      setGovMonthlyLimit(g.maxMonthlyRequestsPerTenant?.toString() ?? '')
    }
  }, [governancePolicyQuery.data])

  // ---- Mutations ----

  const reviewRecommendationMutation = useMutation({
    mutationFn: (data: { id: string; action: string; notes?: string }) =>
      apiClient.post(`/api/v1/ai/recommendations/${data.id}/review`, { action: data.action, notes: data.notes }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ai', 'recommendations'] })
      queryClient.invalidateQueries({ queryKey: ['ai', 'riskSummary'] })
    },
  })

  const dismissRecommendationMutation = useMutation({
    mutationFn: (id: string) =>
      apiClient.post(`/api/v1/ai/recommendations/${id}/dismiss`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ai', 'recommendations'] })
      queryClient.invalidateQueries({ queryKey: ['ai', 'riskSummary'] })
    },
  })

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

  // Sprint 18: Settings mutations
  const updatePreferencesMutation = useMutation({
    mutationFn: (data: { aiEnabled: boolean; preferredProvider: string | null; confirmationRequired: boolean; preferredLanguage: string | null; enabledTaskTypes: string[] | null }) =>
      apiClient.put('/api/v1/ai/preferences', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ai', 'preferences'] })
      queryClient.invalidateQueries({ queryKey: ['ai', 'effectivePolicy'] })
      setPrefSaved(true)
      setTimeout(() => setPrefSaved(false), 3000)
    },
  })

  const updateGovernanceMutation = useMutation({
    mutationFn: (data: { aiEnabled: boolean; allowedProviders: string[] | null; defaultProvider: string | null; allowedTaskTypes: string[] | null; maxDailyRequestsPerUser: number | null; maxMonthlyRequestsPerTenant: number | null; requireHumanConfirmation: boolean }) =>
      apiClient.put('/api/v1/ai/governance', data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['ai', 'governance'] })
      queryClient.invalidateQueries({ queryKey: ['ai', 'effectivePolicy'] })
      setGovSaved(true)
      setTimeout(() => setGovSaved(false), 3000)
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

  // ---- AI Insights tab ----

  function severityColor(severity: string): string {
    switch (severity) {
      case 'Critical': return 'var(--color-error)'
      case 'High': return 'var(--color-error)'
      case 'Medium': return 'var(--color-warning)'
      case 'Low': return 'var(--color-success)'
      default: return 'var(--color-text-secondary)'
    }
  }

  function renderAiInsights() {
    const riskLoading = riskSummaryQuery.isLoading
    const recsLoading = recommendationsQuery.isLoading
    const insightsLoading = insightsQuery.isLoading

    const summary = riskSummaryQuery.data
    const recommendations = recommendationsQuery.data ?? []
    const insights = insightsQuery.data ?? []

    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--spacing-6)' }}>
        {/* Risk Summary */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Risk Summary</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            Aggregate quality risk assessment based on AI-generated recommendations.
          </p>
          {riskLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 120 }} />
          ) : summary ? (
            <>
              <div className={styles.riskStatsGrid}>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{summary.totalRecommendations}</p>
                  <p className={styles.statLabel}>Total</p>
                </div>
                <div className={`${styles.statCard} ${summary.criticalCount > 0 ? styles.riskStatCritical : ''}`}>
                  <p className={styles.statValue}>{summary.criticalCount}</p>
                  <p className={styles.statLabel}>Critical</p>
                </div>
                <div className={`${styles.statCard} ${summary.highCount > 0 ? styles.riskStatHigh : ''}`}>
                  <p className={styles.statValue}>{summary.highCount}</p>
                  <p className={styles.statLabel}>High</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{summary.mediumCount}</p>
                  <p className={styles.statLabel}>Medium</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{summary.lowCount}</p>
                  <p className={styles.statLabel}>Low</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{summary.pendingReview}</p>
                  <p className={styles.statLabel}>Pending Review</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{summary.acceptedCount}</p>
                  <p className={styles.statLabel}>Accepted</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{summary.rejectedCount}</p>
                  <p className={styles.statLabel}>Rejected</p>
                </div>
              </div>

              {/* Risk by Module */}
              {summary.riskByModule.length > 0 && (
                <div style={{ marginTop: 'var(--spacing-5)' }}>
                  <h4 className={styles.subsectionTitle}>Risk by Module</h4>
                  <div className={styles.riskModuleGrid}>
                    {summary.riskByModule.map(mod => (
                      <div key={mod.module} className={styles.riskModuleCard}>
                        <div className={styles.riskModuleHeader}>
                          <span className={styles.riskModuleName}>{mod.module}</span>
                          <span className={styles.riskModuleCount}>{mod.count}</span>
                        </div>
                        <div className={styles.riskModuleBreakdown}>
                          {mod.criticalCount > 0 && (
                            <span style={{ color: 'var(--color-error)', fontWeight: 600, fontSize: 'var(--font-size-xs)' }}>
                              {mod.criticalCount} critical
                            </span>
                          )}
                          {mod.highCount > 0 && (
                            <span style={{ color: 'var(--color-error)', fontSize: 'var(--font-size-xs)' }}>
                              {mod.highCount} high
                            </span>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Risk by Type */}
              {summary.riskByType.length > 0 && (
                <div style={{ marginTop: 'var(--spacing-5)' }}>
                  <h4 className={styles.subsectionTitle}>Risk by Type</h4>
                  <div className={styles.riskTypeGrid}>
                    {summary.riskByType.map(t => (
                      <div key={t.recommendationType} className={styles.riskTypeCard}>
                        <span className={styles.riskTypeName}>
                          {recommendationTypeLabels[t.recommendationType] ?? t.recommendationType}
                        </span>
                        <span className={styles.riskTypeCount}>{t.count}</span>
                        <span className={styles.riskTypeConfidence}>
                          Avg confidence: {(t.averageConfidence * 100).toFixed(0)}%
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </>
          ) : null}
        </div>

        {/* Recommendations */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Recommendations</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            AI-generated quality recommendations. Review, accept, or reject each item.
          </p>
          <div className={styles.filters}>
            <div className={styles.filterSelect}>
              <Select
                label=""
                value={recTypeFilter}
                onChange={(e) => setRecTypeFilter(e.target.value)}
                options={recommendationTypeOptions}
              />
            </div>
            <div className={styles.filterSelect}>
              <Select
                label=""
                value={recStatusFilter}
                onChange={(e) => setRecStatusFilter(e.target.value)}
                options={recommendationStatusOptions}
              />
            </div>
          </div>
          {recsLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 200 }} />
          ) : recommendations.length === 0 ? (
            <div className={styles.emptyState}>
              <BarChart3 size={48} className={styles.emptyIcon} />
              <p>No recommendations generated yet.</p>
            </div>
          ) : (
            <div className={styles.recommendationsList}>
              {recommendations.map(rec => (
                <div key={rec.id} className={styles.recommendationCard}>
                  <div className={styles.recommendationHeader}>
                    <div className={styles.recommendationTitleRow}>
                      <span
                        className={styles.severityDot}
                        style={{ background: severityColor(rec.severity) }}
                      />
                      <h4 className={styles.recommendationTitle}>{rec.title}</h4>
                    </div>
                    <div className={styles.recommendationBadges}>
                      <span className={styles.tag}>
                        {recommendationTypeLabels[rec.recommendationType] ?? rec.recommendationType}
                      </span>
                      <StatusBadge status={rec.status} />
                    </div>
                  </div>

                  <p className={styles.recommendationDesc}>{rec.description}</p>

                  {rec.reason && (
                    <div className={styles.recommendationDetail}>
                      <span className={styles.recommendationDetailLabel}>Reason</span>
                      <span>{rec.reason}</span>
                    </div>
                  )}

                  {rec.recommendedAction && (
                    <div className={styles.recommendationDetail}>
                      <span className={styles.recommendationDetailLabel}>Recommended Action</span>
                      <span>{rec.recommendedAction}</span>
                    </div>
                  )}

                  {rec.supportingData && (
                    <div className={styles.recommendationDetail}>
                      <span className={styles.recommendationDetailLabel}>Supporting Data</span>
                      <span>{rec.supportingData}</span>
                    </div>
                  )}

                  <div className={styles.recommendationMeta}>
                    <span>Module: {rec.relatedModule}</span>
                    <span>Severity: <span style={{ color: severityColor(rec.severity), fontWeight: 600 }}>{rec.severity}</span></span>
                    <span>Confidence: {(rec.confidenceScore * 100).toFixed(0)}%</span>
                    <span>Generated: {formatDate(rec.createdAt)}</span>
                  </div>

                  {rec.reviewedBy && (
                    <div className={styles.recommendationMeta}>
                      <span>Reviewed by: {rec.reviewedBy}</span>
                      {rec.reviewedAt && <span>Reviewed: {formatDate(rec.reviewedAt)}</span>}
                      {rec.reviewNotes && <span>Notes: {rec.reviewNotes}</span>}
                    </div>
                  )}

                  {(rec.status === 'Generated' || rec.status === 'Reviewed') && (
                    <div className={styles.recommendationActions}>
                      {rec.status === 'Generated' && (
                        <Button
                          variant="secondary"
                          size="sm"
                          disabled={reviewRecommendationMutation.isPending}
                          onClick={() => reviewRecommendationMutation.mutate({ id: rec.id, action: 'review' })}
                        >
                          <Eye size={14} style={{ marginRight: 4 }} />
                          Mark Reviewed
                        </Button>
                      )}
                      <Button
                        variant="primary"
                        size="sm"
                        disabled={reviewRecommendationMutation.isPending}
                        onClick={() => reviewRecommendationMutation.mutate({ id: rec.id, action: 'accept' })}
                      >
                        <CheckCircle size={14} style={{ marginRight: 4 }} />
                        Accept
                      </Button>
                      <Button
                        variant="secondary"
                        size="sm"
                        disabled={reviewRecommendationMutation.isPending}
                        onClick={() => reviewRecommendationMutation.mutate({ id: rec.id, action: 'reject' })}
                      >
                        <XCircle size={14} style={{ marginRight: 4 }} />
                        Reject
                      </Button>
                      <Button
                        variant="ghost"
                        size="sm"
                        disabled={dismissRecommendationMutation.isPending}
                        onClick={() => dismissRecommendationMutation.mutate(rec.id)}
                      >
                        Dismiss
                      </Button>
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Quality Insights */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Quality Insights</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            Real-time quality analysis generated from the QMS knowledge foundation.
          </p>
          <div className={styles.filters}>
            <div className={styles.filterSelect}>
              <Select
                label=""
                value={insightModuleFilter}
                onChange={(e) => setInsightModuleFilter(e.target.value)}
                options={insightModuleOptions}
              />
            </div>
          </div>
          {insightsLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 200 }} />
          ) : insights.length === 0 ? (
            <div className={styles.emptyState}>
              <TrendingUp size={48} className={styles.emptyIcon} />
              <p>No quality insights available. Context documents must be indexed before insights can be generated.</p>
            </div>
          ) : (
            <div className={styles.insightsList}>
              {insights.map((insight, idx) => (
                <div key={`${insight.category}-${idx}`} className={styles.insightCard}>
                  <div className={styles.insightHeader}>
                    <div className={styles.recommendationTitleRow}>
                      <span
                        className={styles.severityDot}
                        style={{ background: severityColor(insight.severity) }}
                      />
                      <h4 className={styles.recommendationTitle}>{insight.title}</h4>
                    </div>
                    <span style={{ color: severityColor(insight.severity), fontWeight: 600, fontSize: 'var(--font-size-xs)' }}>
                      {insight.severity}
                    </span>
                  </div>
                  <p className={styles.recommendationDesc}>{insight.description}</p>
                  {insight.supportingEvidence && (
                    <div className={styles.recommendationDetail}>
                      <span className={styles.recommendationDetailLabel}>Evidence</span>
                      <span>{insight.supportingEvidence}</span>
                    </div>
                  )}
                  <div className={styles.recommendationMeta}>
                    <span className={styles.tag}>{insight.category}</span>
                    <span>Module: {insight.relatedModule}</span>
                    <span>Confidence: {(insight.confidenceScore * 100).toFixed(0)}%</span>
                    <span>Generated: {formatDate(insight.generatedAt)}</span>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    )
  }

  // ---- Providers tab (Sprint 16 + Sprint 17) ----

  function renderProviders() {
    const statusLoading = providerStatusQuery.isLoading
    const mappingsLoading = taskMappingsQuery.isLoading
    const usageLoading = usageSummaryQuery.isLoading

    const providers = providerStatusQuery.data ?? []
    const mappings = taskMappingsQuery.data ?? []
    const usage = usageSummaryQuery.data
    const localModels = localModelsQuery.data ?? []

    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--spacing-6)' }}>
        {/* Provider Status */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Provider Status</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            Configured AI providers and their current operational status.
          </p>
          {statusLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 200 }} />
          ) : providers.length === 0 ? (
            <div className={styles.emptyState}>
              <Server size={48} className={styles.emptyIcon} />
              <p>No AI providers configured.</p>
            </div>
          ) : (
            <div className={styles.capabilitiesGrid}>
              {providers.map(p => (
                <div key={p.provider} className={styles.capabilityCard}>
                  <div className={styles.capabilityHeader}>
                    <h4 className={styles.capabilityName}>{p.provider}</h4>
                    <div style={{ display: 'flex', gap: 'var(--spacing-2)', alignItems: 'center' }}>
                      {!p.hasApiCost && (
                        <span className={styles.tag} style={{ fontSize: 'var(--font-size-xs)' }}>No API Cost</span>
                      )}
                      <StatusBadge status={p.isHealthy ? 'Healthy' : p.isEnabled ? 'Degraded' : 'Disabled'} />
                    </div>
                  </div>
                  <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--spacing-2)' }}>
                    <div className={styles.thresholdBar}>
                      <span className={styles.thresholdLabel}>Model</span>
                      <span className={styles.thresholdValue}>{p.model}</span>
                    </div>
                    <div className={styles.thresholdBar}>
                      <span className={styles.thresholdLabel}>Timeout</span>
                      <span className={styles.thresholdValue}>{p.timeoutSeconds}s</span>
                    </div>
                    <div className={styles.thresholdBar}>
                      <span className={styles.thresholdLabel}>Max Retries</span>
                      <span className={styles.thresholdValue}>{p.maxRetries}</span>
                    </div>
                    <div className={styles.thresholdBar}>
                      <span className={styles.thresholdLabel}>Status</span>
                      <span className={styles.thresholdValue}>{p.isEnabled ? 'Enabled' : 'Disabled'}</span>
                    </div>
                    <div className={styles.thresholdBar}>
                      <span className={styles.thresholdLabel}>Cost Type</span>
                      <span className={styles.thresholdValue}>{p.hasApiCost ? 'Paid API' : 'Local (no API cost)'}</span>
                    </div>
                  </div>
                  {p.supportedTaskTypes.length > 0 && (
                    <div style={{ marginTop: 'var(--spacing-2)' }}>
                      <span style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', fontWeight: 500 }}>Supported Tasks</span>
                      <div className={styles.tagList} style={{ marginTop: 'var(--spacing-1)' }}>
                        {p.supportedTaskTypes.map(t => (
                          <span key={t} className={styles.tag}>{taskTypeLabels[t] ?? t}</span>
                        ))}
                      </div>
                    </div>
                  )}
                  {p.lastErrorMessage && (
                    <div style={{ marginTop: 'var(--spacing-2)', fontSize: 'var(--font-size-xs)', color: 'var(--color-error)' }}>
                      Last error: {p.lastErrorMessage}
                    </div>
                  )}
                  <div className={styles.workflowMeta} style={{ marginTop: 'var(--spacing-2)' }}>
                    {p.lastSuccessAt && <span>Last success: {formatDate(p.lastSuccessAt)}</span>}
                    {p.lastErrorAt && <span>Last error: {formatDate(p.lastErrorAt)}</span>}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Local Models (Sprint 17) */}
        {providers.some(p => p.provider === 'Local' && p.isEnabled) && (
          <div className={styles.section}>
            <h3 className={styles.sectionTitle}>Local Models</h3>
            <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
              Models installed on the local inference server.
            </p>
            {localModelsQuery.isLoading ? (
              <div className={styles.loadingSkeleton} style={{ height: 80 }} />
            ) : localModels.length === 0 ? (
              <div className={styles.emptyState}>
                <Server size={32} className={styles.emptyIcon} />
                <p>No local models detected. The local inference server may be unavailable.</p>
              </div>
            ) : (
              <div className={styles.sourcesList}>
                {localModels.map(m => (
                  <div key={m.name} className={styles.sourceCard}>
                    <div className={styles.sourceHeader}>
                      <h4 className={styles.sourceName}>{m.name}</h4>
                      <StatusBadge status={m.available ? 'Available' : 'Unavailable'} />
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Task Mappings */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Task–Provider Routing</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            Provider chain for each AI task type. Requests follow the primary provider first, then fall back in order.
          </p>
          {mappingsLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 100 }} />
          ) : mappings.length === 0 ? (
            <div className={styles.emptyState}>
              <Gauge size={48} className={styles.emptyIcon} />
              <p>No task mappings configured.</p>
            </div>
          ) : (
            <div className={styles.sourcesList}>
              {mappings.map(m => (
                <div key={m.taskType} className={styles.sourceCard}>
                  <div className={styles.sourceHeader}>
                    <h4 className={styles.sourceName}>{taskTypeLabels[m.taskType] ?? m.taskType}</h4>
                  </div>
                  <div className={styles.sourceMeta}>
                    <span>Primary: <strong>{m.primaryProvider}</strong></span>
                    {m.fallbackProvider && (
                      <span>Fallback chain: <strong>{m.fallbackProvider}</strong></span>
                    )}
                    {!m.fallbackProvider && (
                      <span style={{ color: 'var(--color-text-secondary)' }}>No fallback</span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Usage Summary */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Usage Summary</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            Aggregate AI provider usage, token consumption, and cost tracking.
          </p>
          {usageLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 200 }} />
          ) : !usage ? (
            <div className={styles.emptyState}>
              <BarChart3 size={48} className={styles.emptyIcon} />
              <p>No usage data available yet.</p>
            </div>
          ) : (
            <>
              <div className={styles.riskStatsGrid}>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{usage.totalRequests}</p>
                  <p className={styles.statLabel}>Total Requests</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{usage.successfulRequests}</p>
                  <p className={styles.statLabel}>Successful</p>
                </div>
                <div className={`${styles.statCard} ${usage.failedRequests > 0 ? styles.riskStatHigh : ''}`}>
                  <p className={styles.statValue}>{usage.failedRequests}</p>
                  <p className={styles.statLabel}>Failed</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{usage.fallbackRequests}</p>
                  <p className={styles.statLabel}>Fallback</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{(usage.totalInputTokens + usage.totalOutputTokens).toLocaleString()}</p>
                  <p className={styles.statLabel}>Total Tokens</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>${usage.totalEstimatedCostUsd.toFixed(4)}</p>
                  <p className={styles.statLabel}>Est. API Cost</p>
                </div>
                <div className={styles.statCard}>
                  <p className={styles.statValue}>{usage.averageProcessingTimeMs.toFixed(0)}ms</p>
                  <p className={styles.statLabel}>Avg Response</p>
                </div>
              </div>

              {/* Cost Optimization (Sprint 17) */}
              {usage.totalRequests > 0 && (
                <div style={{ marginTop: 'var(--spacing-5)' }}>
                  <h4 className={styles.subsectionTitle}>Cost Optimization</h4>
                  <p style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-3)' }}>
                    Local vs cloud provider distribution. Local inference has zero API cost — infrastructure costs (hardware, electricity) are not tracked here.
                  </p>
                  <div className={styles.riskStatsGrid}>
                    <div className={styles.statCard}>
                      <p className={styles.statValue}>{usage.localRequests}</p>
                      <p className={styles.statLabel}>Local Requests</p>
                    </div>
                    <div className={styles.statCard}>
                      <p className={styles.statValue}>{usage.cloudRequests}</p>
                      <p className={styles.statLabel}>Cloud Requests</p>
                    </div>
                    <div className={styles.statCard}>
                      <p className={styles.statValue}>{usage.localUsagePercent.toFixed(1)}%</p>
                      <p className={styles.statLabel}>Local Usage</p>
                    </div>
                    <div className={styles.statCard}>
                      <p className={styles.statValue}>{usage.cloudUsagePercent.toFixed(1)}%</p>
                      <p className={styles.statLabel}>Cloud Usage</p>
                    </div>
                    <div className={styles.statCard}>
                      <p className={styles.statValue}>${usage.estimatedAvoidedApiCostUsd.toFixed(4)}</p>
                      <p className={styles.statLabel}>Est. Avoided API Cost</p>
                    </div>
                  </div>
                </div>
              )}

              {/* Usage by Provider */}
              {usage.usageByProvider.length > 0 && (
                <div style={{ marginTop: 'var(--spacing-5)' }}>
                  <h4 className={styles.subsectionTitle}>Usage by Provider</h4>
                  <div className={styles.riskModuleGrid}>
                    {usage.usageByProvider.map(up => (
                      <div key={up.provider} className={styles.riskModuleCard}>
                        <div className={styles.riskModuleHeader}>
                          <span className={styles.riskModuleName}>{up.provider}</span>
                          <span className={styles.riskModuleCount}>{up.requestCount} req</span>
                        </div>
                        <div className={styles.sourceMeta} style={{ marginTop: 'var(--spacing-1)' }}>
                          <span>{up.successCount} ok ({up.successRate.toFixed(1)}%)</span>
                          <span>{up.failedCount} failed</span>
                          <span>{up.fallbackCount} fallback</span>
                          <span>{up.totalTokens.toLocaleString()} tokens</span>
                          <span>${up.estimatedCostUsd.toFixed(4)}</span>
                          <span>{up.averageProcessingTimeMs.toFixed(0)}ms avg</span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}

              {/* Usage by Task Type */}
              {usage.usageByTaskType.length > 0 && (
                <div style={{ marginTop: 'var(--spacing-5)' }}>
                  <h4 className={styles.subsectionTitle}>Usage by Task Type</h4>
                  <div className={styles.riskTypeGrid}>
                    {usage.usageByTaskType.map(ut => (
                      <div key={ut.taskType} className={styles.riskTypeCard}>
                        <span className={styles.riskTypeName}>{taskTypeLabels[ut.taskType] ?? ut.taskType}</span>
                        <span className={styles.riskTypeCount}>{ut.requestCount}</span>
                        <span className={styles.riskTypeConfidence}>
                          {ut.averageProcessingTimeMs.toFixed(0)}ms avg
                        </span>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </>
          )}
        </div>
      </div>
    )
  }

  // ---- Settings tab (Sprint 18) ----

  function handleSavePreferences() {
    updatePreferencesMutation.mutate({
      aiEnabled: prefAiEnabled,
      preferredProvider: prefProvider === 'Automatic' ? null : prefProvider,
      confirmationRequired: prefConfirmation,
      preferredLanguage: prefLanguage || null,
      enabledTaskTypes: prefTaskTypes.length > 0 ? prefTaskTypes : null,
    })
  }

  function handleSaveGovernance() {
    updateGovernanceMutation.mutate({
      aiEnabled: govAiEnabled,
      allowedProviders: govAllowedProviders.length > 0 ? govAllowedProviders : null,
      defaultProvider: govDefaultProvider || null,
      allowedTaskTypes: govAllowedTaskTypes.length > 0 ? govAllowedTaskTypes : null,
      maxDailyRequestsPerUser: govDailyLimit ? parseInt(govDailyLimit, 10) : null,
      maxMonthlyRequestsPerTenant: govMonthlyLimit ? parseInt(govMonthlyLimit, 10) : null,
      requireHumanConfirmation: true,
    })
  }

  function togglePrefTaskType(taskType: string) {
    setPrefTaskTypes(prev =>
      prev.includes(taskType)
        ? prev.filter(t => t !== taskType)
        : [...prev, taskType]
    )
  }

  function toggleGovProvider(provider: string) {
    setGovAllowedProviders(prev =>
      prev.includes(provider)
        ? prev.filter(p => p !== provider)
        : [...prev, provider]
    )
  }

  function toggleGovTaskType(taskType: string) {
    setGovAllowedTaskTypes(prev =>
      prev.includes(taskType)
        ? prev.filter(t => t !== taskType)
        : [...prev, taskType]
    )
  }

  function renderSettings() {
    const prefLoading = userPreferencesQuery.isLoading
    const effectivePolicy = effectivePolicyQuery.data
    const isAdmin = user?.roles?.some(r => r === 'Administrator' || r === 'SuperAdministrator')

    return (
      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--spacing-6)' }}>
        {/* Effective Policy Summary */}
        {effectivePolicy && (
          <div className={styles.section}>
            <h3 className={styles.sectionTitle}>Your Effective AI Policy</h3>
            <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
              The resolved combination of organization policy and your personal preferences.
            </p>
            <div className={styles.riskStatsGrid}>
              <div className={styles.statCard}>
                <p className={styles.statValue}>{effectivePolicy.aiEnabled ? 'Enabled' : 'Disabled'}</p>
                <p className={styles.statLabel}>AI Status</p>
              </div>
              <div className={styles.statCard}>
                <p className={styles.statValue}>{effectivePolicy.preferredProvider ?? 'Automatic'}</p>
                <p className={styles.statLabel}>Provider</p>
              </div>
              <div className={styles.statCard}>
                <p className={styles.statValue}>{effectivePolicy.confirmationRequired ? 'Required' : 'Optional'}</p>
                <p className={styles.statLabel}>Confirmation</p>
              </div>
              <div className={styles.statCard}>
                <p className={styles.statValue}>{effectivePolicy.allowedTaskTypes.length > 0 ? effectivePolicy.allowedTaskTypes.length : 'All'}</p>
                <p className={styles.statLabel}>Task Types</p>
              </div>
            </div>

            {/* Usage meters */}
            {effectivePolicy.dailyRequestsLimit != null && (
              <div style={{ marginTop: 'var(--spacing-4)' }}>
                <div className={styles.usageMeter}>
                  <span className={styles.usageMeterLabel}>Daily: {effectivePolicy.dailyRequestsUsed}/{effectivePolicy.dailyRequestsLimit}</span>
                  <div className={styles.usageMeterBar}>
                    <div
                      className={styles.usageMeterFill}
                      style={{
                        width: `${Math.min((effectivePolicy.dailyRequestsUsed / effectivePolicy.dailyRequestsLimit) * 100, 100)}%`,
                        background: effectivePolicy.dailyRequestsUsed >= effectivePolicy.dailyRequestsLimit ? 'var(--color-error)' : 'var(--color-primary)',
                      }}
                    />
                  </div>
                </div>
              </div>
            )}
            {effectivePolicy.monthlyRequestsLimit != null && (
              <div style={{ marginTop: 'var(--spacing-2)' }}>
                <div className={styles.usageMeter}>
                  <span className={styles.usageMeterLabel}>Monthly: {effectivePolicy.monthlyRequestsUsed}/{effectivePolicy.monthlyRequestsLimit}</span>
                  <div className={styles.usageMeterBar}>
                    <div
                      className={styles.usageMeterFill}
                      style={{
                        width: `${Math.min((effectivePolicy.monthlyRequestsUsed / effectivePolicy.monthlyRequestsLimit) * 100, 100)}%`,
                        background: effectivePolicy.monthlyRequestsUsed >= effectivePolicy.monthlyRequestsLimit ? 'var(--color-error)' : 'var(--color-primary)',
                      }}
                    />
                  </div>
                </div>
              </div>
            )}
          </div>
        )}

        {/* User Preferences */}
        <div className={styles.section}>
          <h3 className={styles.sectionTitle}>Your AI Preferences</h3>
          <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
            Personal settings for AI assistance. Organization policies may restrict some options.
          </p>
          {prefLoading ? (
            <div className={styles.loadingSkeleton} style={{ height: 200 }} />
          ) : (
            <div className={styles.settingsForm}>
              <div className={styles.settingsRow}>
                <div className={styles.settingsLabel}>
                  <span className={styles.settingsLabelText}>AI Assistance</span>
                  <span className={styles.settingsLabelHint}>Enable or disable AI-powered suggestions and analysis.</span>
                </div>
                <div className={styles.settingsControl}>
                  <button
                    className={`${styles.toggle} ${prefAiEnabled ? styles.toggleEnabled : ''}`}
                    onClick={() => setPrefAiEnabled(!prefAiEnabled)}
                    title={prefAiEnabled ? 'Disable AI' : 'Enable AI'}
                  />
                </div>
              </div>

              <div className={styles.settingsRow}>
                <div className={styles.settingsLabel}>
                  <span className={styles.settingsLabelText}>Preferred Provider</span>
                  <span className={styles.settingsLabelHint}>Choose your preferred AI provider. Subject to organization policy.</span>
                </div>
                <div className={styles.settingsControl}>
                  <Select
                    label=""
                    value={prefProvider}
                    onChange={(e) => setPrefProvider(e.target.value)}
                    options={providerOptions}
                  />
                </div>
              </div>

              <div className={styles.settingsRow}>
                <div className={styles.settingsLabel}>
                  <span className={styles.settingsLabelText}>Require Confirmation</span>
                  <span className={styles.settingsLabelHint}>Always confirm before applying AI recommendations.</span>
                </div>
                <div className={styles.settingsControl}>
                  <button
                    className={`${styles.toggle} ${prefConfirmation ? styles.toggleEnabled : ''}`}
                    onClick={() => setPrefConfirmation(!prefConfirmation)}
                    title={prefConfirmation ? 'Disable confirmation' : 'Enable confirmation'}
                  />
                </div>
              </div>

              <div className={styles.settingsRow}>
                <div className={styles.settingsLabel}>
                  <span className={styles.settingsLabelText}>Preferred Language</span>
                  <span className={styles.settingsLabelHint}>ISO 639-1 language code for AI responses (e.g., en, de, fr).</span>
                </div>
                <div className={styles.settingsControl}>
                  <Input
                    placeholder="e.g., en"
                    value={prefLanguage}
                    onChange={(e) => setPrefLanguage(e.target.value)}
                    maxLength={10}
                  />
                </div>
              </div>

              <div className={styles.settingsRow}>
                <div className={styles.settingsLabel}>
                  <span className={styles.settingsLabelText}>Enabled Task Types</span>
                  <span className={styles.settingsLabelHint}>Select which AI task types you want enabled. Leave all unchecked for all available tasks.</span>
                </div>
                <div className={styles.checkboxRow}>
                  {allTaskTypes.map(tt => (
                    <label key={tt} className={styles.checkboxItem}>
                      <input
                        type="checkbox"
                        checked={prefTaskTypes.includes(tt)}
                        onChange={() => togglePrefTaskType(tt)}
                      />
                      {taskTypeLabels[tt] ?? tt}
                    </label>
                  ))}
                </div>
              </div>

              <div className={styles.settingsActions}>
                {prefSaved && (
                  <span className={styles.settingsSaved}>
                    <CheckCircle size={14} />
                    Preferences saved
                  </span>
                )}
                <Button
                  variant="primary"
                  onClick={handleSavePreferences}
                  disabled={updatePreferencesMutation.isPending}
                >
                  Save Preferences
                </Button>
              </div>
              {updatePreferencesMutation.isError && (
                <div className={styles.errorBanner}>
                  Failed to save preferences. A selected option may be restricted by organization policy.
                </div>
              )}
            </div>
          )}
        </div>

        {/* Admin: Governance Policy */}
        {isAdmin && (
          <div className={styles.governanceSection}>
            <div className={styles.section}>
              <h3 className={styles.sectionTitle}>Organization AI Governance</h3>
              <p style={{ fontSize: 'var(--font-size-sm)', color: 'var(--color-text-secondary)', marginTop: 0, marginBottom: 'var(--spacing-4)' }}>
                Organization-wide AI controls. These settings override individual user preferences.
              </p>
              {governancePolicyQuery.isLoading ? (
                <div className={styles.loadingSkeleton} style={{ height: 300 }} />
              ) : (
                <div className={styles.settingsForm}>
                  <div className={styles.settingsRow}>
                    <div className={styles.settingsLabel}>
                      <span className={styles.settingsLabelText}>AI Enabled (Organization)</span>
                      <span className={styles.settingsLabelHint}>Master switch. When disabled, no user in this organization can use AI.</span>
                    </div>
                    <div className={styles.settingsControl}>
                      <button
                        className={`${styles.toggle} ${govAiEnabled ? styles.toggleEnabled : ''}`}
                        onClick={() => setGovAiEnabled(!govAiEnabled)}
                        title={govAiEnabled ? 'Disable AI for org' : 'Enable AI for org'}
                      />
                    </div>
                  </div>

                  <div className={styles.settingsRow}>
                    <div className={styles.settingsLabel}>
                      <span className={styles.settingsLabelText}>Allowed Providers</span>
                      <span className={styles.settingsLabelHint}>Restrict which AI providers users can access. Leave all unchecked to allow all.</span>
                    </div>
                    <div className={styles.checkboxRow}>
                      {['Local', 'Claude', 'OpenAi'].map(p => (
                        <label key={p} className={styles.checkboxItem}>
                          <input
                            type="checkbox"
                            checked={govAllowedProviders.includes(p)}
                            onChange={() => toggleGovProvider(p)}
                          />
                          {p}
                        </label>
                      ))}
                    </div>
                  </div>

                  <div className={styles.settingsRow}>
                    <div className={styles.settingsLabel}>
                      <span className={styles.settingsLabelText}>Default Provider</span>
                      <span className={styles.settingsLabelHint}>Organization-wide default provider.</span>
                    </div>
                    <div className={styles.settingsControl}>
                      <Select
                        label=""
                        value={govDefaultProvider}
                        onChange={(e) => setGovDefaultProvider(e.target.value)}
                        options={[{ value: '', label: 'System default' }, { value: 'Local', label: 'Local' }, { value: 'Claude', label: 'Claude' }, { value: 'OpenAi', label: 'OpenAI' }]}
                      />
                    </div>
                  </div>

                  <div className={styles.settingsRow}>
                    <div className={styles.settingsLabel}>
                      <span className={styles.settingsLabelText}>Allowed Task Types</span>
                      <span className={styles.settingsLabelHint}>Restrict AI tasks. Leave all unchecked to allow all.</span>
                    </div>
                    <div className={styles.checkboxRow}>
                      {allTaskTypes.map(tt => (
                        <label key={tt} className={styles.checkboxItem}>
                          <input
                            type="checkbox"
                            checked={govAllowedTaskTypes.includes(tt)}
                            onChange={() => toggleGovTaskType(tt)}
                          />
                          {taskTypeLabels[tt] ?? tt}
                        </label>
                      ))}
                    </div>
                  </div>

                  <div className={styles.settingsRow}>
                    <div className={styles.settingsLabel}>
                      <span className={styles.settingsLabelText}>Daily Limit per User</span>
                      <span className={styles.settingsLabelHint}>Maximum AI requests per user per day. Leave empty for unlimited.</span>
                    </div>
                    <div className={styles.settingsControl}>
                      <Input
                        type="number"
                        placeholder="Unlimited"
                        value={govDailyLimit}
                        onChange={(e) => setGovDailyLimit(e.target.value)}
                        min={1}
                      />
                    </div>
                  </div>

                  <div className={styles.settingsRow}>
                    <div className={styles.settingsLabel}>
                      <span className={styles.settingsLabelText}>Monthly Limit (Organization)</span>
                      <span className={styles.settingsLabelHint}>Maximum total AI requests per month. Leave empty for unlimited.</span>
                    </div>
                    <div className={styles.settingsControl}>
                      <Input
                        type="number"
                        placeholder="Unlimited"
                        value={govMonthlyLimit}
                        onChange={(e) => setGovMonthlyLimit(e.target.value)}
                        min={1}
                      />
                    </div>
                  </div>

                  <div className={styles.settingsRow}>
                    <div className={styles.settingsLabel}>
                      <span className={styles.settingsLabelText}>Human Confirmation</span>
                      <span className={styles.settingsLabelHint}>Always required for QMS modifications. This cannot be disabled.</span>
                    </div>
                    <div className={styles.settingsControl}>
                      <button
                        className={`${styles.toggle} ${styles.toggleEnabled}`}
                        disabled
                        title="Human confirmation is always required"
                      />
                    </div>
                  </div>

                  <div className={styles.settingsActions}>
                    {govSaved && (
                      <span className={styles.settingsSaved}>
                        <CheckCircle size={14} />
                        Governance policy saved
                      </span>
                    )}
                    <Button
                      variant="primary"
                      onClick={handleSaveGovernance}
                      disabled={updateGovernanceMutation.isPending}
                    >
                      Save Governance Policy
                    </Button>
                  </div>
                  {updateGovernanceMutation.isError && (
                    <div className={styles.errorBanner}>
                      Failed to save governance policy. Verify your settings and try again.
                    </div>
                  )}
                </div>
              )}
            </div>
          </div>
        )}
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
        <button className={`${styles.tab} ${activeTab === 'aiInsights' ? styles.tabActive : ''}`} onClick={() => setActiveTab('aiInsights')}>
          <TrendingUp size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          AI Insights
        </button>
        <button className={`${styles.tab} ${activeTab === 'providers' ? styles.tabActive : ''}`} onClick={() => setActiveTab('providers')}>
          <Server size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          Providers
        </button>
        <button className={`${styles.tab} ${activeTab === 'settings' ? styles.tabActive : ''}`} onClick={() => setActiveTab('settings')}>
          <Settings size={14} style={{ marginRight: 6, verticalAlign: 'middle' }} />
          Settings
        </button>
      </div>

      {activeTab === 'capabilities' && renderCapabilities()}
      {activeTab === 'interactions' && renderInteractions()}
      {activeTab === 'models' && renderModels()}
      {activeTab === 'permissions' && renderPermissions()}
      {activeTab === 'actionLog' && renderActionLog()}
      {activeTab === 'workflows' && renderWorkflows()}
      {activeTab === 'knowledgeContext' && renderKnowledgeContext()}
      {activeTab === 'aiInsights' && renderAiInsights()}
      {activeTab === 'providers' && renderProviders()}
      {activeTab === 'settings' && renderSettings()}

      {renderConfirmationDialog()}
    </div>
  )
}
