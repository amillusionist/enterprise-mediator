'use client';

import { Suspense, useEffect, useState } from 'react';
import { useSearchParams, useRouter } from 'next/navigation';
import { processInvoicePayment } from '@/actions/finance.actions';

function PaymentConfirmContent() {
  const searchParams = useSearchParams();
  const router = useRouter();
  const [status, setStatus] = useState<'confirming' | 'error'>('confirming');
  const [errorMessage, setErrorMessage] = useState('');

  useEffect(() => {
    const paymentIntent = searchParams.get('payment_intent');
    const invoiceId = searchParams.get('invoice_id');

    if (!paymentIntent || !invoiceId) {
      setStatus('error');
      setErrorMessage('Missing payment information. Please try again from the invoice link.');
      return;
    }

    const confirmPayment = async () => {
      try {
        const result = await processInvoicePayment(invoiceId, paymentIntent);

        if (!result.success) {
          throw new Error(result.message || 'Payment confirmation failed. Please contact support.');
        }

        router.replace(`/pay/success?invoiceId=${invoiceId}`);
      } catch (err) {
        setStatus('error');
        setErrorMessage(err instanceof Error ? err.message : 'An unexpected error occurred.');
      }
    };

    confirmPayment();
  }, [searchParams, router]);

  if (status === 'error') {
    return (
      <div className="w-full max-w-md rounded-lg bg-white p-8 text-center shadow-lg">
        <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-red-100">
          <svg
            className="h-8 w-8 text-red-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </div>
        <h1 className="mb-2 text-2xl font-bold text-gray-900">Payment Error</h1>
        <p className="mb-6 text-gray-600">{errorMessage}</p>
        <button
          onClick={() => {
            setStatus('confirming');
            setErrorMessage('');
            window.location.reload();
          }}
          className="inline-block rounded-md bg-blue-600 px-6 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          Retry
        </button>
      </div>
    );
  }

  return (
    <div className="w-full max-w-md rounded-lg bg-white p-8 text-center shadow-lg">
      <div className="mx-auto mb-6 h-10 w-10 animate-spin rounded-full border-4 border-blue-200 border-t-blue-600" />
      <h1 className="mb-2 text-xl font-bold text-gray-900">Confirming Payment</h1>
      <p className="text-gray-600">Please wait while we confirm your payment...</p>
    </div>
  );
}

export default function PaymentConfirmPage() {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <Suspense
        fallback={
          <div className="w-full max-w-md rounded-lg bg-white p-8 text-center shadow-lg">
            <div className="mx-auto mb-6 h-10 w-10 animate-spin rounded-full border-4 border-blue-200 border-t-blue-600" />
            <h1 className="mb-2 text-xl font-bold text-gray-900">Loading...</h1>
          </div>
        }
      >
        <PaymentConfirmContent />
      </Suspense>
    </div>
  );
}
