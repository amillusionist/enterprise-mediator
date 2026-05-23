import React from 'react';
import { FinanceService } from '@/services/finance.service';
import type { PayoutDTO } from '@/lib/types';
import { PayoutsTable } from './payouts-table';

export const dynamic = 'force-dynamic';

export default async function PayoutsPage() {
  let payouts: PayoutDTO[] = [];
  let error: string | null = null;

  try {
    payouts = await FinanceService.getPendingPayouts();
  } catch {
    error = 'Failed to load payouts.';
  }

  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">Vendor Payouts</h1>
          <p className="mt-2 text-sm text-gray-700">
            Review and approve pending vendor payouts.
          </p>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded text-sm">{error}</div>
      )}

      <PayoutsTable payouts={payouts} />
    </div>
  );
}
