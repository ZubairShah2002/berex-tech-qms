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
import { InspectionsListPage } from '@/pages/inspections/InspectionsListPage'
import { InspectionDetailPage } from '@/pages/inspections/InspectionDetailPage'
import { InspectionCreatePage } from '@/pages/inspections/InspectionCreatePage'
import { NonConformancesListPage } from '@/pages/nonconformances/NonConformancesListPage'
import { NonConformanceCreatePage } from '@/pages/nonconformances/NonConformanceCreatePage'
import { NonConformanceDetailPage } from '@/pages/nonconformances/NonConformanceDetailPage'
import { CapaListPage } from '@/pages/capa/CapaListPage'
import { CapaCreatePage } from '@/pages/capa/CapaCreatePage'
import { CapaDetailPage } from '@/pages/capa/CapaDetailPage'

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
          { path: 'inspections', element: <InspectionsListPage /> },
          { path: 'inspections/new', element: <InspectionCreatePage /> },
          { path: 'inspections/:id', element: <InspectionDetailPage /> },
          { path: 'nonconformances', element: <NonConformancesListPage /> },
          { path: 'nonconformances/new', element: <NonConformanceCreatePage /> },
          { path: 'nonconformances/:id', element: <NonConformanceDetailPage /> },
          { path: 'capa', element: <CapaListPage /> },
          { path: 'capa/new', element: <CapaCreatePage /> },
          { path: 'capa/:id', element: <CapaDetailPage /> },
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
