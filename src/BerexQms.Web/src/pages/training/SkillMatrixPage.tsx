import { useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import { DataTable } from '@/components/ui/DataTable'
import styles from './SkillMatrixPage.module.css'

interface SkillMatrixEntry {
  employeeId: string
  qualificationId: string
  qualificationCode: string
  qualificationName: string
  status: string
  expiryDate: string | null
}

type TabKey = 'qualifications' | 'courses' | 'assignments' | 'skill-matrix'

const statusStyleMap: Record<string, string> = {
  Qualified: styles.qualified,
  InTraining: styles.intraining,
  Expired: styles.expired,
  NotStarted: styles.notstarted,
  Suspended: styles.suspended,
  Revoked: styles.revoked,
}

export function SkillMatrixPage() {
  const navigate = useNavigate()

  const { data, isLoading, error } = useQuery<SkillMatrixEntry[]>({
    queryKey: ['skill-matrix'],
    queryFn: async () => {
      const res = await apiClient.get('/api/v1/training/skill-matrix')
      return res.data
    },
  })

  const handleTabChange = useCallback((tab: TabKey) => {
    if (tab === 'qualifications') navigate('/training')
    else if (tab === 'courses') navigate('/training/courses')
    else if (tab === 'assignments') navigate('/training/assignments')
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
    { key: 'qualificationCode' as const, header: 'Qualification Code' },
    { key: 'qualificationName' as const, header: 'Qualification' },
    {
      key: 'status' as const,
      header: 'Status',
      render: (row: Record<string, unknown>) => {
        const st = row.status as string
        const cls = statusStyleMap[st] ?? styles.notstarted
        return <span className={`${styles.statusBadge} ${cls}`}>{st}</span>
      },
    },
    {
      key: 'expiryDate' as const,
      header: 'Expiry',
      render: (row: Record<string, unknown>) => {
        const d = row.expiryDate as string | null
        return d ? new Date(d).toLocaleDateString() : '—'
      },
    },
  ]

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <div>
          <h1 className={styles.title}>Training & Competency</h1>
          <p className={styles.subtitle}>
            Employee vs. Qualification competency matrix
          </p>
        </div>
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
          className={styles.tab}
          onClick={() => handleTabChange('assignments')}
        >
          Assignments
        </button>
        <button
          type="button"
          className={`${styles.tab} ${styles.tabActive}`}
        >
          Skill Matrix
        </button>
      </div>

      {error && (
        <div className={styles.errorBanner}>
          Failed to load skill matrix. Please try again.
        </div>
      )}

      {isLoading ? (
        <div className={styles.loading}>Loading skill matrix...</div>
      ) : (
        <div className={styles.matrixContainer}>
          <DataTable
            columns={columns}
            data={(data ?? []) as unknown as Record<string, unknown>[]}
            keyExtractor={(row) => `${row.employeeId as string}-${row.qualificationId as string}`}
            emptyTitle="No competency records found"
            emptyDescription="Competency records will appear here once employees are assigned qualifications."
          />
        </div>
      )}
    </div>
  )
}
