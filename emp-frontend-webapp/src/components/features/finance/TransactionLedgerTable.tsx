'use client';

import React, { useState, useMemo } from 'react';
import type { TransactionDTO, TransactionType } from '@/lib/types';
import { formatCurrency, formatDate } from '@/lib/utils';

interface TransactionLedgerTableProps {
  initialTransactions: TransactionDTO[];
  exportAction?: () => Promise<void>;
}

const PAGE_SIZE = 15;

const STATUS_STYLES: Record<string, string> = {
  Completed: 'bg-green-100 text-green-800',
  Pending: 'bg-yellow-100 text-yellow-800',
  Failed: 'bg-red-100 text-red-800',
  Processing: 'bg-blue-100 text-blue-800',
};

export default function TransactionLedgerTable({ initialTransactions, exportAction }: TransactionLedgerTableProps) {
  const [filterType, setFilterType] = useState<'ALL' | TransactionType>('ALL');
  const [currentPage, setCurrentPage] = useState(1);

  const filteredTransactions = useMemo(() => {
    if (filterType === 'ALL') return initialTransactions;
    return initialTransactions.filter((t) => t.type === filterType);
  }, [initialTransactions, filterType]);

  const totalPages = Math.max(1, Math.ceil(filteredTransactions.length / PAGE_SIZE));
  const paged = filteredTransactions.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);

  function handleFilterChange(value: string) {
    setFilterType(value as 'ALL' | TransactionType);
    setCurrentPage(1);
  }

  async function handleExport() {
    try {
      const baseUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api/v1';
      const response = await fetch(`${baseUrl}/finance/reports/transactions`, {
        credentials: 'include',
      });

      if (!response.ok) throw new Error('Export failed');

      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `transactions-${new Date().toISOString().split('T')[0]}.csv`;
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      a.remove();
    } catch {
      alert('Failed to export transactions. Please try again.');
    }
  }

  return (
    <div className="bg-white shadow rounded-lg border border-slate-200 overflow-hidden">
      {/* Toolbar */}
      <div className="px-4 py-3 border-b border-slate-200 flex flex-col sm:flex-row justify-between items-center gap-4 bg-slate-50">
        <h3 className="text-base font-semibold leading-6 text-slate-900">Transaction Ledger</h3>
        <div className="flex items-center gap-2">
          <div className="relative">
            <select
              value={filterType}
              onChange={(e) => handleFilterChange(e.target.value)}
              className="block w-full rounded-md border-0 py-1.5 pl-3 pr-10 text-slate-900 ring-1 ring-inset ring-slate-300 focus:ring-2 focus:ring-blue-600 sm:text-sm sm:leading-6 appearance-none bg-white"
            >
              <option value="ALL">All Transactions</option>
              <option value="InvoicePayment">Invoice Payments</option>
              <option value="VendorPayout">Vendor Payouts</option>
              <option value="PlatformFee">Platform Fees</option>
              <option value="Refund">Refunds</option>
            </select>
            <div className="pointer-events-none absolute inset-y-0 right-0 flex items-center px-2 text-slate-500">
              <svg className="h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 3c2.755 0 5.455.232 8.083.678.533.09.917.556.917 1.096v1.044a2.25 2.25 0 01-.659 1.591l-5.432 5.432a2.25 2.25 0 00-.659 1.591v2.927a2.25 2.25 0 01-1.244 2.013L9.75 21v-6.568a2.25 2.25 0 00-.659-1.591L3.659 7.409A2.25 2.25 0 013 5.818V4.774c0-.54.384-1.006.917-1.096A48.32 48.32 0 0112 3z" />
              </svg>
            </div>
          </div>
          <button
            onClick={handleExport}
            className="inline-flex items-center gap-x-1.5 rounded-md bg-white px-3 py-2 text-sm font-semibold text-slate-900 shadow-sm ring-1 ring-inset ring-slate-300 hover:bg-slate-50"
          >
            <svg className="-ml-0.5 h-5 w-5 text-slate-400" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M3 16.5v2.25A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75V16.5M16.5 12L12 16.5m0 0L7.5 12m4.5 4.5V3" />
            </svg>
            Export
          </button>
        </div>
      </div>

      {/* Table */}
      <div className="overflow-x-auto">
        <table className="min-w-full divide-y divide-slate-300">
          <thead className="bg-slate-50">
            <tr>
              <th scope="col" className="py-3.5 pl-4 pr-3 text-left text-sm font-semibold text-slate-900 sm:pl-6">ID</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-slate-900">Date</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-slate-900">Description</th>
              <th scope="col" className="px-3 py-3.5 text-left text-sm font-semibold text-slate-900">Type</th>
              <th scope="col" className="px-3 py-3.5 text-right text-sm font-semibold text-slate-900">Amount</th>
              <th scope="col" className="px-3 py-3.5 text-center text-sm font-semibold text-slate-900">Status</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-200 bg-white">
            {paged.length > 0 ? (
              paged.map((transaction) => (
                <tr key={transaction.id} className="hover:bg-slate-50">
                  <td className="whitespace-nowrap py-4 pl-4 pr-3 text-sm font-medium text-slate-900 sm:pl-6 font-mono">
                    {transaction.id.substring(0, 8)}...
                  </td>
                  <td className="whitespace-nowrap px-3 py-4 text-sm text-slate-500">
                    {formatDate(transaction.createdAt)}
                  </td>
                  <td className="px-3 py-4 text-sm text-slate-500 max-w-xs truncate">
                    {transaction.description || '-'}
                  </td>
                  <td className="whitespace-nowrap px-3 py-4 text-sm text-slate-500">
                    {transaction.type}
                  </td>
                  <td className={`whitespace-nowrap px-3 py-4 text-sm text-right font-medium ${transaction.type === 'Refund' ? 'text-red-600' : 'text-slate-900'}`}>
                    {formatCurrency(transaction.amount, transaction.currency)}
                  </td>
                  <td className="whitespace-nowrap px-3 py-4 text-sm text-center">
                    <span className={`inline-flex rounded-full px-2 text-xs font-semibold leading-5 ${STATUS_STYLES[transaction.status] || 'bg-gray-100 text-gray-800'}`}>
                      {transaction.status}
                    </span>
                  </td>
                </tr>
              ))
            ) : (
              <tr>
                <td colSpan={6} className="px-3 py-12 text-center text-sm text-slate-500">
                  No transactions found matching criteria.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex items-center justify-between border-t border-slate-200 bg-white px-4 py-3 sm:px-6">
          <div className="hidden sm:flex sm:flex-1 sm:items-center sm:justify-between">
            <div>
              <p className="text-sm text-slate-700">
                Showing <span className="font-medium">{(currentPage - 1) * PAGE_SIZE + 1}</span> to{' '}
                <span className="font-medium">{Math.min(currentPage * PAGE_SIZE, filteredTransactions.length)}</span> of{' '}
                <span className="font-medium">{filteredTransactions.length}</span> results
              </p>
            </div>
            <div>
              <nav className="isolate inline-flex -space-x-px rounded-md shadow-sm" aria-label="Pagination">
                <button
                  onClick={() => setCurrentPage((p) => Math.max(1, p - 1))}
                  disabled={currentPage === 1}
                  className="relative inline-flex items-center rounded-l-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50"
                >
                  <span className="sr-only">Previous</span>
                  <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M12.79 5.23a.75.75 0 01-.02 1.06L8.832 10l3.938 3.71a.75.75 0 11-1.04 1.08l-4.5-4.25a.75.75 0 010-1.08l4.5-4.25a.75.75 0 011.06.02z" clipRule="evenodd" />
                  </svg>
                </button>
                <button
                  onClick={() => setCurrentPage((p) => Math.min(totalPages, p + 1))}
                  disabled={currentPage >= totalPages}
                  className="relative inline-flex items-center rounded-r-md px-2 py-2 text-gray-400 ring-1 ring-inset ring-gray-300 hover:bg-gray-50 focus:z-20 focus:outline-offset-0 disabled:opacity-50"
                >
                  <span className="sr-only">Next</span>
                  <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
                    <path fillRule="evenodd" d="M7.21 14.77a.75.75 0 01.02-1.06L11.168 10 7.23 6.29a.75.75 0 111.04-1.08l4.5 4.25a.75.75 0 010 1.08l-4.5 4.25a.75.75 0 01-1.06-.02z" clipRule="evenodd" />
                  </svg>
                </button>
              </nav>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
