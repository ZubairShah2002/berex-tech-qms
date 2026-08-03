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
import styles from './DocumentListPage.module.css'

interface DocumentDto {
  id: string
  documentNumber: string
  title: string
  documentType: string
  ownerId: string
  department: string | null
  isActive: boolean
  versionCount: number
  currentVersionNumber: string | null
  currentVersionStatus: string | null
  createdAt: string
}

interface PagedResult {
  items: DocumentDto[]
  totalCount: number
  page: number
  pageSize: number
}

const typeOptions = [
  { value: '', label: 'All types' },
  { value: 'Procedure', label: 'Procedure' },
  { value: 'WorkInstruction', label: 'Work Instruction' },
  { value: 'Specification', label: 'Specification' },
  { value: 'Form', label: 'Form' },
  { value: 'Template', label: 'Template' },
  { value: 'Policy', label: 'Policy' },
  { value: 'Manual', label: 'Manual' },
  { value: 'ExternalDocument', label: 'External Document' },
]

const activeOptions = [
  { value: '', label: 'All' },
  { value: 'true', label: 'Active' },
  { value: 'false', label: 'Inactive' },
]

export function DocumentListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [documentType, setDocumentType] = useState('')
  const [isActive, setIsActive] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, isLoading, isError } = useQuery<PagedResult>({
    queryKey: ['documents', search, documentType, isActive, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      if (documentType) params.set('documentType', documentType)
      if (isActive) params.set('isActive', isActive)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/documents?${params}`)
      return res.data
    },
  })

  const handleSearch = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(1)
  }, [])

  const columns = [
    {
      key: 'documentNumber',
      header: 'Doc #',
      sortable: true,
      width: '130px',
      render: (row: Record<string, unknown>) => {
        const doc = row as unknown as DocumentDto
        return (
          <button
            className={styles.linkButton}
            onClick={() => navigate(`/documents/${doc.id}`)}
          >
            {doc.documentNumber}
          </button>
        )
      },
    },
    {
      key: 'title',
      header: 'Title',
      render: (row: Record<string, unknown>) => (row as unknown as DocumentDto).title,
    },
    {
      key: 'documentType',
      header: 'Type',
      width: '140px',
    },
    {
      key: 'currentVersionStatus',
      header: 'Status',
      width: '140px',
      render: (row: Record<string, unknown>) => {
        const doc = row as unknown as DocumentDto
        if (!doc.currentVersionStatus) return '—'
        return <StatusBadge status={doc.currentVersionStatus} />
      },
    },
    {
      key: 'currentVersionNumber',
      header: 'Version',
      width: '80px',
      render: (row: Record<string, unknown>) =>
        (row as unknown as DocumentDto).currentVersionNumber ?? '—',
    },
    {
      key: 'department',
      header: 'Department',
      width: '120px',
      render: (row: Record<string, unknown>) =>
        (row as unknown as DocumentDto).department ?? '—',
    },
    {
      key: 'isActive',
      header: 'Active',
      width: '80px',
      render: (row: Record<string, unknown>) =>
        (row as unknown as DocumentDto).isActive ? 'Yes' : 'No',
    },
    {
      key: 'createdAt',
      header: 'Created',
      width: '100px',
      render: (row: Record<string, unknown>) =>
        new Date((row as unknown as DocumentDto).createdAt).toLocaleDateString(),
    },
  ]

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Document Control</h1>
          <p className={styles.subtitle}>
            Manage controlled documents, versions, approvals, and distribution
          </p>
        </div>
        <Button icon={<Plus size={16} />} onClick={() => navigate('/documents/new')}>
          New Document
        </Button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search by document number or title..."
            value={search}
            onChange={handleSearch}
            className={styles.searchInput}
          />
        </div>
        <Select
          options={typeOptions}
          value={documentType}
          onChange={(e) => {
            setDocumentType(e.target.value)
            setPage(1)
          }}
          className={styles.typeFilter}
        />
        <Select
          options={activeOptions}
          value={isActive}
          onChange={(e) => {
            setIsActive(e.target.value)
            setPage(1)
          }}
          className={styles.activeFilter}
        />
      </div>

      {isError && (
        <div className={styles.errorBanner}>
          Failed to load documents. Please try again.
        </div>
      )}

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as unknown as Record<string, unknown>[]}
        keyExtractor={(row) => (row as unknown as DocumentDto).id}
        page={page}
        pageSize={pageSize}
        totalCount={data?.totalCount}
        onPageChange={setPage}
        emptyTitle={isLoading ? 'Loading...' : 'No documents found'}
        emptyDescription={
          isLoading
            ? 'Please wait while documents are loaded.'
            : 'Get started by creating your first controlled document.'
        }
      />
    </div>
  )
}
