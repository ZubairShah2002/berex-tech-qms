import { useNavigate, useParams } from 'react-router-dom'
import { ArrowLeft } from 'lucide-react'
import { useQuery } from '@tanstack/react-query'
import { apiClient } from '@/lib/api-client'
import styles from './QualificationDetailPage.module.css'

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

export function QualificationDetailPage() {
  const { id } = useParams<{ id: string }>()
  const navigate = useNavigate()

  const { data, isLoading, error } = useQuery<QualificationDto>({
    queryKey: ['qualification', id],
    queryFn: async () => {
      const res = await apiClient.get(`/api/v1/qualifications/${id}`)
      return res.data
    },
    enabled: !!id,
  })

  if (isLoading) return <div className={styles.loading}>Loading qualification...</div>
  if (error || !data) return <div className={styles.errorBanner}>Failed to load qualification.</div>

  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <button
          type="button"
          className={styles.backButton}
          onClick={() => navigate('/training')}
        >
          <ArrowLeft size={16} />
        </button>
        <div className={styles.headerContent}>
          <h1 className={styles.title}>{data.name}</h1>
          <p className={styles.code}>{data.code}</p>
        </div>
        <span className={`${styles.activeBadge} ${data.isActive ? styles.active : styles.inactive}`}>
          {data.isActive ? 'Active' : 'Inactive'}
        </span>
      </div>

      <div className={styles.section}>
        <h2 className={styles.sectionTitle}>Details</h2>
        <div className={styles.fieldGrid}>
          <div>
            <p className={styles.fieldLabel}>Code</p>
            <p className={styles.fieldValue}>{data.code}</p>
          </div>
          <div>
            <p className={styles.fieldLabel}>Name</p>
            <p className={styles.fieldValue}>{data.name}</p>
          </div>
          <div>
            <p className={styles.fieldLabel}>Validity Period</p>
            <p className={styles.fieldValue}>{data.validityMonths} months</p>
          </div>
          <div>
            <p className={styles.fieldLabel}>Renewal Window</p>
            <p className={styles.fieldValue}>{data.renewalWindowDays} days</p>
          </div>
          <div>
            <p className={styles.fieldLabel}>Created</p>
            <p className={styles.fieldValue}>{new Date(data.createdAt).toLocaleDateString()}</p>
          </div>
        </div>
      </div>

      {data.description && (
        <div className={styles.section}>
          <h2 className={styles.sectionTitle}>Description</h2>
          <p className={styles.fieldValue}>{data.description}</p>
        </div>
      )}

      <div className={styles.section}>
        <h2 className={styles.sectionTitle}>Scope</h2>
        <div className={styles.fieldGrid}>
          <div>
            <p className={styles.fieldLabel}>Product Family</p>
            <p className={styles.fieldValue}>{data.scopeProductFamily ?? '—'}</p>
          </div>
          <div>
            <p className={styles.fieldLabel}>Inspection Type</p>
            <p className={styles.fieldValue}>{data.scopeInspectionType ?? '—'}</p>
          </div>
          <div>
            <p className={styles.fieldLabel}>Process Area</p>
            <p className={styles.fieldValue}>{data.scopeProcessArea ?? '—'}</p>
          </div>
        </div>
      </div>
    </div>
  )
}
