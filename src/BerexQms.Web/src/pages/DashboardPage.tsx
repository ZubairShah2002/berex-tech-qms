import styles from './DashboardPage.module.css'

const metrics = [
  { label: 'Open NCRs', value: '—', note: 'No data yet' },
  { label: 'Pending CAPAs', value: '—', note: 'No data yet' },
  { label: 'Upcoming Calibrations', value: '—', note: 'No data yet' },
  { label: 'Overdue Training', value: '—', note: 'No data yet' },
]

export function DashboardPage() {
  return (
    <div className={styles.page}>
      <div className={styles.header}>
        <h1 className={styles.title}>Dashboard</h1>
      </div>

      <div className={styles.grid}>
        {metrics.map((m) => (
          <div key={m.label} className={styles.card}>
            <div className={styles.cardLabel}>{m.label}</div>
            <div className={styles.cardValue}>{m.value}</div>
            <div className={styles.cardFooter}>{m.note}</div>
          </div>
        ))}
      </div>
    </div>
  )
}
