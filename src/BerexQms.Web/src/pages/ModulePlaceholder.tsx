import { EmptyState } from '@/components/feedback/EmptyState'
import { Construction } from 'lucide-react'

interface ModulePlaceholderProps {
  moduleName: string
}

export function ModulePlaceholder({ moduleName }: ModulePlaceholderProps) {
  return (
    <EmptyState
      icon={<Construction size={48} />}
      title={moduleName}
      description="This module will be implemented in an upcoming sprint."
    />
  )
}
