'use client';

import React, { useTransition } from 'react';
import { useRouter } from 'next/navigation';
import { activateVendorAction, deactivateVendorAction } from '@/actions/vendor.actions';
import type { VendorStatus } from '@/lib/types';

interface VendorStatusActionsProps {
  vendorId: string;
  currentStatus: VendorStatus;
}

export function VendorStatusActions({ vendorId, currentStatus }: VendorStatusActionsProps) {
  const router = useRouter();
  const [isPending, startTransition] = useTransition();
  const [message, setMessage] = React.useState<{ type: 'success' | 'error'; text: string } | null>(null);

  const handleActivate = () => {
    setMessage(null);
    startTransition(async () => {
      const result = await activateVendorAction(vendorId);
      if (result.success) {
        setMessage({ type: 'success', text: result.message || 'Vendor activated.' });
        router.refresh();
      } else {
        setMessage({ type: 'error', text: result.message || 'Failed to activate vendor.' });
      }
    });
  };

  const handleDeactivate = () => {
    setMessage(null);
    startTransition(async () => {
      const result = await deactivateVendorAction(vendorId);
      if (result.success) {
        setMessage({ type: 'success', text: result.message || 'Vendor deactivated.' });
        router.refresh();
      } else {
        setMessage({ type: 'error', text: result.message || 'Failed to deactivate vendor.' });
      }
    });
  };

  return (
    <div className="space-y-4">
      {message && (
        <div
          role="alert"
          className={`px-4 py-3 rounded text-sm ${
            message.type === 'success'
              ? 'bg-green-50 border border-green-200 text-green-700'
              : 'bg-red-50 border border-red-200 text-red-700'
          }`}
        >
          {message.text}
        </div>
      )}

      <div className="flex flex-wrap gap-3">
        {(currentStatus === 'Pending' || currentStatus === 'Deactivated' || currentStatus === 'Suspended') && (
          <button
            type="button"
            onClick={handleActivate}
            disabled={isPending}
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
          >
            {isPending ? 'Processing...' : 'Activate Vendor'}
          </button>
        )}

        {currentStatus === 'Active' && (
          <button
            type="button"
            onClick={handleDeactivate}
            disabled={isPending}
            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-red-600 hover:bg-red-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-red-500 disabled:opacity-50 disabled:cursor-not-allowed transition-all"
          >
            {isPending ? 'Processing...' : 'Deactivate Vendor'}
          </button>
        )}
      </div>
    </div>
  );
}
