import React from 'react';
import { NotificationsList } from './notifications-list';

export const dynamic = 'force-dynamic';

export default function NotificationsPage() {
  return (
    <div className="space-y-6">
      <div className="sm:flex-auto">
        <h1 className="text-2xl font-bold text-gray-900">Notifications</h1>
        <p className="mt-2 text-sm text-gray-700">
          View all system notifications and alerts.
        </p>
      </div>

      <NotificationsList />
    </div>
  );
}
