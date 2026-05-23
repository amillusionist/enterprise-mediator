'use client';

import React, { useState, useTransition } from 'react';
import { formatCurrency } from '@/lib/utils';
import { approvePayoutAction, rejectPayoutAction } from '@/actions/finance.actions';

interface PayoutApprovalModalProps {
  payoutId: string;
  vendorName: string;
  amount: number;
  currency: string;
  projectTitle: string;
  isOpen: boolean;
  onClose: () => void;
  onActionComplete: () => void;
}

export function PayoutApprovalModal({
  payoutId,
  vendorName,
  amount,
  currency,
  projectTitle,
  isOpen,
  onClose,
  onActionComplete
}: PayoutApprovalModalProps) {
  const [isPending, startTransition] = useTransition();
  const [rejectReason, setRejectReason] = useState('');
  const [mode, setMode] = useState<'VIEW' | 'REJECTING'>('VIEW');
  const [error, setError] = useState<string | null>(null);

  const handleApprove = () => {
    startTransition(async () => {
      try {
        const result = await approvePayoutAction(payoutId);
        if (result.success) {
          onActionComplete();
          onClose();
        } else {
          setError(result.message || 'Approval failed');
        }
      } catch {
        setError('System error during approval');
      }
    });
  };

  const handleReject = () => {
    if (!rejectReason.trim()) {
      setError('Rejection reason is required');
      return;
    }

    startTransition(async () => {
      try {
        const result = await rejectPayoutAction(payoutId, rejectReason);
        if (result.success) {
          onActionComplete();
          onClose();
        } else {
          setError(result.message || 'Rejection failed');
        }
      } catch {
        setError('System error during rejection');
      }
    });
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
      <div className="bg-white rounded-xl shadow-2xl max-w-md w-full overflow-hidden animate-in fade-in zoom-in duration-200">

        {/* Header */}
        <div className="px-6 py-4 bg-slate-50 border-b border-slate-100 flex justify-between items-center">
          <h3 className="text-lg font-semibold text-slate-900">
            {mode === 'REJECTING' ? 'Reject Payout' : 'Approve Payout'}
          </h3>
          <button onClick={onClose} className="text-slate-400 hover:text-slate-600">
            <svg className="h-6 w-6" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Content */}
        <div className="p-6">
          {error && (
            <div className="mb-4 p-3 bg-red-50 text-red-700 text-sm rounded border border-red-200 flex items-center">
              <svg className="h-5 w-5 mr-2 flex-shrink-0" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126zM12 15.75h.007v.008H12v-.008z" />
              </svg>
              {error}
            </div>
          )}

          {mode === 'VIEW' ? (
            <div className="space-y-4">
              <div className="bg-blue-50 p-4 rounded-lg text-center">
                <p className="text-sm text-blue-600 mb-1">Total Payout Amount</p>
                <p className="text-3xl font-bold text-blue-900">{formatCurrency(amount, currency)}</p>
              </div>

              <div className="text-sm text-slate-600 space-y-2">
                <div className="flex justify-between">
                  <span>Vendor:</span>
                  <span className="font-medium text-slate-900">{vendorName}</span>
                </div>
                <div className="flex justify-between">
                  <span>Project:</span>
                  <span className="font-medium text-slate-900">{projectTitle}</span>
                </div>
              </div>

              <p className="text-xs text-slate-500 mt-4">
                By approving, you authorize the release of escrowed funds to the vendor immediately. This action is logged.
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              <p className="text-sm text-slate-600">
                Please provide a reason for rejecting this payout. This will be sent to the vendor.
              </p>
              <textarea
                value={rejectReason}
                onChange={(e) => setRejectReason(e.target.value)}
                className="w-full h-32 rounded-md border-slate-300 shadow-sm focus:border-red-500 focus:ring-red-500 sm:text-sm p-3 border"
                placeholder="e.g., Deliverables incomplete, Invoice discrepancy..."
              />
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 bg-slate-50 border-t border-slate-100 flex justify-end gap-3">
          {mode === 'VIEW' ? (
            <>
              <button
                onClick={() => setMode('REJECTING')}
                disabled={isPending}
                className="px-4 py-2 text-sm font-medium text-red-600 hover:bg-red-50 rounded-md transition-colors"
              >
                Reject
              </button>
              <button
                onClick={handleApprove}
                disabled={isPending}
                className="px-4 py-2 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-md shadow-sm transition-colors flex items-center"
              >
                {isPending ? 'Processing...' : 'Confirm Approval'}
              </button>
            </>
          ) : (
            <>
              <button
                onClick={() => { setMode('VIEW'); setError(null); }}
                disabled={isPending}
                className="px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100 rounded-md"
              >
                Back
              </button>
              <button
                onClick={handleReject}
                disabled={isPending}
                className="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-md shadow-sm"
              >
                {isPending ? 'Rejecting...' : 'Confirm Rejection'}
              </button>
            </>
          )}
        </div>
      </div>
    </div>
  );
}
