import React from 'react';
import Link from 'next/link';
import { ProjectService } from '@/services/project.service';
import { formatDate } from '@/lib/utils';
import type { ProjectDTO } from '@/lib/types';

export const dynamic = 'force-dynamic';

const STATUS_BADGE: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  SowProcessing: 'bg-blue-100 text-blue-800',
  ReviewPending: 'bg-orange-100 text-orange-800',
  Proposed: 'bg-purple-100 text-purple-800',
  Awarded: 'bg-indigo-100 text-indigo-800',
  Active: 'bg-green-100 text-green-800',
  OnHold: 'bg-gray-100 text-gray-800',
  Completed: 'bg-teal-100 text-teal-800',
  Cancelled: 'bg-red-100 text-red-800',
};

export default async function ProjectsPage() {
  let projects: ProjectDTO[] = [];
  let error: string | null = null;

  try {
    const result = await ProjectService.getProjects({ pageSize: 50 });
    projects = result.items;
  } catch {
    error = 'Failed to load projects.';
  }

  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">Projects</h1>
          <p className="mt-2 text-sm text-gray-700">
            Manage all projects across the platform lifecycle.
          </p>
        </div>
        <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
          <Link
            href="/admin/projects/new"
            className="inline-flex items-center justify-center rounded-md border border-transparent bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700"
          >
            New Project
          </Link>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded text-sm">{error}</div>
      )}

      <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
        <table className="min-w-full divide-y divide-gray-300">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">Project</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Client</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Status</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Created</th>
              <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                <span className="sr-only">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {projects.map((project) => (
              <tr key={project.id}>
                <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm sm:pl-6">
                  <Link href={`/admin/projects/${project.id}`} className="font-medium text-blue-600 hover:text-blue-800">
                    {project.name}
                  </Link>
                  {project.description && (
                    <p className="text-gray-500 truncate max-w-xs">{project.description}</p>
                  )}
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{project.clientName}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm">
                  <span className={`inline-flex rounded-full px-2 text-xs font-semibold leading-5 ${STATUS_BADGE[project.status] || 'bg-gray-100 text-gray-800'}`}>
                    {project.status}
                  </span>
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{formatDate(project.createdAt)}</td>
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  {(project.status === 'ReviewPending' || project.status === 'SowProcessing') && (
                    <Link href={`/admin/sow-review/${project.id}`} className="text-blue-600 hover:text-blue-900 mr-3">
                      Review SOW
                    </Link>
                  )}
                  {project.status === 'Proposed' && (
                    <Link href={`/admin/proposals/${project.id}`} className="text-blue-600 hover:text-blue-900 mr-3">
                      View Proposals
                    </Link>
                  )}
                  <Link href={`/admin/projects/${project.id}`} className="text-gray-600 hover:text-gray-900">
                    Details
                  </Link>
                </td>
              </tr>
            ))}
            {projects.length === 0 && !error && (
              <tr>
                <td colSpan={5} className="py-8 text-center text-sm text-gray-500">No projects found.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
