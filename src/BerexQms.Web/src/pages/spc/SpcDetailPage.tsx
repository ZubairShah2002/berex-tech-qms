import { useState, useMemo } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import { ArrowLeft, Plus, RefreshCw, AlertTriangle } from 'lucide-react'
import { apiClient } from '@/lib/api-client'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { StatusBadge, Badge } from '@/components/ui/Badge'
import styles from './SpcDetailPage.module.css'

interface ControlLimitsDto {
  upperControlLimit: number
  centerLine: number
  lowerControlLimit: number
  upperSpecLimit: number | null
  lowerSpecLimit: number | null
}

interface ProcessCapabilityDto {
  cp: number
  cpk: number
  pp: number
  ppk: number
  mean: number
  stdDev: number
  sampleSize: number
  calculatedAt: string
}

interface DataPointDto {
  id: string
  value: number
  subgroupValues: string | null
  sampleSize: number
  timestamp: string
  inspectionId: string | null
  ruleViolation: string | null
  isOutOfControl: boolean
}

interface ChartDetail {
  id: string
  code: string
  name: string
  chartType: string
  partId: string
  characteristicName: string
  subgroupSize: number
  status: string
  isActive: boolean
  controlLimits: ControlLimitsDto | null
  processCapability: ProcessCapabilityDto | null
  upperSpecLimit: number | null
  lowerSpecLimit: number | null
  dataPoints: DataPointDto[]
  createdAt: string
}

type Tab = 'chart' | 'data' | 'capability'

const chartTypeLabels: Record<string, string> = {
  XBarR: 'X̄/R',
  XBarS: 'X̄/S',
  IndividualMovingRange: 'I/MR',
  PChart: 'p Chart',
  NpChart: 'np Chart',
  CChart: 'c Chart',
  UChart: 'u Chart',
}

function getCapabilityClass(value: number): string {
  if (value >= 1.33) return styles.capGood
  if (value >= 1.0) return styles.capMarginal
  return styles.capPoor
}

function formatRuleViolation(rule: string | null): string {
  if (!rule) return ''
  const map: Record<string, string> = {
    Rule1_BeyondThreeSigma: 'Rule 1 — Beyond 3σ',
    Rule2_TwoOfThreeBeyondTwoSigma: 'Rule 2 — 2 of 3 beyond 2σ',
    Rule3_FourOfFiveBeyondOneSigma: 'Rule 3 — 4 of 5 beyond 1σ',
    Rule4_EightConsecutiveOneSide: 'Rule 4 — 8 consecutive same side',
    Rule5_SixConsecutiveIncreasingDecreasing: 'Rule 5 — 6 trending',
    Rule6_FourteenAlternating: 'Rule 6 — 14 alternating',
    Rule7_FifteenWithinOneSigma: 'Rule 7 — 15 within 1σ',
    Rule8_EightBeyondOneSigmaBothSides: 'Rule 8 — 8 beyond 1σ both sides',
  }
  return map[rule] ?? rule
}

function ControlChartSvg({ dataPoints, limits }: {
  dataPoints: DataPointDto[]
  limits: ControlLimitsDto | null
}) {
  const sorted = useMemo(
    () => [...dataPoints].sort((a, b) => new Date(a.timestamp).getTime() - new Date(b.timestamp).getTime()),
    [dataPoints]
  )

  if (sorted.length === 0) {
    return (
      <div className={styles.chartContainer} style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--color-text-tertiary)' }}>
        No data points yet. Add measurements to see the control chart.
      </div>
    )
  }

  const values = sorted.map(p => p.value)
  const allValues = [...values]
  if (limits) {
    allValues.push(limits.upperControlLimit, limits.lowerControlLimit, limits.centerLine)
  }

  const minVal = Math.min(...allValues)
  const maxVal = Math.max(...allValues)
  const range = maxVal - minVal || 1
  const padding = range * 0.1

  const width = 800
  const height = 280
  const marginLeft = 60
  const marginRight = 20
  const marginTop = 20
  const marginBottom = 30
  const plotWidth = width - marginLeft - marginRight
  const plotHeight = height - marginTop - marginBottom

  const scaleX = (i: number) => marginLeft + (i / Math.max(sorted.length - 1, 1)) * plotWidth
  const scaleY = (v: number) => marginTop + plotHeight - ((v - (minVal - padding)) / (range + 2 * padding)) * plotHeight

  const linePath = sorted.map((p, i) => `${i === 0 ? 'M' : 'L'}${scaleX(i).toFixed(1)},${scaleY(p.value).toFixed(1)}`).join(' ')

  return (
    <div className={styles.chartContainer}>
      <svg className={styles.chartSvg} viewBox={`0 0 ${width} ${height}`} preserveAspectRatio="xMidYMid meet">
        {/* Y-axis labels */}
        {[0, 0.25, 0.5, 0.75, 1].map(frac => {
          const val = (minVal - padding) + frac * (range + 2 * padding)
          const y = scaleY(val)
          return (
            <g key={frac}>
              <line x1={marginLeft} y1={y} x2={width - marginRight} y2={y} stroke="var(--color-border-subtle)" strokeWidth="0.5" />
              <text x={marginLeft - 8} y={y + 4} textAnchor="end" fill="var(--color-text-tertiary)" fontSize="10">
                {val.toFixed(2)}
              </text>
            </g>
          )
        })}

        {/* Control limit lines */}
        {limits && (
          <>
            <line
              x1={marginLeft} y1={scaleY(limits.upperControlLimit)}
              x2={width - marginRight} y2={scaleY(limits.upperControlLimit)}
              stroke="var(--color-error)" strokeWidth="1.5" strokeDasharray="6,3"
            />
            <text x={width - marginRight + 4} y={scaleY(limits.upperControlLimit) + 4} fill="var(--color-error)" fontSize="9" fontWeight="500">
              UCL
            </text>
            <line
              x1={marginLeft} y1={scaleY(limits.centerLine)}
              x2={width - marginRight} y2={scaleY(limits.centerLine)}
              stroke="var(--color-primary)" strokeWidth="1.5"
            />
            <text x={width - marginRight + 4} y={scaleY(limits.centerLine) + 4} fill="var(--color-primary)" fontSize="9" fontWeight="500">
              CL
            </text>
            <line
              x1={marginLeft} y1={scaleY(limits.lowerControlLimit)}
              x2={width - marginRight} y2={scaleY(limits.lowerControlLimit)}
              stroke="var(--color-error)" strokeWidth="1.5" strokeDasharray="6,3"
            />
            <text x={width - marginRight + 4} y={scaleY(limits.lowerControlLimit) + 4} fill="var(--color-error)" fontSize="9" fontWeight="500">
              LCL
            </text>
          </>
        )}

        {/* Data line */}
        <path d={linePath} fill="none" stroke="var(--color-text-primary)" strokeWidth="1.5" />

        {/* Data points */}
        {sorted.map((p, i) => (
          <circle
            key={p.id}
            cx={scaleX(i)}
            cy={scaleY(p.value)}
            r={p.isOutOfControl ? 5 : 3}
            fill={p.isOutOfControl ? 'var(--color-error)' : 'var(--color-primary)'}
            stroke={p.isOutOfControl ? 'var(--color-error)' : 'var(--color-primary)'}
            strokeWidth="1"
          >
            <title>
              {`Value: ${p.value.toFixed(4)}\nTime: ${new Date(p.timestamp).toLocaleString()}${p.isOutOfControl ? `\n⚠ ${formatRuleViolation(p.ruleViolation)}` : ''}`}
            </title>
          </circle>
        ))}

        {/* X-axis */}
        <line x1={marginLeft} y1={height - marginBottom} x2={width - marginRight} y2={height - marginBottom} stroke="var(--color-border)" strokeWidth="1" />

        {/* X-axis labels (show a few) */}
        {sorted.filter((_, i) => sorted.length <= 10 || i % Math.ceil(sorted.length / 8) === 0 || i === sorted.length - 1).map((p, _, arr) => {
          const idx = sorted.indexOf(p)
          return (
            <text
              key={p.id}
              x={scaleX(idx)}
              y={height - marginBottom + 16}
              textAnchor={idx === arr.length - 1 ? 'end' : 'middle'}
              fill="var(--color-text-tertiary)"
              fontSize="9"
            >
              {new Date(p.timestamp).toLocaleDateString(undefined, { month: 'short', day: 'numeric' })}
            </text>
          )
        })}
      </svg>
    </div>
  )
}

export function SpcDetailPage() {
  const navigate = useNavigate()
  const { id } = useParams()
  const queryClient = useQueryClient()
  const [tab, setTab] = useState<Tab>('chart')
  const [actionError, setActionError] = useState('')
  const [showAddForm, setShowAddForm] = useState(false)

  const [addForm, setAddForm] = useState({
    value: '',
    subgroupValues: '',
    sampleSize: '1',
    timestamp: new Date().toISOString().slice(0, 16),
  })

  const { data: chart, isLoading } = useQuery<ChartDetail>({
    queryKey: ['spc-chart', id],
    queryFn: async () => {
      const res = await apiClient.get(`/api/v1/spc/charts/${id}`)
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
    queryClient.invalidateQueries({ queryKey: ['spc-chart', id] })
    queryClient.invalidateQueries({ queryKey: ['spc-charts'] })
  }

  const addPointMutation = useMutation({
    mutationFn: () => apiClient.post(`/api/v1/spc/charts/${id}/data-points`, {
      value: Number(addForm.value),
      subgroupValues: addForm.subgroupValues || null,
      sampleSize: Number(addForm.sampleSize),
      timestamp: addForm.timestamp,
      inspectionId: null,
    }),
    onSuccess: () => {
      invalidate()
      setShowAddForm(false)
      setAddForm({ value: '', subgroupValues: '', sampleSize: '1', timestamp: new Date().toISOString().slice(0, 16) })
    },
    onError: handleError,
  })

  const recalcMutation = useMutation({
    mutationFn: () => apiClient.post(`/api/v1/spc/charts/${id}/recalculate`),
    onSuccess: invalidate,
    onError: handleError,
  })

  const deactivateMutation = useMutation({
    mutationFn: () => apiClient.post(`/api/v1/spc/charts/${id}/deactivate`),
    onSuccess: invalidate,
    onError: handleError,
  })

  if (isLoading || !chart) {
    return <div className={styles.page}>Loading...</div>
  }

  const oocCount = chart.dataPoints.filter(p => p.isOutOfControl).length
  const sortedPoints = [...chart.dataPoints].sort(
    (a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
  )

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <button
          type="button"
          className={styles.backButton}
          onClick={() => navigate('/spc')}
        >
          <ArrowLeft size={16} />
        </button>
        <div className={styles.headerInfo}>
          <div className={styles.titleRow}>
            <h1 className={styles.title}>{chart.code} — {chart.name}</h1>
            <StatusBadge status={chart.status} />
          </div>
          <div className={styles.meta}>
            <span><span className={styles.metaLabel}>Type: </span>{chartTypeLabels[chart.chartType] ?? chart.chartType}</span>
            <span><span className={styles.metaLabel}>Characteristic: </span>{chart.characteristicName}</span>
            <span><span className={styles.metaLabel}>Created: </span>{new Date(chart.createdAt).toLocaleDateString()}</span>
          </div>
        </div>
        <div className={styles.headerActions}>
          {chart.isActive && (
            <>
              <Button variant="secondary" onClick={() => setShowAddForm(!showAddForm)}>
                <Plus size={14} /> Add Point
              </Button>
              <Button variant="secondary" onClick={() => recalcMutation.mutate()} disabled={recalcMutation.isPending}>
                <RefreshCw size={14} /> Recalculate
              </Button>
            </>
          )}
        </div>
      </div>

      {actionError && <div className={styles.errorBanner}>{actionError}</div>}

      <div className={styles.infoGrid}>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Subgroup Size</span>
          <span className={styles.infoValue}>{chart.subgroupSize}</span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Data Points</span>
          <span className={styles.infoValue}>{chart.dataPoints.length}</span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Out of Control</span>
          <span className={styles.infoValue} style={{ color: oocCount > 0 ? 'var(--color-error)' : undefined }}>
            {oocCount}
          </span>
        </div>
        <div className={styles.infoItem}>
          <span className={styles.infoLabel}>Spec Limits</span>
          <span className={styles.infoValue}>
            {chart.upperSpecLimit != null || chart.lowerSpecLimit != null
              ? `${chart.lowerSpecLimit ?? '—'} / ${chart.upperSpecLimit ?? '—'}`
              : '—'
            }
          </span>
        </div>
        {chart.controlLimits && (
          <>
            <div className={styles.infoItem}>
              <span className={styles.infoLabel}>UCL</span>
              <span className={styles.infoValue}>{chart.controlLimits.upperControlLimit.toFixed(4)}</span>
            </div>
            <div className={styles.infoItem}>
              <span className={styles.infoLabel}>Center Line</span>
              <span className={styles.infoValue}>{chart.controlLimits.centerLine.toFixed(4)}</span>
            </div>
            <div className={styles.infoItem}>
              <span className={styles.infoLabel}>LCL</span>
              <span className={styles.infoValue}>{chart.controlLimits.lowerControlLimit.toFixed(4)}</span>
            </div>
            <div className={styles.infoItem}>
              <span className={styles.infoLabel}>Status</span>
              <span className={styles.infoValue}>
                {chart.isActive ? 'Active' : 'Inactive'}
              </span>
            </div>
          </>
        )}
      </div>

      {/* Capability indices */}
      {chart.processCapability && (
        <div className={styles.capabilityGrid}>
          <div className={styles.capabilityItem}>
            <div className={styles.capabilityLabel}>Cp</div>
            <div className={`${styles.capabilityValue} ${getCapabilityClass(chart.processCapability.cp)}`}>
              {chart.processCapability.cp.toFixed(2)}
            </div>
          </div>
          <div className={styles.capabilityItem}>
            <div className={styles.capabilityLabel}>Cpk</div>
            <div className={`${styles.capabilityValue} ${getCapabilityClass(chart.processCapability.cpk)}`}>
              {chart.processCapability.cpk.toFixed(2)}
            </div>
          </div>
          <div className={styles.capabilityItem}>
            <div className={styles.capabilityLabel}>Pp</div>
            <div className={`${styles.capabilityValue} ${getCapabilityClass(chart.processCapability.pp)}`}>
              {chart.processCapability.pp.toFixed(2)}
            </div>
          </div>
          <div className={styles.capabilityItem}>
            <div className={styles.capabilityLabel}>Ppk</div>
            <div className={`${styles.capabilityValue} ${getCapabilityClass(chart.processCapability.ppk)}`}>
              {chart.processCapability.ppk.toFixed(2)}
            </div>
          </div>
        </div>
      )}

      {/* Add data point form */}
      {showAddForm && (
        <div className={styles.addForm}>
          <h3 className={styles.addFormTitle}>Add Data Point</h3>
          <div className={styles.addFormGrid}>
            <Input
              label="Value"
              type="number"
              step="any"
              value={addForm.value}
              onChange={(e) => setAddForm(prev => ({ ...prev, value: e.target.value }))}
              required
              placeholder="Measured value"
            />
            <Input
              label="Sample Size"
              type="number"
              value={addForm.sampleSize}
              onChange={(e) => setAddForm(prev => ({ ...prev, sampleSize: e.target.value }))}
              min={1}
              required
            />
            <Input
              label="Timestamp"
              type="datetime-local"
              value={addForm.timestamp}
              onChange={(e) => setAddForm(prev => ({ ...prev, timestamp: e.target.value }))}
              required
            />
          </div>
          <Input
            label="Subgroup Values (comma-separated)"
            value={addForm.subgroupValues}
            onChange={(e) => setAddForm(prev => ({ ...prev, subgroupValues: e.target.value }))}
            placeholder="e.g., 10.1, 10.3, 9.8, 10.0, 10.2"
            style={{ marginTop: 'var(--spacing-4)' }}
          />
          <div className={styles.addFormActions}>
            <Button variant="secondary" type="button" onClick={() => setShowAddForm(false)}>
              Cancel
            </Button>
            <Button
              onClick={() => addPointMutation.mutate()}
              disabled={addPointMutation.isPending || !addForm.value}
            >
              {addPointMutation.isPending ? 'Adding...' : 'Add Point'}
            </Button>
          </div>
        </div>
      )}

      {/* Tabs */}
      <div className={styles.tabs}>
        <button
          type="button"
          className={`${styles.tab} ${tab === 'chart' ? styles.tabActive : ''}`}
          onClick={() => setTab('chart')}
        >
          Control Chart
        </button>
        <button
          type="button"
          className={`${styles.tab} ${tab === 'data' ? styles.tabActive : ''}`}
          onClick={() => setTab('data')}
        >
          Data Points ({chart.dataPoints.length})
        </button>
        <button
          type="button"
          className={`${styles.tab} ${tab === 'capability' ? styles.tabActive : ''}`}
          onClick={() => setTab('capability')}
        >
          Capability
        </button>
      </div>

      {/* Tab content */}
      {tab === 'chart' && (
        <div className={styles.chartSection}>
          <div className={styles.chartHeader}>
            <h3 className={styles.chartTitle}>
              {chartTypeLabels[chart.chartType] ?? chart.chartType} Control Chart
            </h3>
            {oocCount > 0 && (
              <Badge variant="error" dot>
                <AlertTriangle size={12} /> {oocCount} out-of-control point{oocCount !== 1 ? 's' : ''}
              </Badge>
            )}
          </div>
          <ControlChartSvg dataPoints={chart.dataPoints} limits={chart.controlLimits} />
        </div>
      )}

      {tab === 'data' && (
        <div className={styles.pointsSection}>
          <div className={styles.sectionHeader}>
            <h3 className={styles.sectionTitle}>Data Points</h3>
          </div>
          {sortedPoints.length === 0 ? (
            <p style={{ color: 'var(--color-text-tertiary)', fontSize: 'var(--font-size-sm)' }}>
              No data points recorded yet.
            </p>
          ) : (
            <div style={{ overflowX: 'auto' }}>
              <table className={styles.pointsTable}>
                <thead>
                  <tr>
                    <th>#</th>
                    <th>Value</th>
                    <th>Timestamp</th>
                    <th>Sample Size</th>
                    <th>Status</th>
                    <th>Rule Violation</th>
                  </tr>
                </thead>
                <tbody>
                  {sortedPoints.map((point, idx) => (
                    <tr key={point.id} className={point.isOutOfControl ? styles.oocRow : undefined}>
                      <td>{sortedPoints.length - idx}</td>
                      <td>{point.value.toFixed(4)}</td>
                      <td>{new Date(point.timestamp).toLocaleString()}</td>
                      <td>{point.sampleSize}</td>
                      <td>
                        {point.isOutOfControl ? (
                          <span className={styles.oocBadge}>
                            <AlertTriangle size={12} /> Out of Control
                          </span>
                        ) : (
                          <span style={{ color: 'var(--color-success)', fontWeight: 500, fontSize: 'var(--font-size-xs)' }}>In Control</span>
                        )}
                      </td>
                      <td>{point.ruleViolation ? formatRuleViolation(point.ruleViolation) : '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      )}

      {tab === 'capability' && (
        <div className={styles.pointsSection}>
          <div className={styles.sectionHeader}>
            <h3 className={styles.sectionTitle}>Process Capability Analysis</h3>
          </div>
          {chart.processCapability ? (
            <div>
              <div className={styles.capabilityGrid} style={{ marginBottom: 'var(--spacing-4)' }}>
                <div className={styles.capabilityItem}>
                  <div className={styles.capabilityLabel}>Cp</div>
                  <div className={`${styles.capabilityValue} ${getCapabilityClass(chart.processCapability.cp)}`}>
                    {chart.processCapability.cp.toFixed(4)}
                  </div>
                  <div style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-tertiary)' }}>
                    Short-term potential
                  </div>
                </div>
                <div className={styles.capabilityItem}>
                  <div className={styles.capabilityLabel}>Cpk</div>
                  <div className={`${styles.capabilityValue} ${getCapabilityClass(chart.processCapability.cpk)}`}>
                    {chart.processCapability.cpk.toFixed(4)}
                  </div>
                  <div style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-tertiary)' }}>
                    Short-term centering
                  </div>
                </div>
                <div className={styles.capabilityItem}>
                  <div className={styles.capabilityLabel}>Pp</div>
                  <div className={`${styles.capabilityValue} ${getCapabilityClass(chart.processCapability.pp)}`}>
                    {chart.processCapability.pp.toFixed(4)}
                  </div>
                  <div style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-tertiary)' }}>
                    Long-term potential
                  </div>
                </div>
                <div className={styles.capabilityItem}>
                  <div className={styles.capabilityLabel}>Ppk</div>
                  <div className={`${styles.capabilityValue} ${getCapabilityClass(chart.processCapability.ppk)}`}>
                    {chart.processCapability.ppk.toFixed(4)}
                  </div>
                  <div style={{ fontSize: 'var(--font-size-xs)', color: 'var(--color-text-tertiary)' }}>
                    Long-term centering
                  </div>
                </div>
              </div>
              <div className={styles.infoGrid}>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Mean</span>
                  <span className={styles.infoValue}>{chart.processCapability.mean.toFixed(4)}</span>
                </div>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Std Dev</span>
                  <span className={styles.infoValue}>{chart.processCapability.stdDev.toFixed(6)}</span>
                </div>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Sample Size</span>
                  <span className={styles.infoValue}>{chart.processCapability.sampleSize}</span>
                </div>
                <div className={styles.infoItem}>
                  <span className={styles.infoLabel}>Calculated</span>
                  <span className={styles.infoValue}>{new Date(chart.processCapability.calculatedAt).toLocaleString()}</span>
                </div>
              </div>
            </div>
          ) : (
            <div style={{ color: 'var(--color-text-tertiary)', fontSize: 'var(--font-size-sm)' }}>
              <p>Process capability has not been calculated yet.</p>
              <p style={{ marginTop: 'var(--spacing-2)' }}>
                Requirements: At least 25 data points and specification limits (USL/LSL) must be defined.
                Click &quot;Recalculate&quot; to compute capability indices.
              </p>
            </div>
          )}
        </div>
      )}

      {/* Deactivate action */}
      {chart.isActive && (
        <div style={{ display: 'flex', justifyContent: 'flex-end', paddingTop: 'var(--spacing-4)', borderTop: '1px solid var(--color-border)' }}>
          <Button
            variant="secondary"
            onClick={() => { if (window.confirm('Deactivate this control chart?')) deactivateMutation.mutate() }}
            disabled={deactivateMutation.isPending}
          >
            Deactivate Chart
          </Button>
        </div>
      )}
    </div>
  )
}
