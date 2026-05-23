'use client';

import React, { useState, useTransition } from 'react';
import { useRouter } from 'next/navigation';
import { createVendorAction } from '@/actions/vendor.actions';

export function CreateVendorForm() {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [error, setError] = useState<string | null>(null);
  const [skills, setSkills] = useState<string[]>(['']);

  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    setError(null);

    const formData = new FormData(e.currentTarget);
    // Inject skills as JSON for the server action
    formData.set('skills', JSON.stringify(skills.filter(Boolean)));

    startTransition(async () => {
      try {
        const result = await createVendorAction(null, formData);
        if (result.success) {
          router.push('/admin/vendors');
        } else {
          setError(result.message || 'Failed to create vendor.');
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
        <input type="text" name="companyName" id="companyName" required className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2" />
      </div>

      <div>
        <label htmlFor="address" className="block text-sm font-medium text-gray-700">Address</label>
        <input type="text" name="address" id="address" className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2" />
      </div>

      <div>
        <label htmlFor="primaryContactName" className="block text-sm font-medium text-gray-700">Primary Contact Name</label>
        <input type="text" name="primaryContactName" id="primaryContactName" required className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2" />
      </div>

      <div>
        <label htmlFor="primaryContactEmail" className="block text-sm font-medium text-gray-700">Primary Contact Email</label>
        <input type="email" name="primaryContactEmail" id="primaryContactEmail" required className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2" />
      </div>

      <div>
        <label htmlFor="primaryContactPhone" className="block text-sm font-medium text-gray-700">Primary Contact Phone</label>
        <input type="tel" name="primaryContactPhone" id="primaryContactPhone" className="mt-1 block w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2" />
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-2">Skills</label>
        <div className="space-y-2">
          {skills.map((skill, i) => (
            <div key={i} className="flex gap-2">
              <input
                type="text"
                value={skill}
                onChange={(e) => {
                  const updated = [...skills];
                  updated[i] = e.target.value;
                  setSkills(updated);
                }}
                className="flex-1 rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
                placeholder={`Skill ${i + 1}`}
              />
              {skills.length > 1 && (
                <button
                  type="button"
                  onClick={() => setSkills(skills.filter((_, idx) => idx !== i))}
                  className="p-2 text-gray-400 hover:text-red-500 text-sm"
                  aria-label="Remove skill"
                >
                  Remove
                </button>
              )}
            </div>
          ))}
          <button
            type="button"
            onClick={() => setSkills([...skills, ''])}
            className="text-xs font-medium text-blue-600 hover:text-blue-800"
          >
            + Add Skill
          </button>
        </div>
      </div>

      <div className="flex justify-end">
        <button
          type="submit"
          disabled={isPending}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 disabled:opacity-50"
        >
          {isPending ? 'Creating...' : 'Create Vendor'}
        </button>
      </div>
    </form>
  );
}
