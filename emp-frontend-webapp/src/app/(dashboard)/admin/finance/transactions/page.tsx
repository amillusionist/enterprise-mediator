import React from 'react';
import { FinanceService } from '@/services/finance.service';
import TransactionLedgerTable from '@/components/features/finance/TransactionLedgerTable';
import type { TransactionDTO } from '@/lib/types';

export const dynamic = 'force-dynamic';

export default async function TransactionsPage() {
  let transactions: TransactionDTO[] = [];
  let error: string | null = null;

  try {
    const result = await FinanceService.getTransactions({ pageSize: 100 });
    transactions = result.items;
  } catch {
    error = 'Failed to load transactions.';
  }

  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">Transaction Ledger</h1>
          <p className="mt-2 text-sm text-gray-700">
            Immutable record of all financial transactions across the platform.
          </p>
        </div>
      </div>

      {error && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded text-sm">{error}</div>
      )}

      <TransactionLedgerTable
        initialTransactions={transactions}
        exportAction={async () => {
          'use server';
          await FinanceService.exportTransactionsCsv();
        }}
      />
    </div>
  );
}
