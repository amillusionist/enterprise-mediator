'use client';

import React, { useTransition, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { userInviteSchema, type UserInviteFormData } from '@/lib/schemas';
import { inviteUserAction } from '@/actions/auth.actions';

export default function InviteUserPage() {
  const [isPending, startTransition] = useTransition();
  const [result, setResult] = useState<{ success?: boolean; message?: string } | null>(null);

  const { register, handleSubmit, formState: { errors }, reset } = useForm<UserInviteFormData>({
    resolver: zodResolver(userInviteSchema),
  });

  const onSubmit = (data: UserInviteFormData) => {
    setResult(null);
    startTransition(async () => {
      const response = await inviteUserAction(data);
      setResult({ success: response.success, message: response.success ? 'Invitation sent successfully.' : response.error });
      if (response.success) reset();
    });
  };

  return (
    <div className="max-w-lg mx-auto space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-gray-900">Invite User</h1>
        <p className="mt-1 text-sm text-gray-500">
          Send a secure registration link to a new user.
        </p>
      </div>

      {result && (
        <div className={`px-4 py-3 rounded text-sm border ${
          result.success
            ? 'bg-green-50 border-green-200 text-green-700'
            : 'bg-red-50 border-red-200 text-red-700'
        }`}>
          {result.message}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="bg-white p-6 rounded-lg shadow border border-gray-200 space-y-4">
        <div>
          <label htmlFor="email" className="block text-sm font-medium text-gray-700">Email Address</label>
          <input
            id="email"
            type="email"
            {...register('email')}
            className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm px-3 py-2 border"
          />
          {errors.email && <p className="mt-1 text-xs text-red-600">{errors.email.message}</p>}
        </div>

        <div>
          <label htmlFor="role" className="block text-sm font-medium text-gray-700">Role</label>
          <select
            id="role"
            {...register('role')}
            className="mt-1 block w-full border-gray-300 rounded-md shadow-sm focus:ring-blue-500 focus:border-blue-500 sm:text-sm px-3 py-2 border"
          >
            <option value="">Select role...</option>
            <option value="SystemAdministrator">System Administrator</option>
            <option value="FinanceManager">Finance Manager</option>
            <option value="ClientContact">Client Contact</option>
            <option value="VendorContact">Vendor Contact</option>
          </select>
          {errors.role && <p className="mt-1 text-xs text-red-600">{errors.role.message}</p>}
        </div>

        <div className="pt-4 border-t border-gray-200 flex justify-end">
          <button
            type="submit"
            disabled={isPending}
            className="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
          >
            {isPending ? 'Sending...' : 'Send Invitation'}
          </button>
        </div>
      </form>
    </div>
  );
}
