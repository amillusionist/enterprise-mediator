'use client';

import React, { useState } from 'react';
import { useRouter } from 'next/navigation';
import type { PayoutDTO } from '@/lib/types';
import { formatCurrency, formatDate } from '@/lib/utils';
import { PayoutApprovalModal } from '@/components/features/finance/PayoutApprovalModal';

const STATUS_BADGE: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Processing: 'bg-blue-100 text-blue-800',
  Completed: 'bg-green-100 text-green-800',
  Failed: 'bg-red-100 text-red-800',
};

interface PayoutsTableProps {
  payouts: PayoutDTO[];
}

export function PayoutsTable({ payouts }: PayoutsTableProps) {
  const router = useRouter();
  const [selectedPayout, setSelectedPayout] = useState<PayoutDTO | null>(null);

  return (
    <>
      <div className="overflow-hidden shadow ring-1 ring-black ring-opacity-5 md:rounded-lg">
        <table className="min-w-full divide-y divide-gray-300">
          <thead className="bg-gray-50">
            <tr>
              <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-gray-900 sm:pl-6">Vendor</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Amount</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Status</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-gray-900">Created</th>
              <th scope="col" className="relative py-3.5 pl-3 pr-4 sm:pr-6">
                <span className="sr-only">Actions</span>
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200 bg-white">
            {payouts.map((payout) => (
              <tr key={payout.id}>
                <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-gray-900 sm:pl-6">{payout.vendorName}</td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-900 font-semibold">
                  {formatCurrency(payout.amount, payout.currency)}
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm">
                  <span className={`inline-flex rounded-full px-2 text-xs font-semibold leading-5 ${STATUS_BADGE[payout.status] || 'bg-gray-100 text-gray-800'}`}>
                    {payout.status}
                  </span>
                </td>
                <td className="whitespace-nowrap px-3 py-4 text-sm text-gray-500">{formatDate(payout.createdAt)}</td>
                <td className="relative whitespace-nowrap py-4 pl-3 pr-4 text-right text-sm font-medium sm:pr-6">
                  {payout.status === 'Pending' && (
                    <button
                      onClick={() => setSelectedPayout(payout)}
                      className="text-blue-600 hover:text-blue-900"
                    >
                      Review
                    </button>
                  )}
                </td>
              </tr>
            ))}
            {payouts.length === 0 && (
              <tr>
                <td colSpan={5} className="py-8 text-center text-sm text-gray-500">No pending payouts.</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {selectedPayout && (
        <PayoutApprovalModal
          payoutId={selectedPayout.id}
          vendorName={selectedPayout.vendorName}
          amount={selectedPayout.amount}
          currency={selectedPayout.currency}
          projectTitle={selectedPayout.projectId}
          isOpen={true}
          onClose={() => setSelectedPayout(null)}
          onActionComplete={() => {
            setSelectedPayout(null);
            router.refresh();
          }}
        />
      )}
    </>
  );
}
