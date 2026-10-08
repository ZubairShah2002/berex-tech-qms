import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <div className="page page-narrow">
      <div className="alert alert-info">Page not found.</div>
      <Link className="btn" to="/">Go to search</Link>
    </div>
  );
}
