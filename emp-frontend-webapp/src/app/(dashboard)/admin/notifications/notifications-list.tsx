'use client';

import React, { useEffect, useState } from 'react';
import { useNotificationStore } from '@/store/use-notification-store';
import type { Notification } from '@/store/use-notification-store';

const PRIORITY_BADGE: Record<string, string> = {
  HIGH: 'bg-red-100 text-red-800',
  MEDIUM: 'bg-yellow-100 text-yellow-800',
  LOW: 'bg-gray-100 text-gray-700',
};

const TYPE_LABEL: Record<string, string> = {
  ProjectBriefReceived: 'Brief Received',
  ProposalStatusChanged: 'Proposal Update',
  PaymentReceived: 'Payment',
  PayoutSent: 'Payout',
  MilestoneApprovalNeeded: 'Milestone Approval',
  General: 'General',
};

function formatRelativeTime(dateString: string): string {
  const now = Date.now();
  const date = new Date(dateString).getTime();
  const diff = now - date;
  const minutes = Math.floor(diff / 60000);
  if (minutes < 1) return 'Just now';
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days}d ago`;
  return new Date(dateString).toLocaleDateString();
}

export function NotificationsList() {
  const { notifications, unreadCount, fetchNotifications, markAsRead, markAllAsRead } = useNotificationStore();
  const [loading, setLoading] = useState(true);
  const [filter, setFilter] = useState<'all' | 'unread'>('all');

  useEffect(() => {
    fetchNotifications().finally(() => setLoading(false));
  }, [fetchNotifications]);

  const filtered = filter === 'unread'
    ? notifications.filter((n) => !n.isRead)
    : notifications;

  if (loading) {
    return (
      <div className="bg-white shadow rounded-lg p-8 text-center">
        <p className="text-sm text-gray-500">Loading notifications...</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <div className="flex space-x-2">
          <button
            onClick={() => setFilter('all')}
            className={`px-3 py-1.5 text-sm font-medium rounded-md ${filter === 'all' ? 'bg-blue-100 text-blue-800' : 'text-gray-600 hover:bg-gray-100'}`}
          >
            All
          </button>
          <button
            onClick={() => setFilter('unread')}
            className={`px-3 py-1.5 text-sm font-medium rounded-md ${filter === 'unread' ? 'bg-blue-100 text-blue-800' : 'text-gray-600 hover:bg-gray-100'}`}
          >
            Unread {unreadCount > 0 && `(${unreadCount})`}
          </button>
        </div>
        {unreadCount > 0 && (
          <button
            onClick={markAllAsRead}
            className="text-sm text-blue-600 hover:text-blue-800 font-medium"
          >
            Mark all as read
          </button>
        )}
      </div>

      {filtered.length === 0 ? (
        <div className="bg-white shadow rounded-lg p-8 text-center">
          <p className="text-sm text-gray-500">
            {filter === 'unread' ? 'No unread notifications.' : 'No notifications yet.'}
          </p>
        </div>
      ) : (
        <div className="bg-white shadow rounded-lg divide-y divide-gray-200">
          {filtered.map((notification: Notification) => (
            <div
              key={notification.id}
              className={`px-4 py-4 sm:px-6 flex items-start space-x-4 ${!notification.isRead ? 'bg-blue-50/50' : ''}`}
            >
              <div className={`mt-1 flex-shrink-0 w-2 h-2 rounded-full ${!notification.isRead ? 'bg-blue-500' : 'bg-transparent'}`} />
              <div className="flex-1 min-w-0">
                <div className="flex items-center space-x-2">
                  <p className="text-sm font-medium text-gray-900 truncate">{notification.title}</p>
                  <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${PRIORITY_BADGE[notification.priority] || PRIORITY_BADGE.LOW}`}>
                    {notification.priority}
                  </span>
                  <span className="text-xs text-gray-400">
                    {TYPE_LABEL[notification.type] || notification.type}
                  </span>
                </div>
                <p className="mt-1 text-sm text-gray-600">{notification.message}</p>
                <p className="mt-1 text-xs text-gray-400">{formatRelativeTime(notification.createdAt)}</p>
              </div>
              {!notification.isRead && (
                <button
                  onClick={() => markAsRead(notification.id)}
                  className="flex-shrink-0 text-xs text-blue-600 hover:text-blue-800"
                >
                  Mark read
                </button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
