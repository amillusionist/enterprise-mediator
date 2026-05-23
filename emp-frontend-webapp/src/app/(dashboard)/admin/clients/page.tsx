import React from 'react';
import Link from 'next/link';
import { ClientService } from '@/services/client.service';
import { formatDate } from '@/lib/utils';
import type { ClientDTO } from '@/lib/types';

export const dynamic = 'force-dynamic';

export default async function ClientsPage() {
  let clients: ClientDTO[] = [];
  let error: string | null = null;

  try {
    const result = await ClientService.getClients({ pageSize: 50 });
    clients = result.items;
  } catch {
    error = 'Failed to load clients.';
  }

  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">Client Management</h1>
          <p className="mt-2 text-sm text-gray-700">
            Manage client company profiles, contacts, and billing details.
          </p>
        </div>
        <div className="mt-4 sm:mt-0 sm:ml-16 sm:flex-none">
          <Link
            href="/admin/clients/new"
            className="inline-flex items-center justify-center rounded-md border border-transparent bg-blue-600 px-4 py-2 text-sm font-medium text-white shadow-sm hover:bg-blue-700"
          >
            Add Client
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
              <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">Company</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Primary Contact</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Status</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Active Projects</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Created</th>
              <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                <span className="sr-only">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {clients.map((client) => (
              <tr key={client.id}>
                <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{client.companyName}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">
                  <div>{client.primaryContactName}</div>
                  <div className="text-xs text-gray-400">{client.primaryContactEmail}</div>
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm">
                  <span className={`inline-flex rounded-full px-2 text-xs font-semibold leading-5 ${
                    client.status === 'Active' ? 'bg-green-100 text-green-800' : 'bg-gray-100 text-gray-800'
                  }`}>
                    {client.status}
                  </span>
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{client.activeProjectsCount}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{formatDate(client.createdAt)}</td>
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  <Link href={`/admin/clients/${client.id}`} className="text-blue-600 hover:text-blue-900">
                    View
                  </Link>
                </td>
              </tr>
            ))}
            {clients.length === 0 && !error && (
              <tr>
                <td colSpan={6} className="py-8 text-center text-sm text-gray-500">No clients found.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
}
