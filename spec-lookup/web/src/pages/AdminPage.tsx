import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { api, type AdminUser } from '../api';
import { useAuth } from '../auth';
import { Dialog } from '../components/Dialog';
import { useToast } from '../components/Toast';
import { formatDateTime } from '../util';

type Tab = 'users' | 'settings' | 'audit';

export function AdminPage() {
  const [tab, setTab] = useState<Tab>('users');
  return (
    <div className="page">
      <div className="page-head"><h1>Administration</h1></div>
      <div className="tabs">
        <button className={tab === 'users' ? 'active' : ''} onClick={() => setTab('users')}>Users</button>
        <button className={tab === 'settings' ? 'active' : ''} onClick={() => setTab('settings')}>Settings</button>
        <button className={tab === 'audit' ? 'active' : ''} onClick={() => setTab('audit')}>Audit Log</button>
      </div>
      {tab === 'users' && <UsersTab />}
      {tab === 'settings' && <SettingsTab />}
      {tab === 'audit' && <AuditTab />}
    </div>
  );
}

function UsersTab() {
  const { user: me } = useAuth();
  const qc = useQueryClient();
  const toast = useToast();
  const users = useQuery({ queryKey: ['users'], queryFn: api.users });
  const [adding, setAdding] = useState(false);
  const [resetFor, setResetFor] = useState<AdminUser | null>(null);

  const patch = useMutation({
    mutationFn: ({ id, data }: { id: string; data: Parameters<typeof api.updateUser>[1] }) => api.updateUser(id, data),
    onSuccess: () => { void qc.invalidateQueries({ queryKey: ['users'] }); toast('User updated.'); },
    onError: (e: Error) => toast(e.message, 'error'),
  });

  return (
    <>
      <div className="toolbar"><button className="btn btn-primary" onClick={() => setAdding(true)}>+ Add User</button></div>
      {users.isError && <div className="alert alert-error">{(users.error as Error).message}</div>}
      <section className="panel">
        <div className="table-wrap">
          <table className="grid stack">
            <thead><tr><th>User ID</th><th>Name</th><th>Role</th><th>Import</th><th>Status</th><th>Last login</th><th /></tr></thead>
            <tbody>
              {users.data?.map((u) => {
                const self = u.id === me?.id;
                return (
                  <tr key={u.id}>
                    <td className="stack-title mono">{u.userId}{self && <span className="faint"> (you)</span>}</td>
                    <td data-label="Name">{u.displayName || '—'}</td>
                    <td data-label="Role">
                      <select className="select" style={{ width: 'auto', minWidth: 120 }} value={u.role} disabled={self}
                        onChange={(e) => patch.mutate({ id: u.id, data: { role: e.target.value } })}>
                        <option value="qc">QC User</option>
                        <option value="admin">Admin</option>
                      </select>
                    </td>
                    <td data-label="Import">
                      <label className="check">
                        <input type="checkbox" checked={u.canImport} disabled={u.role === 'admin'}
                          onChange={(e) => patch.mutate({ id: u.id, data: { canImport: e.target.checked } })} />
                        {u.role === 'admin' ? 'Always' : 'Allowed'}
                      </label>
                    </td>
                    <td data-label="Status">{u.status === 'active' ? 'Active' : <span className="badge badge-archived">Disabled</span>}</td>
                    <td data-label="Last login" className="nowrap">{u.lastLoginAt ? formatDateTime(u.lastLoginAt) : '—'}</td>
                    <td>
                      <div className="btn-row">
                        <button className="btn btn-sm" onClick={() => setResetFor(u)}>Reset password</button>
                        {!self && (u.status === 'active'
                          ? <button className="btn btn-sm btn-danger" onClick={() => patch.mutate({ id: u.id, data: { status: 'disabled' } })}>Disable</button>
                          : <button className="btn btn-sm" onClick={() => patch.mutate({ id: u.id, data: { status: 'active' } })}>Enable</button>)}
                      </div>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </section>
      {adding && <AddUserDialog onClose={() => setAdding(false)} />}
      {resetFor && <ResetPasswordDialog user={resetFor} onClose={() => setResetFor(null)} />}
    </>
  );
}

function AddUserDialog({ onClose }: { onClose: () => void }) {
  const qc = useQueryClient();
  const toast = useToast();
  const [userId, setUserId] = useState('');
  const [displayName, setDisplayName] = useState('');
  const [role, setRole] = useState<'qc' | 'admin'>('qc');
  const [canImport, setCanImport] = useState(false);
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);
  const submit = async () => {
    setBusy(true); setError(null);
    try {
      await api.createUser({ userId: userId.trim(), displayName: displayName.trim(), role, canImport, password });
      void qc.invalidateQueries({ queryKey: ['users'] });
      toast(`User ${userId.trim()} created.`);
      onClose();
    } catch (e) { setError((e as Error).message); } finally { setBusy(false); }
  };
  return (
    <Dialog title="Add User" onClose={onClose} actions={<>
      <button className="btn" onClick={onClose}>Cancel</button>
      <button className="btn btn-primary" onClick={submit} disabled={busy || !userId.trim() || password.length < 8}>Create User</button>
    </>}>
      <div style={{ display: 'grid', gap: 10 }}>
        {error && <div className="alert alert-error" style={{ margin: 0 }}>{error}</div>}
        <div className="field"><label htmlFor="nu-id">User ID</label>
          <input id="nu-id" className="input" value={userId} onChange={(e) => setUserId(e.target.value)} placeholder="e.g. QC01" autoCapitalize="off" /></div>
        <div className="field"><label htmlFor="nu-name">Name</label>
          <input id="nu-name" className="input" value={displayName} onChange={(e) => setDisplayName(e.target.value)} /></div>
        <div className="field"><label htmlFor="nu-role">Role</label>
          <select id="nu-role" className="select" value={role} onChange={(e) => setRole(e.target.value as 'qc' | 'admin')}>
            <option value="qc">QC User</option><option value="admin">Admin</option>
          </select></div>
        {role === 'qc' && <label className="check"><input type="checkbox" checked={canImport} onChange={(e) => setCanImport(e.target.checked)} /> Allow Excel / CSV import</label>}
        <div className="field"><label htmlFor="nu-pw">Initial password</label>
          <input id="nu-pw" className="input" type="password" autoComplete="new-password" value={password} onChange={(e) => setPassword(e.target.value)} />
          <span className="hint">At least 8 characters. Ask the user to change it after first login.</span></div>
      </div>
    </Dialog>
  );
}

function ResetPasswordDialog({ user, onClose }: { user: AdminUser; onClose: () => void }) {
  const toast = useToast();
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  return (
    <Dialog title={`Reset password — ${user.userId}`} onClose={onClose} actions={<>
      <button className="btn" onClick={onClose}>Cancel</button>
      <button className="btn btn-primary" disabled={password.length < 8} onClick={async () => {
        try { await api.updateUser(user.id, { password }); toast('Password reset. The user has been logged out.'); onClose(); }
        catch (e) { setError((e as Error).message); }
      }}>Set Password</button>
    </>}>
      {error && <div className="alert alert-error">{error}</div>}
      <div className="field"><label htmlFor="rp">New password</label>
        <input id="rp" className="input" type="password" autoComplete="new-password" value={password} onChange={(e) => setPassword(e.target.value)} />
        <span className="hint">At least 8 characters.</span></div>
    </Dialog>
  );
}

function SettingsTab() {
  const qc = useQueryClient();
  const toast = useToast();
  const settings = useQuery({ queryKey: ['settings'], queryFn: api.settings });
  const [siteName, setSiteName] = useState('');
  const [publicHistory, setPublicHistory] = useState(true);
  useEffect(() => {
    if (settings.data) { setSiteName(settings.data.siteName); setPublicHistory(settings.data.publicRevisionHistory); }
  }, [settings.data]);
  const save = useMutation({
    mutationFn: () => api.saveSettings({ siteName: siteName.trim(), publicRevisionHistory: publicHistory }),
    onSuccess: () => { void qc.invalidateQueries({ queryKey: ['settings'] }); void qc.invalidateQueries({ queryKey: ['meta'] }); toast('Settings saved.'); },
    onError: (e: Error) => toast(e.message, 'error'),
  });
  if (settings.isPending) return <div className="loading">Loading…</div>;
  return (
    <section className="panel">
      <div className="panel-body" style={{ display: 'grid', gap: 14, maxWidth: 520 }}>
        <div className="field"><label htmlFor="s-name">Company name in the header (optional)</label>
          <input id="s-name" className="input" value={siteName} maxLength={100} onChange={(e) => setSiteName(e.target.value)} />
          <span className="hint">Leave blank to show only "Product Specification".</span></div>
        <label className="check"><input type="checkbox" checked={publicHistory} onChange={(e) => setPublicHistory(e.target.checked)} />
          Allow viewers without login to see revision history</label>
        <div><button className="btn btn-primary" onClick={() => save.mutate()} disabled={save.isPending}>Save Settings</button></div>
      </div>
    </section>
  );
}

function AuditTab() {
  const [page, setPage] = useState(1);
  const audit = useQuery({ queryKey: ['audit', page], queryFn: () => api.audit({ page, pageSize: 50 }) });
  return (
    <section className="panel">
      {audit.isError && <div className="alert alert-error">{(audit.error as Error).message}</div>}
      <div className="table-wrap">
        <table className="grid stack">
          <thead><tr><th>Date / Time</th><th>User</th><th>Product</th><th>Details</th></tr></thead>
          <tbody>
            {audit.data?.map((a) => (
              <tr key={a.id}>
                <td className="nowrap stack-title" style={{ fontSize: 13 }}>{formatDateTime(a.at)}</td>
                <td data-label="User">{a.userName}</td>
                <td data-label="Product" className="mono nowrap">{a.productId && a.productCode ? <Link to={`/products/${a.productId}`}>{a.productCode}</Link> : (a.productCode ?? '—')}</td>
                <td data-label="Details" style={{ wordBreak: 'break-word' }}>{a.detail}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <div className="pager">
        <button className="btn btn-sm" disabled={page <= 1} onClick={() => setPage(page - 1)}>Newer</button>
        <span className="muted">Page {page}</span>
        <button className="btn btn-sm" disabled={(audit.data?.length ?? 0) < 50} onClick={() => setPage(page + 1)}>Older</button>
      </div>
    </section>
  );
}
