'use client';

import React, { useState, useTransition } from 'react';
import { useRouter } from 'next/navigation';
import { createClientAction } from '@/actions/client.actions';

export function CreateClientForm() {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const formData = new FormData(e.currentTarget);
    const companyName = formData.get('companyName') as string;
    const address = formData.get('address') as string;
    const contactName = formData.get('contactName') as string;
    const contactEmail = formData.get('contactEmail') as string;
    const contactPhone = formData.get('contactPhone') as string;

    if (!companyName || !contactName || !contactEmail) {
      setError('Company name, contact name, and email are required.');
      return;
    }

    startTransition(async () => {
      try {
        const result = await createClientAction({
          companyName,
          address: address || '',
          contacts: [{ name: contactName, email: contactEmail, phone: contactPhone || undefined }],
        });
        if (result.success) {
          router.push('/admin/clients');
        } else {
          setError(result.message || 'Failed to create client.');
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
        <label htmlFor="companyName" className="block text-sm font-medium text-gray-700">Company Name</label>
        <input
          type="text"
          name="companyName"
          id="companyName"
          required
          className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
        />
      </div>

      <div>
        <label htmlFor="address" className="block text-sm font-medium text-gray-700">Address</label>
        <input
          type="text"
          name="address"
          id="address"
          className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
        />
      </div>

      <fieldset className="border border-gray-200 rounded-md p-4">
        <legend className="text-sm font-medium text-gray-700 px-1">Primary Contact</legend>
        <div className="space-y-4 mt-2">
          <div>
            <label htmlFor="contactName" className="block text-sm font-medium text-gray-700">Name</label>
            <input type="text" name="contactName" id="contactName" required className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2" />
          </div>
          <div>
            <label htmlFor="contactEmail" className="block text-sm font-medium text-gray-700">Email</label>
            <input type="email" name="contactEmail" id="contactEmail" required className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2" />
          </div>
          <div>
            <label htmlFor="contactPhone" className="block text-sm font-medium text-gray-700">Phone (optional)</label>
            <input type="tel" name="contactPhone" id="contactPhone" className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2" />
          </div>
        </div>
      </fieldset>

      <div className="flex justify-end">
        <button
          type="submit"
          disabled={isPending}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50"
        >
          {isPending ? 'Creating...' : 'Create Client'}
        </button>
      </div>
    </form>
  );
}
