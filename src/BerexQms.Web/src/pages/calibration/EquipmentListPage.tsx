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
import styles from './EquipmentListPage.module.css'

interface EquipmentDto {
  id: string
  code: string
  name: string
  type: string | null
  manufacturer: string | null
  model: string | null
  serialNumber: string | null
  status: string
  location: string | null
  department: string | null
  nextDueDate: string | null
  calibrationCount: number
  createdAt: string
}

interface PagedResult {
  items: EquipmentDto[]
  totalCount: number
  page: number
  pageSize: number
}

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Active', label: 'Active' },
  { value: 'DueForCalibration', label: 'Due for Calibration' },
  { value: 'Overdue', label: 'Overdue' },
  { value: 'InCalibration', label: 'In Calibration' },
  { value: 'OutOfService', label: 'Out of Service' },
  { value: 'Retired', label: 'Retired' },
]

export function EquipmentListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, error } = useQuery<PagedResult>({
    queryKey: ['equipment', search, status, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (status) params.set('status', status)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/api/v1/equipment?${params}`)
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
          onClick={() => navigate(`/calibration/${row.id as string}`)}
        >
          {row.code as string}
        </button>
      ),
    },
    { key: 'name' as const, header: 'Name', sortable: true },
    {
      key: 'status' as const,
      header: 'Status',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={row.status as string} />
      ),
    },
    { key: 'type' as const, header: 'Type' },
    { key: 'manufacturer' as const, header: 'Manufacturer' },
    { key: 'location' as const, header: 'Location' },
    {
      key: 'nextDueDate' as const,
      header: 'Next Due',
      render: (row: Record<string, unknown>) => {
        const d = row.nextDueDate as string | null
        return d ? new Date(d).toLocaleDateString() : '—'
      },
    },
    {
      key: 'calibrationCount' as const,
      header: 'Records',
      render: (row: Record<string, unknown>) => String(row.calibrationCount ?? 0),
    },
  ]

  const handleSearchChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
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
          <h1 className={styles.title}>Equipment Registry</h1>
          <p className={styles.subtitle}>
            Manage measurement equipment, calibration records, and gauge studies
          </p>
        </div>
        <Button onClick={() => navigate('/calibration/new')}>
          <Plus size={16} /> Register Equipment
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
          value={status}
          onChange={handleStatusChange}
          className={styles.statusFilter}
          options={statusOptions}
        />
      </div>

      {error && (
        <div className={styles.errorBanner}>
          Failed to load equipment. Please try again.
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
        emptyTitle={isLoading ? 'Loading...' : 'No equipment found'}
        emptyDescription="Register equipment to start tracking calibrations."
      />
    </div>
  )
}
