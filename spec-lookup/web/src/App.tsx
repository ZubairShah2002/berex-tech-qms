import { Route, Routes } from 'react-router-dom';
import { AuthProvider, RequireAuth } from './auth';
import { Layout } from './components/Layout';
import { ToastProvider } from './components/Toast';
import { HomePage } from './pages/HomePage';
import { ProductsPage } from './pages/ProductsPage';
import { ProductPage } from './pages/ProductPage';
import { ProductFormPage } from './pages/ProductFormPage';
import { LoginPage } from './pages/LoginPage';
import { ImportPage } from './pages/ImportPage';
import { AdminPage } from './pages/AdminPage';
import { AccountPage } from './pages/AccountPage';
import { NotFoundPage } from './pages/NotFoundPage';

export function App() {
  return (
    <AuthProvider>
      <ToastProvider>
        <Layout>
          <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/products" element={<ProductsPage />} />
            <Route path="/products/new" element={<RequireAuth><ProductFormPage mode="new" /></RequireAuth>} />
            <Route path="/products/:id" element={<ProductPage />} />
            <Route path="/products/:id/edit" element={<RequireAuth><ProductFormPage mode="edit" /></RequireAuth>} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/import" element={<RequireAuth importer><ImportPage /></RequireAuth>} />
            <Route path="/admin" element={<RequireAuth admin><AdminPage /></RequireAuth>} />
            <Route path="/account" element={<RequireAuth><AccountPage /></RequireAuth>} />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </Layout>
      </ToastProvider>
    </AuthProvider>
  );
}
