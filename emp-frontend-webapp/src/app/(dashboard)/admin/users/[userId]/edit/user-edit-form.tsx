'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { updateUserAction } from '@/actions/user.actions';
import type { User, UserRole } from '@/lib/types';

const ROLES: { value: UserRole; label: string }[] = [
  { value: 'SystemAdministrator', label: 'System Administrator' },
  { value: 'FinanceManager', label: 'Finance Manager' },
  { value: 'ClientContact', label: 'Client Contact' },
  { value: 'VendorContact', label: 'Vendor Contact' },
];

interface UserEditFormProps {
  user: User;
}

export function UserEditForm({ user }: UserEditFormProps) {
  const router = useRouter();
  const [role, setRole] = useState<UserRole>(user.role);
  const [isActive, setIsActive] = useState(user.isActive);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    setMessage(null);

    const result = await updateUserAction(user.id, { role, isActive });

    if (result.success) {
      setMessage({ type: 'success', text: result.message || 'User updated successfully.' });
      router.refresh();
    } else {
      setMessage({ type: 'error', text: result.message || 'Failed to update user.' });
    }

    setIsSubmitting(false);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div>
        <label htmlFor="email" className="block text-sm font-medium text-gray-700">
          Email
        </label>
        <input
          id="email"
          type="text"
          value={user.email}
          disabled
          className="mt-1 block w-full rounded-md border border-gray-300 bg-gray-50 px-3 py-2 text-sm text-gray-500"
        />
      </div>

      <div>
        <label htmlFor="role" className="block text-sm font-medium text-gray-700">
          Role
        </label>
        <select
          id="role"
          value={role}
          onChange={(e) => setRole(e.target.value as UserRole)}
          className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500"
        >
          {ROLES.map((r) => (
            <option key={r.value} value={r.value}>
              {r.label}
            </option>
          ))}
        </select>
      </div>

      <div className="flex items-center gap-3">
        <label htmlFor="isActive" className="text-sm font-medium text-gray-700">
          Active Status
        </label>
        <button
          id="isActive"
          type="button"
          role="switch"
          aria-checked={isActive}
          onClick={() => setIsActive(!isActive)}
          className={`relative inline-flex h-6 w-11 shrink-0 cursor-pointer rounded-full border-2 border-transparent transition-colors ${
            isActive ? 'bg-blue-600' : 'bg-gray-200'
          }`}
        >
          <span
            className={`pointer-events-none inline-block h-5 w-5 rounded-full bg-white shadow ring-0 transition-transform ${
              isActive ? 'translate-x-5' : 'translate-x-0'
            }`}
          />
        </button>
        <span className="text-sm text-gray-500">{isActive ? 'Active' : 'Inactive'}</span>
      </div>

      {message && (
        <p className={`text-sm ${message.type === 'success' ? 'text-green-600' : 'text-red-600'}`}>
          {message.text}
        </p>
      )}

      <div className="flex gap-3">
        <button
          type="submit"
          disabled={isSubmitting}
          className="rounded-md bg-blue-600 px-4 py-2 text-sm font-medium text-white hover:bg-blue-700 disabled:opacity-50"
        >
          {isSubmitting ? 'Saving...' : 'Save Changes'}
        </button>
        <button
          type="button"
          onClick={() => router.back()}
          className="rounded-md border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-50"
        >
          Cancel
        </button>
      </div>
    </form>
  );
}
