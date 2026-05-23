import React from 'react';
import Link from 'next/link';
import { redirect } from 'next/navigation';
import { AuthService } from '@/services/auth.service';
import { NotificationCenter } from '@/components/features/notifications/NotificationCenter';

interface AdminLayoutProps {
  children: React.ReactNode;
}

export default async function AdminLayout({ children }: AdminLayoutProps) {
  let userName = 'A';
  try {
    const user = await AuthService.getCurrentUser();
    if (!user) {
      redirect('/login');
    }
    userName = user.name?.charAt(0).toUpperCase() || 'A';
  } catch {
    redirect('/login');
  }

  const navigation = [
    { name: 'Dashboard', href: '/admin/dashboard' },
    { name: 'Projects', href: '/admin/projects' },
    { name: 'Clients', href: '/admin/clients' },
    { name: 'Vendors', href: '/admin/vendors' },
    { name: 'Finance', href: '/admin/finance/transactions' },
    { name: 'Audit Trail', href: '/admin/audit-trail' },
    { name: 'Users', href: '/admin/users' },
    { name: 'Settings', href: '/admin/settings/retention' },
  ];

  return (
    <div className="min-h-screen bg-gray-100 flex flex-col">
      <nav className="bg-white border-b border-gray-200 shadow-sm z-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex">
              <div className="flex-shrink-0 flex items-center">
                <span className="text-xl font-bold text-blue-600">EMP Admin</span>
              </div>
              <div className="hidden sm:ml-6 sm:flex sm:space-x-8">
                {navigation.map((item) => (
                  <Link
                    key={item.name}
                    href={item.href}
                    className="border-transparent text-gray-500 hover:border-gray-300 hover:text-gray-700 inline-flex items-center px-1 pt-1 border-b-2 text-sm font-medium"
                  >
                    {item.name}
                  </Link>
                ))}
              </div>
            </div>
            <div className="flex items-center space-x-4">
              <NotificationCenter />
              <div className="ml-3 relative">
                <div className="flex items-center">
                  <div className="h-8 w-8 rounded-full bg-blue-100 flex items-center justify-center text-blue-600 font-bold">
                    {userName}
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </nav>

      <main className="flex-1 py-10">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          {children}
        </div>
      </main>
    </div>
  );
}
