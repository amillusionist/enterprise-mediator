import { create } from 'zustand';
import { devtools } from 'zustand/middleware';
import type { ServerNotification } from '@/lib/types';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface ToastNotification {
  id: string;
  type: ToastType;
  title: string;
  message: string;
  timestamp: string;
}

export interface Notification extends ServerNotification {
  timestamp: string;
}

export type ToastPayload = Omit<ToastNotification, 'id' | 'timestamp'>;

interface NotificationState {
  notifications: Notification[];
  toasts: ToastNotification[];
  unreadCount: number;

  addNotification: (payload: ToastPayload) => string;
  removeNotification: (id: string) => void;
  clearNotifications: () => void;
  fetchNotifications: (entityId?: string) => Promise<Notification[]>;
  markAsRead: (id: string) => void;
  markAllAsRead: () => void;
  setNotifications: (notifications: Notification[]) => void;
  dismissToast: (id: string) => void;
}

const generateId = (): string => {
  return Date.now().toString(36) + Math.random().toString(36).substring(2);
};

export const useNotificationStore = create<NotificationState>()(
  devtools(
    (set) => ({
      notifications: [],
      toasts: [],
      unreadCount: 0,

      addNotification: (payload: ToastPayload) => {
        const id = generateId();
        const toast: ToastNotification = {
          ...payload,
          id,
          timestamp: new Date().toISOString(),
        };

        set((state) => ({
          toasts: [toast, ...state.toasts].slice(0, 50),
        }));

        return id;
      },

      dismissToast: (id: string) => {
        set((state) => ({
          toasts: state.toasts.filter((t) => t.id !== id),
        }));
      },

      removeNotification: (id: string) => {
        set((state) => {
          const updated = state.notifications.filter((n) => n.id !== id);
          return {
            notifications: updated,
            unreadCount: updated.filter((n) => !n.isRead).length,
          };
        });
      },

      clearNotifications: () => {
        set(() => ({ notifications: [], unreadCount: 0 }));
      },

      fetchNotifications: async (entityId?: string) => {
        try {
          const params = new URLSearchParams();
          if (entityId) params.append('entityId', entityId);
          const queryString = params.toString();
          const endpoint = queryString
            ? `/notifications?${queryString}`
            : '/notifications';

          const baseUrl = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api/v1';
          const response = await fetch(`${baseUrl}/${endpoint.replace(/^\//, '')}`, {
            credentials: 'include',
          });

          if (!response.ok) return [];

          const data: Notification[] = await response.json();

          set(() => ({
            notifications: data,
            unreadCount: data.filter((n) => !n.isRead).length,
          }));

          return data;
        } catch {
          return [];
        }
      },

      markAsRead: (id: string) => {
        set((state) => {
          const updated = state.notifications.map((n) =>
            n.id === id ? { ...n, isRead: true } : n
          );
          return {
            notifications: updated,
            unreadCount: updated.filter((n) => !n.isRead).length,
          };
        });
      },

      markAllAsRead: () => {
        set((state) => ({
          notifications: state.notifications.map((n) => ({ ...n, isRead: true })),
          unreadCount: 0,
        }));
      },

      setNotifications: (notifications: Notification[]) => {
        set(() => ({
          notifications,
          unreadCount: notifications.filter((n) => !n.isRead).length,
        }));
      },
    }),
    { name: 'notification-store' }
  )
);
