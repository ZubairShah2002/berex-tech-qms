import { createContext, useContext, type ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { api, type Meta, type User } from './api';

interface AuthState {
  user: User | null;
  meta: Meta | undefined;
  loading: boolean;
  login: (userId: string, password: string) => Promise<User>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthState | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const qc = useQueryClient();
  const meta = useQuery({ queryKey: ['meta'], queryFn: api.meta, staleTime: 60_000 });
  const value: AuthState = {
    user: meta.data?.user ?? null,
    meta: meta.data,
    loading: meta.isPending,
    login: async (userId, password) => {
      const user = await api.login(userId, password);
      await qc.invalidateQueries({ queryKey: ['meta'] });
      return user;
    },
    logout: async () => {
      await api.logout().catch(() => undefined);
      qc.clear();
      await qc.invalidateQueries({ queryKey: ['meta'] });
    },
  };
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthState {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth outside AuthProvider');
  return ctx;
}

export function loginPath(next: string): string {
  return `/login?next=${encodeURIComponent(next)}`;
}

/** Renders children only for logged-in users (optionally a role); otherwise sends to the login page. */
export function RequireAuth({ children, admin, importer }: { children: ReactNode; admin?: boolean; importer?: boolean }) {
  const { user, loading } = useAuth();
  const location = useLocation();
  if (loading) return <div className="page loading">Loading…</div>;
  if (!user) return <Navigate to={loginPath(location.pathname + location.search)} replace />;
  if ((admin && user.role !== 'admin') || (importer && !user.canImport)) {
    return (
      <div className="page page-narrow">
        <div className="alert alert-error">
          {admin ? 'Administrator access is required for this page.' : 'Your account is not permitted to import products. Ask an administrator.'}
        </div>
      </div>
    );
  }
  return <>{children}</>;
}
