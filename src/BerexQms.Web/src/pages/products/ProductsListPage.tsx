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
import styles from './ProductsListPage.module.css'

interface PartDto {
  id: string
  partNumber: string
  name: string
  description: string | null
  productFamily: string | null
  category: string | null
  serializationMode: string
  status: string
  unitOfMeasure: string | null
  currentRevision: string | null
  revisionCount: number
  createdAt: string
}

interface PagedResult {
  items: PartDto[]
  totalCount: number
  page: number
  pageSize: number
}

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Active', label: 'Active' },
  { value: 'Inactive', label: 'Inactive' },
  { value: 'Obsolete', label: 'Obsolete' },
]

export function ProductsListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading } = useQuery<PagedResult>({
    queryKey: ['parts', search, status, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (status) params.set('status', status)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/parts?${params}`)
      return res.data
    },
  })

  const handleSearch = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(1)
  }, [])

  const columns = [
    {
      key: 'partNumber',
      header: 'Part Number',
      sortable: true,
      width: '140px',
      render: (row: Record<string, unknown>) => {
        const part = row as unknown as PartDto
        return (
          <button
            className={styles.linkButton}
            onClick={() => navigate(`/products/${part.id}`)}
          >
            {part.partNumber}
          </button>
        )
      },
    },
    { key: 'name', header: 'Name', sortable: true },
    {
      key: 'productFamily',
      header: 'Product Family',
      width: '140px',
      render: (row: Record<string, unknown>) => (row as unknown as PartDto).productFamily ?? '—',
    },
    {
      key: 'category',
      header: 'Category',
      width: '120px',
      render: (row: Record<string, unknown>) => (row as unknown as PartDto).category ?? '—',
    },
    {
      key: 'currentRevision',
      header: 'Current Rev.',
      width: '100px',
      render: (row: Record<string, unknown>) => (row as unknown as PartDto).currentRevision ?? '—',
    },
    {
      key: 'status',
      header: 'Status',
      width: '100px',
      render: (row: Record<string, unknown>) => <StatusBadge status={(row as unknown as PartDto).status} />,
    },
  ]

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Product Catalog</h1>
          <p className={styles.subtitle}>
            Manage part definitions, revisions, and specifications
          </p>
        </div>
        <Button icon={<Plus size={16} />} onClick={() => navigate('/products/new')}>
          New Part
        </Button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search by part number or name..."
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
      </div>

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(row) => (row as unknown as PartDto).id}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        emptyTitle={isLoading ? 'Loading...' : 'No parts found'}
        emptyDescription={
          isLoading
            ? 'Please wait while parts are loaded.'
            : 'Get started by creating your first part definition.'
        }
      />
    </div>
  )
}
