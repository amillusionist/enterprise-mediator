'use client';

import React, { useState } from 'react';
import { useNotificationStore } from '@/store/use-notification-store';
import { useNotificationPolling } from '@/hooks/use-notification-polling';
import { formatDistanceToNow } from 'date-fns';

/**
 * NotificationCenter Component
 *
 * Displays a bell icon with an unread badge.
 * Expands into a dropdown list of recent notifications.
 * Uses polling hook to keep data fresh.
 */
export function NotificationCenter() {
  useNotificationPolling();

  const notifications = useNotificationStore((s) => s.notifications);
  const unreadCount = useNotificationStore((s) => s.unreadCount);
  const markAsRead = useNotificationStore((s) => s.markAsRead);
  const markAllAsRead = useNotificationStore((s) => s.markAllAsRead);
  const removeNotification = useNotificationStore((s) => s.removeNotification);

  const [isOpen, setIsOpen] = useState(false);

  const toggleDropdown = () => setIsOpen(!isOpen);

  const handleMarkAsRead = (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    markAsRead(id);
  };

  const handleRemove = (id: string, e: React.MouseEvent) => {
    e.stopPropagation();
    removeNotification(id);
  };

  return (
    <div className="relative z-50">
      <button
        onClick={toggleDropdown}
        className="relative p-2 text-slate-500 hover:text-slate-700 focus:outline-none focus:ring-2 focus:ring-slate-200 rounded-full transition-colors"
        aria-label="Notifications"
        aria-expanded={isOpen}
      >
        <svg className="h-6 w-6" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
          <path strokeLinecap="round" strokeLinejoin="round" d="M14.857 17.082a23.848 23.848 0 005.454-1.31A8.967 8.967 0 0118 9.75v-.7V9A6 6 0 006 9v.75a8.967 8.967 0 01-2.312 6.022c1.733.64 3.56 1.085 5.455 1.31m5.714 0a24.255 24.255 0 01-5.714 0m5.714 0a3 3 0 11-5.714 0" />
        </svg>
        {unreadCount > 0 && (
          <span className="absolute top-1 right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white ring-2 ring-white">
            {unreadCount > 9 ? '9+' : unreadCount}
          </span>
        )}
      </button>

      {isOpen && (
        <>
          <div
            className="fixed inset-0 z-40"
            onClick={() => setIsOpen(false)}
            aria-hidden="true"
          />

          <div className="absolute right-0 mt-2 w-80 sm:w-96 origin-top-right rounded-lg bg-white shadow-lg ring-1 ring-black ring-opacity-5 focus:outline-none z-50 overflow-hidden">
            <div className="px-4 py-3 border-b border-slate-100 flex justify-between items-center bg-slate-50">
              <h3 className="text-sm font-semibold text-slate-900">Notifications</h3>
              {unreadCount > 0 && (
                <button
                  onClick={() => markAllAsRead()}
                  className="text-xs text-blue-600 hover:text-blue-800 font-medium"
                >
                  Mark all read
                </button>
              )}
            </div>

            <div className="max-h-[400px] overflow-y-auto">
              {notifications.length === 0 ? (
                <div className="px-4 py-8 text-center text-slate-500 text-sm">
                  No notifications yet.
                </div>
              ) : (
                <ul className="divide-y divide-slate-100">
                  {notifications.map((notification) => (
                    <li
                      key={notification.id}
                      className={`group relative px-4 py-3 hover:bg-slate-50 transition-colors ${
                        !notification.isRead ? 'bg-blue-50/50' : ''
                      }`}
                    >
                      <div className="flex justify-between items-start">
                        <div className="flex-1 pr-4">
                          <p className={`text-sm ${!notification.isRead ? 'font-semibold text-slate-900' : 'text-slate-700'}`}>
                            {notification.title}
                          </p>
                          <p className="text-xs text-slate-500 mt-1">
                            {notification.message}
                          </p>
                          {notification.createdAt && (
                            <p className="text-[10px] text-slate-400 mt-2">
                              {formatDistanceToNow(new Date(notification.createdAt), { addSuffix: true })}
                            </p>
                          )}
                        </div>

                        <div className="flex items-center space-x-1 opacity-0 group-hover:opacity-100 transition-opacity">
                          {!notification.isRead && (
                            <button
                              onClick={(e) => handleMarkAsRead(notification.id, e)}
                              className="p-1 text-slate-400 hover:text-blue-600 rounded"
                              title="Mark as read"
                            >
                              <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                                <path strokeLinecap="round" strokeLinejoin="round" d="M4.5 12.75l6 6 9-13.5" />
                              </svg>
                            </button>
                          )}
                          <button
                            onClick={(e) => handleRemove(notification.id, e)}
                            className="p-1 text-slate-400 hover:text-red-600 rounded"
                            title="Remove"
                          >
                            <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
                            </svg>
                          </button>
                        </div>
                      </div>

                      {!notification.isRead && (
                        <div className="absolute left-2 top-4 h-2 w-2 rounded-full bg-blue-500" />
                      )}
                    </li>
                  ))}
                </ul>
              )}
            </div>

            <div className="bg-slate-50 px-4 py-2 border-t border-slate-100 text-center">
              <a href="/admin/notifications" className="text-xs text-slate-600 hover:text-slate-900 font-medium">
                View all notifications
              </a>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

export default NotificationCenter;
