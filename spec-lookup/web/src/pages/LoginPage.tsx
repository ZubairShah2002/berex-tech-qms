import { useState } from 'react';
import { Navigate, useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../auth';

/** Only allow redirects to paths inside this app. */
function safeNext(next: string | null): string {
  if (!next || !next.startsWith('/') || next.startsWith('//') || next.includes('\\')) return '/';
  return next;
}

export function LoginPage() {
  const { user, login } = useAuth();
  const [params] = useSearchParams();
  const next = safeNext(params.get('next'));
  const navigate = useNavigate();
  const [userId, setUserId] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (user) return <Navigate to={next} replace />;

  const reason = next.startsWith('/products/new') ? 'Log in to add a product.'
    : next.endsWith('/edit') ? 'Log in to edit this product.'
    : next.startsWith('/import') ? 'Log in to import products.'
    : next.startsWith('/admin') ? 'Log in as an administrator.'
    : 'Searching and viewing products does not require login.';

  return (
    <div className="page">
      <div className="login-box">
        <h1>Log in</h1>
        <form onSubmit={async (e) => {
          e.preventDefault();
          setBusy(true);
          setError(null);
          try {
            await login(userId, password);
            navigate(next, { replace: true });
          } catch (err) {
            setError((err as Error).message);
            setPassword('');
          } finally {
            setBusy(false);
          }
        }}>
          <p className="muted" style={{ margin: 0 }}>{reason}</p>
          {error && <div className="alert alert-error" role="alert" style={{ margin: 0 }}>{error}</div>}
          <div className="field">
            <label htmlFor="userId">User ID</label>
            <input id="userId" className="input" value={userId} onChange={(e) => setUserId(e.target.value)}
              autoComplete="username" autoCapitalize="off" autoCorrect="off" spellCheck={false} autoFocus required />
          </div>
          <div className="field">
            <label htmlFor="password">Password</label>
            <input id="password" className="input" type="password" value={password} onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password" required />
          </div>
          <div className="btn-row">
            <button className="btn btn-primary" type="submit" disabled={busy || !userId || !password}>{busy ? 'Logging in…' : 'Login'}</button>
            <button className="btn" type="button" onClick={() => navigate(-1)}>Cancel</button>
          </div>
        </form>
      </div>
    </div>
  );
}
