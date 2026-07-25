import { createBrowserRouter } from 'react-router-dom'
import { AppShell } from '@/components/layout/AppShell'
import { ProtectedRoute } from '@/components/layout/ProtectedRoute'
import { DashboardPage } from '@/pages/DashboardPage'
import { LoginPage } from '@/pages/LoginPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { ModulePlaceholder } from '@/pages/ModulePlaceholder'
import { ProductsListPage } from '@/pages/products/ProductsListPage'
import { ProductDetailPage } from '@/pages/products/ProductDetailPage'
import { ProductFormPage } from '@/pages/products/ProductFormPage'

export const router = createBrowserRouter([
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/',
    element: <ProtectedRoute />,
    children: [
      {
        element: <AppShell />,
        children: [
          { index: true, element: <DashboardPage /> },
          { path: 'documents', element: <ModulePlaceholder moduleName="Document Control" /> },
          { path: 'inspections', element: <ModulePlaceholder moduleName="Inspections" /> },
          { path: 'nonconformances', element: <ModulePlaceholder moduleName="Non-Conformances" /> },
          { path: 'capa', element: <ModulePlaceholder moduleName="CAPA" /> },
          { path: 'audits', element: <ModulePlaceholder moduleName="Audit Management" /> },
          { path: 'suppliers', element: <ModulePlaceholder moduleName="Supplier Quality" /> },
          { path: 'calibration', element: <ModulePlaceholder moduleName="Calibration" /> },
          { path: 'training', element: <ModulePlaceholder moduleName="Training" /> },
          { path: 'products', element: <ProductsListPage /> },
          { path: 'products/new', element: <ProductFormPage /> },
          { path: 'products/:id', element: <ProductDetailPage /> },
          { path: 'products/:id/edit', element: <ProductFormPage /> },
          { path: 'spc', element: <ModulePlaceholder moduleName="Statistical Process Control" /> },
          { path: 'settings', element: <ModulePlaceholder moduleName="Settings" /> },
          { path: '*', element: <NotFoundPage /> },
        ],
      },
    ],
  },
])
