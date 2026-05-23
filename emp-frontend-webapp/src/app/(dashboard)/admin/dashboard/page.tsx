import React from 'react';
import Link from 'next/link';
import { FinanceService } from '@/services/finance.service';
import { formatCurrency, formatDate } from '@/lib/utils';
import type { DashboardMetrics } from '@/lib/types';

export const dynamic = 'force-dynamic';

export default async function DashboardPage() {
  let metrics: DashboardMetrics | null = null;
  let error: string | null = null;

  try {
    metrics = await FinanceService.getDashboardMetrics();
  } catch {
    error = 'Failed to load dashboard metrics.';
  }

  const statCards = metrics ? [
    { label: 'Active Projects', value: metrics.activeProjectsCount, href: '/admin/projects?status=Active', color: 'bg-blue-500' },
    { label: 'Pending SOWs', value: metrics.pendingSowCount, href: '/admin/projects?status=SowProcessing', color: 'bg-yellow-500' },
    { label: 'Proposals Awaiting', value: metrics.proposalsAwaitingCount, href: '/admin/projects?status=Proposed', color: 'bg-purple-500' },
    { label: 'Total Revenue', value: formatCurrency(metrics.totalRevenue), href: '/admin/finance/transactions', color: 'bg-green-500' },
    { label: 'Net Profit', value: formatCurrency(metrics.netProfit), href: '/admin/finance/transactions', color: 'bg-emerald-500' },
    { label: 'Total Payouts', value: formatCurrency(metrics.totalPayouts), href: '/admin/finance/payouts', color: 'bg-orange-500' },
  ] : [];

  return (
    <div className="space-y-8">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Dashboard</h1>
        <p className="mt-1 text-sm text-gray-500">Overview of platform activity and key metrics.</p>
      </div>

      {error && (
        <div className="bg-yellow-50 border border-yellow-200 text-yellow-800 px-4 py-3 rounded text-sm">
          {error}
        </div>
      )}

      {metrics && (
        <>
          <div className="grid grid-cols-1 gap-5 sm:grid-cols-2 lg:grid-cols-3">
            {statCards.map((card) => (
              <Link key={card.label} href={card.href} className="bg-white overflow-hidden shadow rounded-lg hover:shadow-md transition-shadow">
                <div className="p-5">
                  <div className="flex items-center">
                    <div className={`flex-shrink-0 ${card.color} rounded-md p-3`}>
                      <div className="h-6 w-6 text-white" />
                    </div>
                    <div className="ml-5 w-0 flex-1">
                      <dl>
                        <dt className="text-sm font-medium text-gray-500 truncate">{card.label}</dt>
                        <dd className="text-lg font-semibold text-gray-900">{card.value}</dd>
                      </dl>
                    </div>
                  </div>
                </div>
              </Link>
            ))}
          </div>

          {metrics.upcomingMilestones.length > 0 && (
            <div className="bg-white shadow rounded-lg">
              <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
                <h3 className="text-lg font-medium text-gray-900">Upcoming Milestones</h3>
              </div>
              <ul className="divide-y divide-gray-200">
                {metrics.upcomingMilestones.slice(0, 5).map((milestone) => (
                  <li key={milestone.id} className="px-4 py-4 sm:px-6">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="text-sm font-medium text-gray-900">{milestone.title}</p>
                        <p className="text-sm text-gray-500">Project: {milestone.projectId}</p>
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-semibold text-gray-900">{formatCurrency(milestone.amount, milestone.currency)}</p>
                        {milestone.dueDate && (
                          <p className="text-xs text-gray-500">Due: {formatDate(milestone.dueDate)}</p>
                        )}
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          )}

          {metrics.recentActivity.length > 0 && (
            <div className="bg-white shadow rounded-lg">
              <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
                <h3 className="text-lg font-medium text-gray-900">Recent Activity</h3>
              </div>
              <ul className="divide-y divide-gray-200">
                {metrics.recentActivity.slice(0, 10).map((log) => (
                  <li key={log.id} className="px-4 py-3 sm:px-6 flex items-center justify-between">
                    <div>
                      <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-gray-100 text-gray-800 mr-2">
                        {log.actionType}
                      </span>
                      <span className="text-sm text-gray-700">
                        {log.entityType} {log.entityName ? `"${log.entityName}"` : ''}
                      </span>
                    </div>
                    <span className="text-xs text-gray-500">{formatDate(log.timestamp)}</span>
                  </li>
                ))}
              </ul>
              <div className="px-4 py-3 border-t border-gray-200 text-center">
                <Link href="/admin/audit-trail" className="text-sm text-blue-600 hover:text-blue-800 font-medium">
                  View full audit trail
                </Link>
              </div>
            </div>
          )}
        </>
      )}
    </div>
  );
}
