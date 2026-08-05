import { useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, Search } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import styles from './QualificationsListPage.module.css'

interface QualificationDto {
  id: string
  code: string
  name: string
  description: string | null
  scopeProductFamily: string | null
  scopeInspectionType: string | null
  scopeProcessArea: string | null
  validityMonths: number
  renewalWindowDays: number
  isActive: boolean
  createdAt: string
}

interface PagedResult {
  items: QualificationDto[]
  totalCount: number
  page: number
  pageSize: number
}

type TabKey = 'qualifications' | 'courses' | 'assignments' | 'skill-matrix'

export function QualificationsListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [activeTab, setActiveTab] = useState<TabKey>('qualifications')
  const pageSize = 20

  const { data, error } = useQuery<PagedResult>({
    queryKey: ['qualifications', search, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/api/v1/qualifications?${params}`)
      return res.data
    },
    enabled: activeTab === 'qualifications',
  })

  const handleTabChange = useCallback((tab: TabKey) => {
    setActiveTab(tab)
    if (tab === 'courses') navigate('/training/courses')
    else if (tab === 'assignments') navigate('/training/assignments')
    else if (tab === 'skill-matrix') navigate('/training/skill-matrix')
  }, [navigate])

  const columns = [
    {
      key: 'code' as const,
      header: 'Code',
      sortable: true,
      render: (row: Record<string, unknown>) => (
        <button
          type="button"
          className={styles.linkButton}
          onClick={() => navigate(`/training/qualifications/${row.id as string}`)}
        >
          {row.code as string}
        </button>
      ),
    },
    { key: 'name' as const, header: 'Name', sortable: true },
    {
      key: 'validityMonths' as const,
      header: 'Validity',
      render: (row: Record<string, unknown>) => `${row.validityMonths as number} months`,
    },
    {
      key: 'scopeProductFamily' as const,
      header: 'Scope',
      render: (row: Record<string, unknown>) => {
        const parts = [
          row.scopeProductFamily,
          row.scopeInspectionType,
          row.scopeProcessArea,
        ].filter(Boolean)
        return parts.length > 0 ? (parts as string[]).join(', ') : '—'
      },
    },
    {
      key: 'isActive' as const,
      header: 'Status',
      render: (row: Record<string, unknown>) => (
        <span className={`${styles.activeBadge} ${row.isActive ? styles.active : styles.inactive}`}>
          {row.isActive ? 'Active' : 'Inactive'}
        </span>
      ),
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

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Training & Competency</h1>
          <p className={styles.subtitle}>
            Manage qualifications, courses, assignments, and competency tracking
          </p>
        </div>
        <div className={styles.headerActions}>
          <Button onClick={() => navigate('/training/qualifications/new')}>
            <Plus size={16} /> New Qualification
          </Button>
        </div>
      </div>

      <div className={styles.tabs}>
        <button
          type="button"
          className={`${styles.tab} ${activeTab === 'qualifications' ? styles.tabActive : ''}`}
          onClick={() => setActiveTab('qualifications')}
        >
          Qualifications
        </button>
        <button
          type="button"
          className={`${styles.tab} ${activeTab === 'courses' ? styles.tabActive : ''}`}
          onClick={() => handleTabChange('courses')}
        >
          Courses
        </button>
        <button
          type="button"
          className={`${styles.tab} ${activeTab === 'assignments' ? styles.tabActive : ''}`}
          onClick={() => handleTabChange('assignments')}
        >
          Assignments
        </button>
        <button
          type="button"
          className={`${styles.tab} ${activeTab === 'skill-matrix' ? styles.tabActive : ''}`}
          onClick={() => handleTabChange('skill-matrix')}
        >
          Skill Matrix
        </button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search qualifications..."
            value={search}
            onChange={handleSearchChange}
            className={styles.searchInput}
          />
        </div>
      </div>

      {error && (
        <div className={styles.errorBanner}>
          Failed to load qualifications. Please try again.
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
        emptyTitle="No qualifications found"
        emptyDescription="Create a qualification to define competency requirements."
      />
    </div>
  )
}
