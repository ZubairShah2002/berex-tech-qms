import { Link } from 'react-router-dom'
import { Button } from '@/components/ui/Button'
import { EmptyState } from '@/components/feedback/EmptyState'
import { FileQuestion } from 'lucide-react'

export function NotFoundPage() {
  return (
    <EmptyState
      icon={<FileQuestion size={48} />}
      title="Page not found"
      description="The page you are looking for does not exist or has been moved."
      action={
        <Link to="/">
          <Button variant="secondary">Back to Dashboard</Button>
        </Link>
      }
    />
  )
}
