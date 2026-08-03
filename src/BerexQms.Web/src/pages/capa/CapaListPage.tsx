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
import styles from './CapaListPage.module.css'

interface CAPADto {
  id: string
  capaNumber: string
  title: string
  status: string
  priority: string
  sourceType: string
  ownerId: string
  assignedTo: string | null
  sourceNonConformanceId: string | null
  targetClosureDate: string | null
  actionCount: number
  completedActionCount: number
  createdAt: string
}

interface PagedResult {
  items: CAPADto[]
  totalCount: number
  page: number
  pageSize: number
}

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Initiated', label: 'Initiated' },
  { value: 'RCAInProgress', label: 'RCA In Progress' },
  { value: 'ActionPlanning', label: 'Action Planning' },
  { value: 'Implementation', label: 'Implementation' },
  { value: 'PendingVerification', label: 'Pending Verification' },
  { value: 'ClosedEffective', label: 'Closed (Effective)' },
  { value: 'ClosedIneffective', label: 'Closed (Ineffective)' },
]

const priorityOptions = [
  { value: '', label: 'All priorities' },
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' },
]

export function CapaListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [priority, setPriority] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, isError } = useQuery<PagedResult>({
    queryKey: ['capas', search, status, priority, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (status) params.set('status', status)
      if (priority) params.set('priority', priority)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/capas?${params}`)
      return res.data
    },
  })

  const handleSearch = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(1)
  }, [])

  const columns = [
    {
      key: 'capaNumber',
      header: 'CAPA #',
      sortable: true,
      width: '120px',
      render: (row: Record<string, unknown>) => {
        const capa = row as unknown as CAPADto
        return (
          <button
            className={styles.linkButton}
            onClick={() => navigate(`/capa/${capa.id}`)}
          >
            {capa.capaNumber}
          </button>
        )
      },
    },
    {
      key: 'title',
      header: 'Title',
      render: (row: Record<string, unknown>) => (row as unknown as CAPADto).title,
    },
    {
      key: 'status',
      header: 'Status',
      width: '160px',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={(row as unknown as CAPADto).status} />
      ),
    },
    {
      key: 'priority',
      header: 'Priority',
      width: '100px',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={(row as unknown as CAPADto).priority} />
      ),
    },
    {
      key: 'sourceType',
      header: 'Source',
      width: '140px',
    },
    {
      key: 'actions',
      header: 'Actions',
      width: '120px',
      render: (row: Record<string, unknown>) => {
        const capa = row as unknown as CAPADto
        if (capa.actionCount === 0) return '—'
        const maxSegments = Math.min(capa.actionCount, 8)
        const filled = Math.round((capa.completedActionCount / capa.actionCount) * maxSegments)
        return (
          <div className={styles.progressBar}>
            {Array.from({ length: maxSegments }).map((_, i) => (
              <div
                key={i}
                className={`${styles.progressSegment} ${i < filled ? styles.progressSegmentFilled : ''}`}
              />
            ))}
            <span className={styles.progressText}>
              {capa.completedActionCount}/{capa.actionCount}
            </span>
          </div>
        )
      },
    },
    {
      key: 'ownerId',
      header: 'Owner',
      width: '120px',
    },
    {
      key: 'createdAt',
      header: 'Created',
      width: '100px',
      render: (row: Record<string, unknown>) =>
        new Date((row as unknown as CAPADto).createdAt).toLocaleDateString(),
    },
  ]

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>CAPA Management</h1>
          <p className={styles.subtitle}>
            Corrective and preventive actions from initiation through effectiveness verification
          </p>
        </div>
        <Button icon={<Plus size={16} />} onClick={() => navigate('/capa/new')}>
          New CAPA
        </Button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search by CAPA number or title..."
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
          options={priorityOptions}
          value={priority}
          onChange={(e) => {
            setPriority(e.target.value)
            setPage(1)
          }}
          className={styles.priorityFilter}
        />
      </div>

      {isError && (
        <div className={styles.errorBanner}>
          Failed to load CAPA records. Please try again.
        </div>
      )}

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(row) => (row as unknown as CAPADto).id}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        emptyTitle={isLoading ? 'Loading...' : 'No CAPA records found'}
        emptyDescription={
          isLoading
            ? 'Please wait while records are loaded.'
            : 'Get started by initiating your first CAPA.'
        }
      />
    </div>
  )
}
