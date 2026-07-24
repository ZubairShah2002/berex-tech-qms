import { type ReactNode } from 'react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import {
  Menu,
  LayoutDashboard,
  FileText,
  ClipboardCheck,
  AlertTriangle,
  Shield,
  Search,
  Users,
  Wrench,
  GraduationCap,
  Package,
  BarChart3,
  Settings,
  ChevronLeft,
  LogOut,
} from 'lucide-react'
import { cn } from '@/lib/cn'
import { useUiStore } from '@/stores/ui-store'
import { useAuthStore } from '@/stores/auth-store'
import styles from './AppShell.module.css'

interface NavEntry {
  label: string
  path: string
  icon: ReactNode
}

const navigation: NavEntry[] = [
  { label: 'Dashboard', path: '/', icon: <LayoutDashboard size={18} /> },
  { label: 'Document Control', path: '/documents', icon: <FileText size={18} /> },
  { label: 'Inspections', path: '/inspections', icon: <ClipboardCheck size={18} /> },
  { label: 'Non-Conformances', path: '/nonconformances', icon: <AlertTriangle size={18} /> },
  { label: 'CAPA', path: '/capa', icon: <Shield size={18} /> },
  { label: 'Audits', path: '/audits', icon: <Search size={18} /> },
  { label: 'Suppliers', path: '/suppliers', icon: <Users size={18} /> },
  { label: 'Calibration', path: '/calibration', icon: <Wrench size={18} /> },
  { label: 'Training', path: '/training', icon: <GraduationCap size={18} /> },
  { label: 'Product Catalog', path: '/products', icon: <Package size={18} /> },
  { label: 'SPC', path: '/spc', icon: <BarChart3 size={18} /> },
  { label: 'Settings', path: '/settings', icon: <Settings size={18} /> },
]

export function AppShell() {
  const navigate = useNavigate()
  const { sidebarOpen, sidebarCollapsed, toggleSidebar, toggleSidebarCollapsed } = useUiStore()
  const user = useAuthStore((s) => s.user)
  const clearAuth = useAuthStore((s) => s.clearAuth)

  function handleLogout() {
    clearAuth()
    navigate('/login', { replace: true })
  }

  const initials = user
    ? `${user.firstName.charAt(0)}${user.lastName.charAt(0)}`
    : 'BQ'

  return (
    <div className={styles.shell}>
      <aside
        className={cn(
          styles.sidebar,
          sidebarCollapsed && styles.sidebarCollapsed,
          !sidebarOpen && styles.sidebarHidden,
        )}
      >
        <div className={styles.logo}>
          <Shield size={24} />
          {!sidebarCollapsed && <span className={styles.logoText}>Berex QMS</span>}
        </div>

        <nav className={styles.nav}>
          {navigation.map((item) => (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === '/'}
              className={({ isActive }) =>
                cn(styles.navItem, isActive && styles.navItemActive)
              }
            >
              {item.icon}
              {!sidebarCollapsed && <span className={styles.navLabel}>{item.label}</span>}
            </NavLink>
          ))}
        </nav>

        <div className={styles.sidebarFooter}>
          {!sidebarCollapsed && (
            <div className={styles.userInfo}>
              <div className={styles.avatar}>{initials}</div>
              <div className={styles.userDetails}>
                <div className={styles.userName}>
                  {user ? `${user.firstName} ${user.lastName}` : 'System User'}
                </div>
                <div className={styles.userRole}>
                  {user?.roles[0] ?? 'Administrator'}
                </div>
              </div>
            </div>
          )}
          {sidebarCollapsed && <div className={styles.avatar}>{initials}</div>}
        </div>
      </aside>

      <header className={styles.header}>
        <div className={styles.headerLeft}>
          <button
            className={styles.menuButton}
            onClick={sidebarOpen ? toggleSidebarCollapsed : toggleSidebar}
            aria-label="Toggle sidebar"
          >
            {sidebarCollapsed ? <Menu size={18} /> : <ChevronLeft size={18} />}
          </button>
        </div>
        <div className={styles.headerRight}>
          {user && (
            <span className={styles.headerUser}>
              {user.firstName} {user.lastName}
            </span>
          )}
          <button
            className={styles.logoutButton}
            onClick={handleLogout}
            aria-label="Sign out"
          >
            <LogOut size={16} />
            <span>Sign out</span>
          </button>
        </div>
      </header>

      <main className={styles.content}>
        <Outlet />
      </main>
    </div>
  )
}
