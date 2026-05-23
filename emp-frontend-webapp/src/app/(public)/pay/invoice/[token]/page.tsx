import React from 'react';
import { notFound } from 'next/navigation';
import { FinanceService } from '@/services/finance.service';
import { InvoicePaymentForm } from '@/components/features/public/InvoicePaymentForm';
import { formatCurrency, formatDate } from '@/lib/utils';

interface InvoicePaymentPageProps {
  params: {
    token: string;
  };
}

export default async function InvoicePaymentPage({ params }: InvoicePaymentPageProps) {
  const { token } = params;

  if (!token) notFound();

  try {
    const invoiceDetails = await FinanceService.getInvoiceByToken(token);

    if (!invoiceDetails) {
      return (
        <div className="max-w-md mx-auto bg-white p-8 rounded-lg shadow-md text-center">
          <h2 className="text-xl font-semibold text-red-600 mb-2">Invalid or Expired Link</h2>
          <p className="text-gray-600">
            This invoice payment link is invalid or has already been processed. Please contact your administrator for a new link.
          </p>
        </div>
      );
    }

    if (invoiceDetails.status === 'Paid') {
      return (
        <div className="max-w-md mx-auto bg-white p-8 rounded-lg shadow-md text-center">
          <div className="mx-auto flex items-center justify-center h-12 w-12 rounded-full bg-green-100 mb-4">
            <svg className="h-6 w-6 text-green-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M5 13l4 4L19 7" />
            </svg>
          </div>
          <h2 className="text-xl font-semibold text-gray-900 mb-2">Invoice Already Paid</h2>
          <p className="text-gray-600">
            Thank you! This invoice was paid on {formatDate(invoiceDetails.paidAt || Date.now())}.
          </p>
        </div>
      );
    }

    if (!invoiceDetails.clientSecret) {
      return (
        <div className="max-w-md mx-auto bg-white p-8 rounded-lg shadow-md text-center">
          <h2 className="text-xl font-semibold text-gray-900 mb-2">Payment Not Ready</h2>
          <p className="text-gray-600">
            The payment session is being prepared. Please try again in a moment.
          </p>
        </div>
      );
    }

    return (
      <div className="max-w-2xl mx-auto">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Secure Payment</h1>
          <p className="mt-2 text-gray-600">Complete payment for Invoice #{invoiceDetails.invoiceNumber}</p>
        </div>

        <InvoicePaymentForm
          invoiceId={invoiceDetails.id}
          clientSecret={invoiceDetails.clientSecret}
          amount={invoiceDetails.amount}
          currency={invoiceDetails.currency}
        />
      </div>
    );
  } catch (error) {
    console.error('Error fetching invoice:', error);
    return (
      <div className="max-w-md mx-auto bg-white p-8 rounded-lg shadow-md text-center">
        <h2 className="text-xl font-semibold text-gray-900 mb-2">System Error</h2>
        <p className="text-gray-600">
          We encountered a problem retrieving the invoice details. Please try again later.
        </p>
      </div>
    );
  }
}
