import React from 'react';
import Link from 'next/link';
import { CreateClientForm } from '@/components/features/clients/CreateClientForm';

export default function NewClientPage() {
  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">Add New Client</h1>
          <p className="mt-2 text-sm text-gray-700">
            Register a new client company on the platform.
          </p>
        </div>
        <div className="mt-4 sm:mt-0">
          <Link href="/admin/clients" className="text-sm text-blue-600 hover:text-blue-800">
            &larr; Back to Clients
          </Link>
        </div>
      </div>

      <CreateClientForm />
    </div>
  );
}
