import Link from 'next/link';

interface PaymentSuccessPageProps {
  searchParams: { invoiceId?: string };
}

export default function PaymentSuccessPage({ searchParams }: PaymentSuccessPageProps) {
  return (
    <div className="flex min-h-screen items-center justify-center bg-gray-50 px-4">
      <div className="w-full max-w-md rounded-lg bg-white p-8 text-center shadow-lg">
        {/* Success checkmark */}
        <div className="mx-auto mb-6 flex h-16 w-16 items-center justify-center rounded-full bg-green-100">
          <svg
            className="h-8 w-8 text-green-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
            aria-hidden="true"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M5 13l4 4L19 7"
            />
          </svg>
        </div>

        <h1 className="mb-2 text-2xl font-bold text-gray-900">Payment Successful</h1>
        <p className="mb-6 text-gray-600">
          Your payment has been processed successfully. Thank you!
        </p>

        {searchParams.invoiceId && (
          <p className="mb-6 text-sm text-gray-500">
            Invoice ID: <span className="font-mono font-medium">{searchParams.invoiceId}</span>
          </p>
        )}

        <Link
          href="/"
          className="inline-block rounded-md bg-blue-600 px-6 py-2 text-sm font-medium text-white hover:bg-blue-700"
        >
          Return to Home
        </Link>
      </div>
    </div>
  );
}
