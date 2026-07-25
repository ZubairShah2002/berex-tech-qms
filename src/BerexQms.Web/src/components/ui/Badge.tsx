import type { ReactNode } from 'react'
import { cn } from '@/lib/cn'
import styles from './Badge.module.css'

type BadgeVariant = 'neutral' | 'success' | 'warning' | 'error' | 'info'

interface BadgeProps {
  variant?: BadgeVariant
  dot?: boolean
  children: ReactNode
  className?: string
}

export function Badge({ variant = 'neutral', dot, children, className }: BadgeProps) {
  return (
    <span className={cn(styles.badge, styles[variant], className)}>
      {dot && <span className={styles.dot} />}
      {children}
    </span>
  )
}

interface StatusBadgeProps {
  status: string
  className?: string
}

const statusMap: Record<string, BadgeVariant> = {
  draft: 'neutral',
  pending: 'warning',
  'in-review': 'info',
  approved: 'success',
  rejected: 'error',
  active: 'success',
  inactive: 'neutral',
  open: 'info',
  closed: 'neutral',
  overdue: 'error',
  completed: 'success',
  inprogress: 'info',
  'in-progress': 'info',
  pendingapproval: 'warning',
  'pending-approval': 'warning',
  cancelled: 'neutral',
  pass: 'success',
  fail: 'error',
  conditionalpass: 'warning',
  'conditional-pass': 'warning',
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const variant = statusMap[status.toLowerCase().replace(/\s+/g, '-')] ?? 'neutral'
  const label = status.charAt(0).toUpperCase() + status.slice(1)

  return (
    <Badge variant={variant} dot className={className}>
      {label}
    </Badge>
  )
}
