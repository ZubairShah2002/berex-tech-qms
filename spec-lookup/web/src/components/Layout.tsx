import { useEffect, useState, type ReactNode } from 'react';
import { Link, NavLink, useLocation, useNavigate } from 'react-router-dom';
import { loginPath, useAuth } from '../auth';

export function Layout({ children }: { children: ReactNode }) {
  const { user, meta, logout } = useAuth();
  const [open, setOpen] = useState(false);
  const [online, setOnline] = useState(navigator.onLine);
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => setOpen(false), [location.pathname]);
  useEffect(() => {
    const on = () => setOnline(true);
    const off = () => setOnline(false);
    window.addEventListener('online', on);
    window.addEventListener('offline', off);
    return () => { window.removeEventListener('online', on); window.removeEventListener('offline', off); };
  }, []);

  return (
    <>
      <header className={`topbar${open ? ' open' : ''}`}>
        <div className="topbar-inner">
          <Link to="/" className="brand">{meta?.siteName ?? 'Berex Tech'}<small>Product Specification</small></Link>
          <button className="menu-toggle" aria-expanded={open} onClick={() => setOpen((o) => !o)}>Menu</button>
          <nav className="nav" aria-label="Main">
            <NavLink to="/" end>Home</NavLink>
            <NavLink to="/products" end>Products</NavLink>
            <NavLink to="/products/new">Add Product</NavLink>
            {user?.canImport && <NavLink to="/import">Import</NavLink>}
            {user?.role === 'admin' && <NavLink to="/admin">Admin</NavLink>}
          </nav>
          <div className="nav-user">
            {user ? (
              <>
                <Link to="/account" title="Account">{user.userId}</Link>
                <button className="linklike" onClick={async () => { await logout(); navigate('/'); }}>Log out</button>
              </>
            ) : (
              <Link to={loginPath(location.pathname + location.search)}>Log in</Link>
            )}
          </div>
        </div>
      </header>
      {!online && <div className="offline-bar">Offline — showing saved data where available.</div>}
      <main>{children}</main>
    </>
  );
}
