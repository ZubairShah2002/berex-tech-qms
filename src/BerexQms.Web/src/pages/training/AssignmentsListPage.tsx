import { useState, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { DataTable } from '@/components/ui/DataTable'
import { Button } from '@/components/ui/Button'
import { Select } from '@/components/ui/Select'
import { StatusBadge } from '@/components/ui/Badge'
import styles from './AssignmentsListPage.module.css'

interface AssignmentDto {
  id: string
  employeeId: string
  courseId: string
  courseName: string | null
  assignedBy: string
  assignedDate: string
  dueDate: string
  status: string
  completion: {
    completionDate: string
    score: number | null
    result: string
    assessorId: string | null
    evidenceRef: string | null
  } | null
  createdAt: string
}

interface PagedResult {
  items: AssignmentDto[]
  totalCount: number
  page: number
  pageSize: number
}

const statusOptions = [
  { value: '', label: 'All statuses' },
  { value: 'Assigned', label: 'Assigned' },
  { value: 'InProgress', label: 'In Progress' },
  { value: 'Completed', label: 'Completed' },
  { value: 'Overdue', label: 'Overdue' },
  { value: 'Cancelled', label: 'Cancelled' },
]

type TabKey = 'qualifications' | 'courses' | 'assignments' | 'skill-matrix'

export function AssignmentsListPage() {
  const navigate = useNavigate()
  const [status, setStatus] = useState('')
  const [page, setPage] = useState(1)
  const pageSize = 20

  const { data, error } = useQuery<PagedResult>({
    queryKey: ['training-assignments', status, page],
    queryFn: async () => {
      const params = new URLSearchParams()
      if (status) params.set('status', status)
      params.set('page', String(page))
      params.set('pageSize', String(pageSize))
      const res = await apiClient.get(`/api/v1/training/assignments?${params}`)
      return res.data
    },
  })

  const handleTabChange = useCallback((tab: TabKey) => {
    if (tab === 'qualifications') navigate('/training')
    else if (tab === 'courses') navigate('/training/courses')
    else if (tab === 'skill-matrix') navigate('/training/skill-matrix')
  }, [navigate])

  const columns = [
    {
      key: 'employeeId' as const,
      header: 'Employee',
      render: (row: Record<string, unknown>) => {
        const empId = (row.employeeId as string).slice(0, 8)
        return <span title={row.employeeId as string}>{empId}...</span>
      },
    },
    {
      key: 'courseId' as const,
      header: 'Course',
      render: (row: Record<string, unknown>) => {
        const courseId = (row.courseId as string).slice(0, 8)
        return <span title={row.courseId as string}>{courseId}...</span>
      },
    },
    {
      key: 'status' as const,
      header: 'Status',
      render: (row: Record<string, unknown>) => (
        <StatusBadge status={row.status as string} />
      ),
    },
    {
      key: 'assignedDate' as const,
      header: 'Assigned',
      render: (row: Record<string, unknown>) =>
        new Date(row.assignedDate as string).toLocaleDateString(),
    },
    {
      key: 'dueDate' as const,
      header: 'Due Date',
      render: (row: Record<string, unknown>) =>
        new Date(row.dueDate as string).toLocaleDateString(),
    },
    {
      key: 'completion' as const,
      header: 'Result',
      render: (row: Record<string, unknown>) => {
        const comp = row.completion as AssignmentDto['completion']
        if (!comp) return '—'
        return `${comp.result}${comp.score != null ? ` (${comp.score}%)` : ''}`
      },
    },
  ]

  const handleStatusChange = useCallback((e: React.ChangeEvent<HTMLSelectElement>) => {
    setStatus(e.target.value)
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
        <Button onClick={() => navigate('/training/assignments/new')}>
          <Plus size={16} /> New Assignment
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
          className={styles.tab}
          onClick={() => handleTabChange('courses')}
        >
          Courses
        </button>
        <button
          type="button"
          className={`${styles.tab} ${styles.tabActive}`}
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
        <Select
          value={status}
          onChange={handleStatusChange}
          className={styles.statusFilter}
          options={statusOptions}
        />
      </div>

      {error && (
        <div className={styles.errorBanner}>
          Failed to load assignments. Please try again.
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
        emptyTitle="No assignments found"
        emptyDescription="Create a training assignment to start tracking employee training."
      />
    </div>
  )
}
