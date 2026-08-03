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
import styles from './AuditListPage.module.css'

interface AuditPlanDto {
  id: string
  planName: string
  year: number
  description: string | null
  scope: string | null
  isActive: boolean
  auditCount: number
  createdAt: string
}

interface PagedResult {
  items: AuditPlanDto[]
  totalCount: number
  page: number
  pageSize: number
}

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'true', label: 'Active' },
  { value: 'false', label: 'Inactive' },
]

const currentYear = new Date().getFullYear()
const yearOptions = [
  { value: '', label: 'All years' },
  ...Array.from({ length: 5 }, (_, i) => ({
    value: String(currentYear - i),
    label: String(currentYear - i),
  })),
]

export function AuditListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [isActive, setIsActive] = useState('')
  const [year, setYear] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, isError } = useQuery<PagedResult>({
    queryKey: ['audits', search, isActive, year, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (isActive) params.set('isActive', isActive)
      if (year) params.set('year', year)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/audits?${params}`)
      return res.data
    },
  })

  const handleSearch = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(1)
  }, [])

  const columns = [
    {
      key: 'planName',
      header: 'Plan Name',
      sortable: true,
      render: (row: Record<string, unknown>) => {
        const plan = row as unknown as AuditPlanDto
        return (
          <button
            className={styles.linkButton}
            onClick={() => navigate(`/audits/${plan.id}`)}
          >
            {plan.planName}
          </button>
        )
      },
    },
    {
      key: 'year',
      header: 'Year',
      width: '80px',
    },
    {
      key: 'isActive',
      header: 'Status',
      width: '100px',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={(row as unknown as AuditPlanDto).isActive ? 'Active' : 'Inactive'} />
      ),
    },
    {
      key: 'auditCount',
      header: 'Audits',
      width: '80px',
    },
    {
      key: 'scope',
      header: 'Scope',
      render: (row: Record<string, unknown>) => {
        const scope = (row as unknown as AuditPlanDto).scope
        if (!scope) return '—'
        return scope.length > 60 ? `${scope.substring(0, 60)}...` : scope
      },
    },
    {
      key: 'createdAt',
      header: 'Created',
      width: '100px',
      render: (row: Record<string, unknown>) =>
        new Date((row as unknown as AuditPlanDto).createdAt).toLocaleDateString(),
    },
  ]

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Audit Management</h1>
          <p className={styles.subtitle}>
            Plan, execute, and track internal and external audits
          </p>
        </div>
        <Button icon={<Plus size={16} />} onClick={() => navigate('/audits/new')}>
          New Audit Plan
        </Button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search by plan name..."
            value={search}
            onChange={handleSearch}
            className={styles.searchInput}
          />
        </div>
        <Select
          options={statusOptions}
          value={isActive}
          onChange={(e) => {
            setIsActive(e.target.value)
            setPage(1)
          }}
          className={styles.statusFilter}
        />
        <Select
          options={yearOptions}
          value={year}
          onChange={(e) => {
            setYear(e.target.value)
            setPage(1)
          }}
          className={styles.yearFilter}
        />
      </div>

      {isError && (
        <div className={styles.errorBanner}>
          Failed to load audit plans. Please try again.
        </div>
      )}

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(row) => (row as unknown as AuditPlanDto).id}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        emptyTitle={isLoading ? 'Loading...' : 'No audit plans found'}
        emptyDescription={
          isLoading
            ? 'Please wait while records are loaded.'
            : 'Get started by creating your first audit plan.'
        }
      />
    </div>
  )
}
