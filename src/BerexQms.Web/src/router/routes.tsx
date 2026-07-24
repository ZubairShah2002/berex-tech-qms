import { createBrowserRouter } from 'react-router-dom'
import { AppShell } from '@/components/layout/AppShell'
import { DashboardPage } from '@/pages/DashboardPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { ModulePlaceholder } from '@/pages/ModulePlaceholder'

export const router = createBrowserRouter([
  {
    path: '/',
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
      { path: 'products', element: <ModulePlaceholder moduleName="Product Catalog" /> },
      { path: 'spc', element: <ModulePlaceholder moduleName="Statistical Process Control" /> },
      { path: 'settings', element: <ModulePlaceholder moduleName="Settings" /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
])
