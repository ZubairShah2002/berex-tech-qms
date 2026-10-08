import { createContext, useCallback, useContext, useState, type ReactNode } from 'react';

interface ToastItem { id: number; text: string; kind: 'ok' | 'error' }
const ToastContext = createContext<(text: string, kind?: 'ok' | 'error') => void>(() => undefined);

export function ToastProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<ToastItem[]>([]);
  const show = useCallback((text: string, kind: 'ok' | 'error' = 'ok') => {
    const id = Date.now() + Math.random();
    setItems((xs) => [...xs, { id, text, kind }]);
    setTimeout(() => setItems((xs) => xs.filter((x) => x.id !== id)), kind === 'error' ? 6000 : 3500);
  }, []);
  return (
    <ToastContext.Provider value={show}>
      {children}
      <div className="toast-wrap" role="status" aria-live="polite">
        {items.map((t) => (
          <div key={t.id} className={`toast${t.kind === 'error' ? ' toast-error' : ''}`}>{t.text}</div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export const useToast = () => useContext(ToastContext);
