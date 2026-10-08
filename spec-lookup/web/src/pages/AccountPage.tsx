import { useState } from 'react';
import { api } from '../api';
import { useAuth } from '../auth';
import { useToast } from '../components/Toast';

export function AccountPage() {
  const { user } = useAuth();
  const toast = useToast();
  const [current, setCurrent] = useState('');
  const [next, setNext] = useState('');
  const [confirm, setConfirm] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  return (
    <div className="page page-narrow">
      <div className="page-head"><h1>My Account</h1></div>
      <section className="panel">
        <div className="panel-head"><h2>Account</h2></div>
        <dl className="kv">
          <div><dt>User ID</dt><dd>{user?.userId}</dd></div>
          <div><dt>Name</dt><dd>{user?.displayName}</dd></div>
          <div><dt>Role</dt><dd>{user?.role === 'admin' ? 'Administrator' : 'QC User'}</dd></div>
          <div><dt>Import</dt><dd>{user?.canImport ? 'Allowed' : 'Not allowed'}</dd></div>
        </dl>
      </section>
      <section className="panel">
        <div className="panel-head"><h2>Change Password</h2></div>
        <form className="panel-body" style={{ display: 'grid', gap: 12, maxWidth: 360 }} onSubmit={async (e) => {
          e.preventDefault();
          setError(null);
          if (next !== confirm) { setError('New passwords do not match.'); return; }
          setBusy(true);
          try {
            await api.changePassword(current, next);
            setCurrent(''); setNext(''); setConfirm('');
            toast('Password changed. Other devices have been logged out.');
          } catch (err) { setError((err as Error).message); } finally { setBusy(false); }
        }}>
          {error && <div className="alert alert-error" style={{ margin: 0 }}>{error}</div>}
          <div className="field"><label htmlFor="cp">Current password</label>
            <input id="cp" className="input" type="password" autoComplete="current-password" value={current} onChange={(e) => setCurrent(e.target.value)} required /></div>
          <div className="field"><label htmlFor="np">New password</label>
            <input id="np" className="input" type="password" autoComplete="new-password" minLength={8} value={next} onChange={(e) => setNext(e.target.value)} required />
            <span className="hint">At least 8 characters.</span></div>
          <div className="field"><label htmlFor="np2">Confirm new password</label>
            <input id="np2" className="input" type="password" autoComplete="new-password" value={confirm} onChange={(e) => setConfirm(e.target.value)} required /></div>
          <div><button className="btn btn-primary" disabled={busy}>Change Password</button></div>
        </form>
      </section>
    </div>
  );
}
