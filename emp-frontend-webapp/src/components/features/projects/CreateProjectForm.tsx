'use client';

import React, { useState, useTransition } from 'react';
import { useRouter } from 'next/navigation';
import { createProjectAction } from '@/actions/project.actions';

interface CreateProjectFormProps {
  clients: { id: string; companyName: string }[];
}

export function CreateProjectForm({ clients }: CreateProjectFormProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const formData = new FormData(e.currentTarget);
    const name = formData.get('name') as string;
    const clientId = formData.get('clientId') as string;
    const description = formData.get('description') as string;

    if (!name || !clientId) {
      setError('Project name and client are required.');
      return;
    }

    startTransition(async () => {
      try {
        const result = await createProjectAction({ name, clientId, description });
        if (result.success && result.data) {
          router.push(`/admin/projects`);
        } else {
          setError(result.message || 'Failed to create project.');
        }
      } catch {
        setError('An unexpected error occurred.');
      }
    });
  };

  return (
    <form onSubmit={handleSubmit} className="bg-white shadow rounded-lg p-6 space-y-6 max-w-2xl">
      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded text-sm">{error}</div>
      )}

      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700">Project Name</label>
        <input
          type="text"
          name="name"
          id="name"
          required
          className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
          placeholder="e.g., Cloud Migration Phase 2"
        />
      </div>

      <div>
        <label htmlFor="clientId" className="block text-sm font-medium text-gray-700">Client</label>
        <select
          name="clientId"
          id="clientId"
          required
          className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
        >
          <option value="">Select a client...</option>
          {clients.map((c) => (
            <option key={c.id} value={c.id}>{c.companyName}</option>
          ))}
        </select>
        {clients.length === 0 && (
          <p className="mt-1 text-xs text-amber-600">No clients available. Create a client first.</p>
        )}
      </div>

      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700">Description (optional)</label>
        <textarea
          name="description"
          id="description"
          rows={3}
          className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
          placeholder="Brief description of the project scope..."
        />
      </div>

      <div className="flex justify-end">
        <button
          type="submit"
          disabled={isPending}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50"
        >
          {isPending ? 'Creating...' : 'Create Project'}
        </button>
      </div>
    </form>
  );
}
