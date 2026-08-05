import { useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, Search } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import styles from './CoursesListPage.module.css'

interface CourseDto {
  id: string
  code: string
  name: string
  description: string | null
  durationHours: number
  assessmentType: string | null
  passCriteria: string | null
  qualificationId: string | null
  isActive: boolean
  createdAt: string
}

interface PagedResult {
  items: CourseDto[]
  totalCount: number
  page: number
  pageSize: number
}

type TabKey = 'qualifications' | 'courses' | 'assignments' | 'skill-matrix'

export function CoursesListPage() {
  const navigate = useNavigate()
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, error } = useQuery<PagedResult>({
    queryKey: ['training-courses', search, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (search) params.set('search', search)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/api/v1/training/courses?${params}`)
      return res.data
    },
  })

  const handleTabChange = useCallback((tab: TabKey) => {
    if (tab === 'qualifications') navigate('/training')
    else if (tab === 'assignments') navigate('/training/assignments')
    else if (tab === 'skill-matrix') navigate('/training/skill-matrix')
  }, [navigate])

  const columns = [
    {
      key: 'code' as const,
      header: 'Code',
      sortable: true,
      render: (row: Record<string, unknown>) => (
        <span className={styles.linkButton}>{row.code as string}</span>
      ),
    },
    { key: 'name' as const, header: 'Name', sortable: true },
    {
      key: 'durationHours' as const,
      header: 'Duration',
      render: (row: Record<string, unknown>) => `${row.durationHours as number}h`,
    },
    {
      key: 'assessmentType' as const,
      header: 'Assessment',
      render: (row: Record<string, unknown>) => (row.assessmentType as string) ?? '—',
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
        <Button onClick={() => navigate('/training/courses/new')}>
          <Plus size={16} /> New Course
        </Button>
      </div>

      <div className={styles.tabs}>
        <button
          type="button"
          className={styles.tab}
          onClick={() => handleTabChange('qualifications')}
        >
          Qualifications
        </button>
        <button
          type="button"
          className={`${styles.tab} ${styles.tabActive}`}
        >
          Courses
        </button>
        <button
          type="button"
          className={styles.tab}
          onClick={() => handleTabChange('assignments')}
        >
          Assignments
        </button>
        <button
          type="button"
          className={styles.tab}
          onClick={() => handleTabChange('skill-matrix')}
        >
          Skill Matrix
        </button>
      </div>

      <div className={styles.filters}>
        <div className={styles.searchWrapper}>
          <Search size={16} className={styles.searchIcon} />
          <Input
            placeholder="Search courses..."
            value={search}
            onChange={handleSearchChange}
            className={styles.searchInput}
          />
        </div>
      </div>

      {error && (
        <div className={styles.errorBanner}>
          Failed to load courses. Please try again.
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
        emptyTitle="No courses found"
        emptyDescription="Create a training course to start building your training program."
      />
    </div>
  )
}
