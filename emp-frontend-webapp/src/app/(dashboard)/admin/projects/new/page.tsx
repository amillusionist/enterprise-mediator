import React from 'react';
import Link from 'next/link';
import { ClientService } from '@/services/client.service';
import { CreateProjectForm } from '@/components/features/projects/CreateProjectForm';

export default async function NewProjectPage() {
  let clients: { id: string; companyName: string }[] = [];

  try {
    const result = await ClientService.getClients({ pageSize: 100 });
    clients = result.items.map((c) => ({ id: c.id, companyName: c.companyName }));
  } catch {
    // Clients will be empty; form will show an error
  }

  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">Create New Project</h1>
          <p className="mt-2 text-sm text-gray-700">
            Set up a new project engagement. You can upload the SOW after creation.
          </p>
        </div>
        <div className="mt-4 sm:mt-0">
          <Link href="/admin/projects" className="text-sm text-blue-600 hover:text-blue-800">
            &larr; Back to Projects
          </Link>
        </div>
      </div>

      <CreateProjectForm clients={clients} />
    </div>
  );
}
