import { useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, Search } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { StatusBadge } from '@/components/ui/Badge'
import styles from './SpcListPage.module.css'

interface ControlLimitsDto {
  upperControlLimit: number
  centerLine: number
  lowerControlLimit: number
}

interface ProcessCapabilityDto {
  cp: number
  cpk: number
  pp: number
  ppk: number
}

interface ControlChartDto {
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
  dataPointCount: number
  createdAt: string
}

interface PagedResult {
  items: ControlChartDto[]
  totalCount: number
  page: number
  pageSize: number
}

const chartTypeOptions = [
  { value: '', label: 'All chart types' },
  { value: 'XBarR', label: 'X̄/R' },
  { value: 'XBarS', label: 'X̄/S' },
  { value: 'IndividualMovingRange', label: 'I/MR' },
  { value: 'PChart', label: 'p Chart' },
  { value: 'NpChart', label: 'np Chart' },
  { value: 'CChart', label: 'c Chart' },
  { value: 'UChart', label: 'u Chart' },
]

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
  { value: 'UnderReview', label: 'Under Review' },
]

const chartTypeLabels: Record<string, string> = {
  XBarR: 'X̄/R',
  XBarS: 'X̄/S',
  IndividualMovingRange: 'I/MR',
  PChart: 'p',
  NpChart: 'np',
  CChart: 'c',
  UChart: 'u',
}

export function SpcListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [chartType, setChartType] = useState('')
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, error } = useQuery<PagedResult>({
    queryKey: ['spc-charts', search, chartType, status, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (chartType) params.set('chartType', chartType)
      if (status) params.set('status', status)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/api/v1/spc/charts?${params}`)
      return res.data
    },
  })

  const columns = [
    {
      key: 'code' as const,
      header: 'Code',
      sortable: true,
      render: (row: Record<string, unknown>) => (
        <button
          type="button"
          className={styles.linkButton}
          onClick={() => navigate(`/spc/${row.id as string}`)}
        >
          {row.code as string}
        </button>
      ),
    },
    { key: 'name' as const, header: 'Name', sortable: true },
    {
      key: 'chartType' as const,
      header: 'Type',
      render: (row: Record<string, unknown>) =>
        chartTypeLabels[row.chartType as string] ?? (row.chartType as string),
    },
    { key: 'characteristicName' as const, header: 'Characteristic' },
    {
      key: 'status' as const,
      header: 'Status',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={row.status as string} />
      ),
    },
    {
      key: 'dataPointCount' as const,
      header: 'Points',
      render: (row: Record<string, unknown>) => String(row.dataPointCount ?? 0),
    },
    {
      key: 'processCapability' as const,
      header: 'Cpk',
      render: (row: Record<string, unknown>) => {
        const cap = row.processCapability as ProcessCapabilityDto | null
        if (!cap) return '—'
        return <span className={styles.capValue}>{cap.cpk.toFixed(2)}</span>
      },
    },
    {
      key: 'createdAt' as const,
      header: 'Created',
      render: (row: Record<string, unknown>) =>
        new Date(row.createdAt as string).toLocaleDateString(),
    },
  ]

  const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(1)
  }, [])

  const handleChartTypeChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    setChartType(e.target.value)
    setPage(1)
  }, [])

  const handleStatusChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    setStatus(e.target.value)
    setPage(1)
  }, [])

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Control Charts</h1>
          <p className={styles.subtitle}>
            Statistical process control charts, capability analysis, and trend detection
          </p>
        </div>
        <Button onClick={() => navigate('/spc/new')}>
          <Plus size={16} /> New Chart
        </Button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search by code or name..."
            value={search}
            onChange={handleSearchChange}
            className={styles.searchInput}
          />
        </div>
        <Select
          value={chartType}
          onChange={handleChartTypeChange}
          className={styles.statusFilter}
          options={chartTypeOptions}
        />
        <Select
          value={status}
          onChange={handleStatusChange}
          className={styles.statusFilter}
          options={statusOptions}
        />
      </div>

      {error && (
        <div className={styles.errorBanner}>
          Failed to load control charts. Please try again.
        </div>
      )}

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(row) => row.id as string}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        emptyTitle={isLoading ? 'Loading...' : 'No control charts found'}
        emptyDescription="Create a control chart to start monitoring process stability."
      />
    </div>
  )
}
