import { create } from 'zustand';
import { devtools } from 'zustand/middleware';

interface ToastPayload {
  title?: string;
  message: string;
  type?: 'success' | 'error' | 'info' | 'warning';
}

interface UiState {
  isSidebarOpen: boolean;
  isSidebarCollapsed: boolean;
  isMobileMenuOpen: boolean;
  activeModalId: string | null;
  modalProps: Record<string, unknown>;

  toggleSidebar: () => void;
  collapseSidebar: (collapsed: boolean) => void;
  toggleMobileMenu: () => void;
  closeMobileMenu: () => void;
  openModal: (modalId: string, props?: Record<string, unknown>) => void;
  closeModal: () => void;
  showToast: (payload: ToastPayload) => void;
  reset: () => void;
}

export const useUiStore = create<UiState>()(
  devtools(
    (set) => ({
      isSidebarOpen: true,
      isSidebarCollapsed: false,
      isMobileMenuOpen: false,
      activeModalId: null,
      modalProps: {},

      toggleSidebar: () =>
        set((state) => ({ isSidebarOpen: !state.isSidebarOpen })),

      collapseSidebar: (collapsed: boolean) =>
        set(() => ({ isSidebarCollapsed: collapsed })),

      toggleMobileMenu: () =>
        set((state) => ({ isMobileMenuOpen: !state.isMobileMenuOpen })),

      closeMobileMenu: () =>
        set(() => ({ isMobileMenuOpen: false })),

      openModal: (modalId: string, props = {}) =>
        set(() => ({
          activeModalId: modalId,
          modalProps: props,
        })),

      closeModal: () =>
        set(() => ({
          activeModalId: null,
          modalProps: {},
        })),

      showToast: (_payload: ToastPayload) => {
        // Toast display bridged through the notification store.
        // This method exists for the polling hook integration.
      },

      reset: () =>
        set(() => ({
          isSidebarOpen: true,
          isSidebarCollapsed: false,
          isMobileMenuOpen: false,
          activeModalId: null,
          modalProps: {},
        })),
    }),
    { name: 'ui-store' }
  )
);
