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
import styles from './InspectionsListPage.module.css'

interface InspectionDto {
  id: string
  inspectionNumber: string
  type: string
  status: string
  partId: string
  partRevisionId: string | null
  lotNumber: string | null
  lotSize: number | null
  sampleSize: number | null
  supplierId: string | null
  inspectorId: string
  result: string | null
  createdAt: string
}

interface PagedResult {
  items: InspectionDto[]
  totalCount: number
  page: number
  pageSize: number
}

const typeOptions = [
  { value: '', label: 'All types' },
  { value: 'IQC', label: 'IQC' },
  { value: 'IPQC', label: 'IPQC' },
  { value: 'OQC', label: 'OQC' },
]

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Draft', label: 'Draft' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'PendingApproval', label: 'Pending Approval' },
  { value: 'Approved', label: 'Approved' },
  { value: 'Rejected', label: 'Rejected' },
  { value: 'Cancelled', label: 'Cancelled' },
]

export function InspectionsListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [type, setType] = useState('')
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, isError } = useQuery<PagedResult>({
    queryKey: ['inspections', search, type, status, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (type) params.set('type', type)
      if (status) params.set('status', status)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/inspections?${params}`)
      return res.data
    },
  })

  const handleSearch = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(1)
  }, [])

  const columns = [
    {
      key: 'inspectionNumber',
      header: 'Inspection #',
      sortable: true,
      width: '160px',
      render: (row: Record<string, unknown>) => {
        const insp = row as unknown as InspectionDto
        return (
          <button
            className={styles.linkButton}
            onClick={() => navigate(`/inspections/${insp.id}`)}
          >
            {insp.inspectionNumber}
          </button>
        )
      },
    },
    {
      key: 'type',
      header: 'Type',
      width: '80px',
    },
    {
      key: 'status',
      header: 'Status',
      width: '140px',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={(row as unknown as InspectionDto).status} />
      ),
    },
    {
      key: 'lotNumber',
      header: 'Lot #',
      width: '120px',
      render: (row: Record<string, unknown>) =>
        (row as unknown as InspectionDto).lotNumber ?? '—',
    },
    {
      key: 'sampleSize',
      header: 'Sample',
      width: '80px',
      render: (row: Record<string, unknown>) => {
        const insp = row as unknown as InspectionDto
        return insp.sampleSize != null ? String(insp.sampleSize) : '—'
      },
    },
    {
      key: 'result',
      header: 'Result',
      width: '120px',
      render: (row: Record<string, unknown>) => {
        const result = (row as unknown as InspectionDto).result
        if (!result) return '—'
        return <StatusBadge status={result} />
      },
    },
    {
      key: 'createdAt',
      header: 'Created',
      width: '120px',
      render: (row: Record<string, unknown>) =>
        new Date((row as unknown as InspectionDto).createdAt).toLocaleDateString(),
    },
  ]

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Quality Inspections</h1>
          <p className={styles.subtitle}>
            Manage incoming, in-process, and outgoing quality inspections
          </p>
        </div>
        <Button icon={<Plus size={16} />} onClick={() => navigate('/inspections/new')}>
          New Inspection
        </Button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search by inspection number..."
            value={search}
            onChange={handleSearch}
            className={styles.searchInput}
          />
        </div>
        <Select
          options={typeOptions}
          value={type}
          onChange={(e) => {
            setType(e.target.value)
            setPage(1)
          }}
          className={styles.typeFilter}
        />
        <Select
          options={statusOptions}
          value={status}
          onChange={(e) => {
            setStatus(e.target.value)
            setPage(1)
          }}
          className={styles.statusFilter}
        />
      </div>

      {isError && (
        <div className={styles.errorBanner}>
          Failed to load inspections. Please try again.
        </div>
      )}

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(row) => (row as unknown as InspectionDto).id}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        emptyTitle={isLoading ? 'Loading...' : 'No inspections found'}
        emptyDescription={
          isLoading
            ? 'Please wait while inspections are loaded.'
            : 'Get started by creating your first inspection.'
        }
      />
    </div>
  )
}
