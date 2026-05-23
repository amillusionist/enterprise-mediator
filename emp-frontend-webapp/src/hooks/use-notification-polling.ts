'use client';

import { useEffect, useRef } from 'react';
import { useNotificationStore } from '@/store/use-notification-store';

interface PollingOptions {
  enabled?: boolean;
  intervalMs?: number;
  entityId?: string;
}

/**
 * Hook to poll for new notifications or specific entity status updates.
 * Used for real-time feedback on long-running processes like SOW ingestion.
 */
export function useNotificationPolling({
  enabled = true,
  intervalMs = 10000,
  entityId,
}: PollingOptions = {}) {
  const fetchNotifications = useNotificationStore((s) => s.fetchNotifications);
  const unreadCount = useNotificationStore((s) => s.unreadCount);
  const timerRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    if (!enabled) {
      if (timerRef.current) clearInterval(timerRef.current);
      return;
    }

    const poll = async () => {
      try {
        await fetchNotifications(entityId);
      } catch (error) {
        console.error('Notification polling failed:', error);
      }
    };

    poll();

    timerRef.current = setInterval(poll, intervalMs);

    return () => {
      if (timerRef.current) clearInterval(timerRef.current);
    };
  }, [enabled, intervalMs, entityId, fetchNotifications]);

  return {
    unreadCount,
  };
}
