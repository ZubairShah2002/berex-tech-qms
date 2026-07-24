import { create } from 'zustand'

type DensityMode = 'office' | 'floor'

interface UiState {
  sidebarOpen: boolean
  sidebarCollapsed: boolean
  densityMode: DensityMode
  toggleSidebar: () => void
  toggleSidebarCollapsed: () => void
  setDensityMode: (mode: DensityMode) => void
}

export const useUiStore = create<UiState>((set) => ({
  sidebarOpen: true,
  sidebarCollapsed: false,
  densityMode: (localStorage.getItem('density_mode') as DensityMode) || 'office',

  toggleSidebar: () => set((s) => ({ sidebarOpen: !s.sidebarOpen })),

  toggleSidebarCollapsed: () =>
    set((s) => ({ sidebarCollapsed: !s.sidebarCollapsed })),

  setDensityMode: (mode) => {
    localStorage.setItem('density_mode', mode)
    set({ densityMode: mode })
  },
}))
