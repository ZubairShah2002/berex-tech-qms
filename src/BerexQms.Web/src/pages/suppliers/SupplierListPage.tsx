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
import styles from './SupplierListPage.module.css'

interface SupplierDto {
  id: string
  code: string
  name: string
  status: string
  riskLevel: string
  tier: string | null
  approvedSince: string | null
  contactName: string | null
  contactEmail: string | null
  approvalCount: number
  scarCount: number
  createdAt: string
}

interface PagedResult {
  items: SupplierDto[]
  totalCount: number
  page: number
  pageSize: number
}

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Prospective', label: 'Prospective' },
  { value: 'Approved', label: 'Approved' },
  { value: 'ConditionalApproval', label: 'Conditional' },
  { value: 'OnProbation', label: 'On Probation' },
  { value: 'Disqualified', label: 'Disqualified' },
  { value: 'Inactive', label: 'Inactive' },
]

const riskOptions = [
  { value: '', label: 'All risk levels' },
  { value: 'Low', label: 'Low' },
  { value: 'Medium', label: 'Medium' },
  { value: 'High', label: 'High' },
  { value: 'Critical', label: 'Critical' },
]

export function SupplierListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [riskLevel, setRiskLevel] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, isError } = useQuery<PagedResult>({
    queryKey: ['suppliers', search, status, riskLevel, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (status) params.set('status', status)
      if (riskLevel) params.set('riskLevel', riskLevel)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/suppliers?${params}`)
      return res.data
    },
  })

  const handleSearch = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(1)
  }, [])

  const columns = [
    {
      key: 'code',
      header: 'Code',
      width: '100px',
      sortable: true,
      render: (row: Record<string, unknown>) => {
        const s = row as unknown as SupplierDto
        return (
          <button className={styles.linkButton} onClick={() => navigate(`/suppliers/${s.id}`)}>
            {s.code}
          </button>
        )
      },
    },
    {
      key: 'name',
      header: 'Supplier Name',
      sortable: true,
      render: (row: Record<string, unknown>) => {
        const s = row as unknown as SupplierDto
        return (
          <button className={styles.linkButton} onClick={() => navigate(`/suppliers/${s.id}`)}>
            {s.name}
          </button>
        )
      },
    },
    {
      key: 'status',
      header: 'Status',
      width: '140px',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={(row as unknown as SupplierDto).status} />
      ),
    },
    {
      key: 'riskLevel',
      header: 'Risk',
      width: '90px',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={(row as unknown as SupplierDto).riskLevel} />
      ),
    },
    {
      key: 'tier',
      header: 'Tier',
      width: '100px',
      render: (row: Record<string, unknown>) =>
        (row as unknown as SupplierDto).tier ?? '—',
    },
    {
      key: 'contactName',
      header: 'Contact',
      render: (row: Record<string, unknown>) =>
        (row as unknown as SupplierDto).contactName ?? '—',
    },
    {
      key: 'scarCount',
      header: 'SCARs',
      width: '70px',
    },
    {
      key: 'createdAt',
      header: 'Created',
      width: '100px',
      render: (row: Record<string, unknown>) =>
        new Date((row as unknown as SupplierDto).createdAt).toLocaleDateString(),
    },
  ]

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Supplier Quality</h1>
          <p className={styles.subtitle}>
            Manage suppliers, approvals, scorecards, and corrective actions
          </p>
        </div>
        <Button icon={<Plus size={16} />} onClick={() => navigate('/suppliers/new')}>
          New Supplier
        </Button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search by code or name..."
            value={search}
            onChange={handleSearch}
            className={styles.searchInput}
          />
        </div>
        <Select
          options={statusOptions}
          value={status}
          onChange={(e) => { setStatus(e.target.value); setPage(1) }}
          className={styles.statusFilter}
        />
        <Select
          options={riskOptions}
          value={riskLevel}
          onChange={(e) => { setRiskLevel(e.target.value); setPage(1) }}
          className={styles.riskFilter}
        />
      </div>

      {isError && (
        <div className={styles.errorBanner}>
          Failed to load suppliers. Please try again.
        </div>
      )}

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(row) => (row as unknown as SupplierDto).id}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        emptyTitle={isLoading ? 'Loading...' : 'No suppliers found'}
        emptyDescription={
          isLoading
            ? 'Please wait while records are loaded.'
            : 'Get started by registering your first supplier.'
        }
      />
    </div>
  )
}
