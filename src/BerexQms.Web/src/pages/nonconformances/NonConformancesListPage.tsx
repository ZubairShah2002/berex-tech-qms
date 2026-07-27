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
import styles from './NonConformancesListPage.module.css'

interface NonConformanceDto {
  id: string
  ncrNumber: string
  status: string
  severity: string
  source: string
  detectionPoint: string
  partId: string
  lotNumber: string | null
  supplierId: string | null
  quantityAffected: number
  quantityDefective: number
  assignedTo: string | null
  createdAt: string
}

interface PagedResult {
  items: NonConformanceDto[]
  totalCount: number
  page: number
  pageSize: number
}

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Open', label: 'Open' },
  { value: 'UnderInvestigation', label: 'Under Investigation' },
  { value: 'PendingDisposition', label: 'Pending Disposition' },
  { value: 'Closed', label: 'Closed' },
  { value: 'Reopened', label: 'Reopened' },
]

const severityOptions = [
  { value: '', label: 'All severities' },
  { value: 'Minor', label: 'Minor' },
  { value: 'Major', label: 'Major' },
  { value: 'Critical', label: 'Critical' },
]

export function NonConformancesListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [severity, setSeverity] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, isError } = useQuery<PagedResult>({
    queryKey: ['nonconformances', search, status, severity, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (status) params.set('status', status)
      if (severity) params.set('severity', severity)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/non-conformances?${params}`)
      return res.data
    },
  })

  const handleSearch = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(1)
  }, [])

  const columns = [
    {
      key: 'ncrNumber',
      header: 'NCR #',
      sortable: true,
      width: '140px',
      render: (row: Record<string, unknown>) => {
        const nc = row as unknown as NonConformanceDto
        return (
          <button
            className={styles.linkButton}
            onClick={() => navigate(`/nonconformances/${nc.id}`)}
          >
            {nc.ncrNumber}
          </button>
        )
      },
    },
    {
      key: 'status',
      header: 'Status',
      width: '160px',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={(row as unknown as NonConformanceDto).status} />
      ),
    },
    {
      key: 'severity',
      header: 'Severity',
      width: '100px',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={(row as unknown as NonConformanceDto).severity} />
      ),
    },
    {
      key: 'source',
      header: 'Source',
      width: '140px',
    },
    {
      key: 'quantityDefective',
      header: 'Defective',
      width: '90px',
      render: (row: Record<string, unknown>) => {
        const nc = row as unknown as NonConformanceDto
        return `${nc.quantityDefective}/${nc.quantityAffected}`
      },
    },
    {
      key: 'assignedTo',
      header: 'Assigned To',
      width: '140px',
      render: (row: Record<string, unknown>) =>
        (row as unknown as NonConformanceDto).assignedTo ?? '—',
    },
    {
      key: 'createdAt',
      header: 'Created',
      width: '110px',
      render: (row: Record<string, unknown>) =>
        new Date((row as unknown as NonConformanceDto).createdAt).toLocaleDateString(),
    },
  ]

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Non-Conformances</h1>
          <p className={styles.subtitle}>
            Track and manage non-conformance records through investigation and disposition
          </p>
        </div>
        <Button icon={<Plus size={16} />} onClick={() => navigate('/nonconformances/new')}>
          New NCR
        </Button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search by NCR number..."
            value={search}
            onChange={handleSearch}
            className={styles.searchInput}
          />
        </div>
        <Select
          options={statusOptions}
          value={status}
          onChange={(e) => {
            setStatus(e.target.value)
            setPage(1)
          }}
          className={styles.statusFilter}
        />
        <Select
          options={severityOptions}
          value={severity}
          onChange={(e) => {
            setSeverity(e.target.value)
            setPage(1)
          }}
          className={styles.severityFilter}
        />
      </div>

      {isError && (
        <div className={styles.errorBanner}>
          Failed to load non-conformances. Please try again.
        </div>
      )}

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(row) => (row as unknown as NonConformanceDto).id}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        emptyTitle={isLoading ? 'Loading...' : 'No non-conformances found'}
        emptyDescription={
          isLoading
            ? 'Please wait while records are loaded.'
            : 'Get started by creating your first NCR.'
        }
      />
    </div>
  )
}
