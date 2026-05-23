import React from 'react';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { ProjectService } from '@/services/project.service';
import { formatDate, formatCurrency } from '@/lib/utils';

interface ProjectDetailPageProps {
  params: {
    projectId: string;
  };
}

const STATUS_BADGE: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  SowProcessing: 'bg-blue-100 text-blue-800',
  ReviewPending: 'bg-orange-100 text-orange-800',
  Proposed: 'bg-indigo-100 text-indigo-800',
  Awarded: 'bg-purple-100 text-purple-800',
  Active: 'bg-green-100 text-green-800',
  OnHold: 'bg-gray-100 text-gray-800',
  Completed: 'bg-emerald-100 text-emerald-800',
  Cancelled: 'bg-red-100 text-red-800',
};

export default async function ProjectDetailPage({ params }: ProjectDetailPageProps) {
  const { projectId } = params;

  if (!projectId) notFound();

  let project;
  let milestones;

  try {
    [project, milestones] = await Promise.all([
      ProjectService.getProjectById(projectId),
      ProjectService.getMilestones(projectId),
    ]);
  } catch {
    notFound();
  }

  if (!project) notFound();

  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-gray-900">{project.name}</h1>
            <span className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold ${STATUS_BADGE[project.status] || 'bg-gray-100 text-gray-800'}`}>
              {project.status}
            </span>
          </div>
          <p className="mt-1 text-sm text-gray-500">
            Client: {project.clientName} | Created: {formatDate(project.createdAt)}
          </p>
        </div>
        <div className="mt-4 sm:mt-0 flex gap-2">
          <Link href="/admin/projects" className="text-sm text-blue-600 hover:text-blue-800">
            &larr; Back to Projects
          </Link>
        </div>
      </div>

      {project.description && (
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-sm font-medium text-gray-500 mb-2">Description</h2>
          <p className="text-sm text-gray-900">{project.description}</p>
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {project.budget != null && (
          <div className="bg-white shadow rounded-lg p-6">
            <h3 className="text-sm font-medium text-gray-500">Total Budget</h3>
            <p className="mt-1 text-2xl font-semibold text-gray-900">
              {formatCurrency(project.budget, project.currency || 'USD')}
            </p>
          </div>
        )}
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-sm font-medium text-gray-500">Milestones</h3>
          <p className="mt-1 text-2xl font-semibold text-gray-900">{milestones?.length || 0}</p>
        </div>
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-sm font-medium text-gray-500">Vendor</h3>
          <p className="mt-1 text-lg font-semibold text-gray-900">{project.awardedVendorName || 'Not assigned'}</p>
        </div>
      </div>

      {/* Quick Actions */}
      <div className="bg-white shadow rounded-lg p-6">
        <h2 className="text-lg font-medium text-gray-900 mb-4">Actions</h2>
        <div className="flex flex-wrap gap-3">
          {(project.status === 'Pending' || project.status === 'SowProcessing' || project.status === 'ReviewPending') && (
            <Link
              href={`/admin/sow-review/${projectId}`}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700"
            >
              Review SOW
            </Link>
          )}
          {project.status === 'Proposed' && (
            <Link
              href={`/admin/proposals/${projectId}`}
              className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700"
            >
              Compare Proposals
            </Link>
          )}
        </div>
      </div>

      {/* Milestones Table */}
      {milestones && milestones.length > 0 && (
        <div className="bg-white shadow rounded-lg overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-200">
            <h2 className="text-lg font-medium text-gray-900">Milestones</h2>
          </div>
          <table className="min-w-full divide-y divide-gray-300">
            <thead className="bg-gray-50">
              <tr>
                <th scope="col" className="py-3.5 pl-6 pr-3 text-left text-sm font-semibold text-gray-900">Name</th>
                <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Amount</th>
                <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Status</th>
                <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Due Date</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200 bg-white">
              {milestones.map((m) => (
                <tr key={m.id}>
                  <td className="py-4 pl-6 pr-3 text-sm font-medium text-gray-900">{m.title}</td>
                  <td className="px-3 py-4 text-sm text-gray-500">{formatCurrency(m.amount, m.currency)}</td>
                  <td className="px-3 py-4 text-sm">
                    <span className={`inline-flex rounded-full px-2 text-xs font-semibold leading-5 ${
                      m.status === 'Approved' ? 'bg-green-100 text-green-800' :
                      m.status === 'Pending' ? 'bg-yellow-100 text-yellow-800' :
                      m.status === 'Rejected' ? 'bg-red-100 text-red-800' :
                      'bg-gray-100 text-gray-800'
                    }`}>
                      {m.status}
                    </span>
                  </td>
                  <td className="px-3 py-4 text-sm text-gray-500">{m.dueDate ? formatDate(m.dueDate) : '-'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
