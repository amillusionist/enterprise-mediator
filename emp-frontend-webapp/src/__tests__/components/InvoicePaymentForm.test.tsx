import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';

// Mock Stripe before importing the component
const mockConfirmPayment = vi.fn();
const mockUseStripe = vi.fn(() => ({
  confirmPayment: mockConfirmPayment,
}));
const mockUseElements = vi.fn(() => ({}));

vi.mock('@stripe/stripe-js', () => ({
  loadStripe: vi.fn(() => Promise.resolve({})),
}));

vi.mock('@stripe/react-stripe-js', () => ({
  Elements: ({
    children,
  }: {
    children: React.ReactNode;
  }) => <div data-testid="stripe-elements">{children}</div>,
  PaymentElement: () => (
    <div data-testid="stripe-payment-element">Payment Element</div>
  ),
  useStripe: () => mockUseStripe(),
  useElements: () => mockUseElements(),
}));

vi.mock('@/actions/finance.actions', () => ({
  processInvoicePayment: vi.fn(),
}));

// Import after mocks are set up
import { InvoicePaymentForm } from '@/components/features/public/InvoicePaymentForm';

describe('InvoicePaymentForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockUseStripe.mockReturnValue({ confirmPayment: mockConfirmPayment });
    mockUseElements.mockReturnValue({});
  });

  it('renders the Stripe Elements wrapper', () => {
    render(
      <InvoicePaymentForm
        invoiceId="inv-1"
        clientSecret="cs_test_secret"
        amount={5000}
        currency="USD"
      />
    );

    expect(screen.getByTestId('stripe-elements')).toBeInTheDocument();
  });

  it('renders the PaymentElement inside the form', () => {
    render(
      <InvoicePaymentForm
        invoiceId="inv-1"
        clientSecret="cs_test_secret"
        amount={5000}
        currency="USD"
      />
    );

    expect(
      screen.getByTestId('stripe-payment-element')
    ).toBeInTheDocument();
  });

  it('displays the Secure Payment heading', () => {
    render(
      <InvoicePaymentForm
        invoiceId="inv-1"
        clientSecret="cs_test_secret"
        amount={5000}
        currency="USD"
      />
    );

    expect(screen.getByText('Secure Payment')).toBeInTheDocument();
  });

  it('displays the formatted invoice amount', () => {
    render(
      <InvoicePaymentForm
        invoiceId="inv-1"
        clientSecret="cs_test_secret"
        amount={5000}
        currency="USD"
      />
    );

    expect(screen.getByText('$5,000.00')).toBeInTheDocument();
  });

  it('displays the total due label', () => {
    render(
      <InvoicePaymentForm
        invoiceId="inv-1"
        clientSecret="cs_test_secret"
        amount={12500.5}
        currency="USD"
      />
    );

    expect(screen.getByText(/Total due:/)).toBeInTheDocument();
    expect(screen.getByText('$12,500.50')).toBeInTheDocument();
  });

  it('renders the Pay Now button', () => {
    render(
      <InvoicePaymentForm
        invoiceId="inv-1"
        clientSecret="cs_test_secret"
        amount={5000}
        currency="USD"
      />
    );

    expect(screen.getByText('Pay Now')).toBeInTheDocument();
  });

  it('shows the Stripe security footer text', () => {
    render(
      <InvoicePaymentForm
        invoiceId="inv-1"
        clientSecret="cs_test_secret"
        amount={5000}
        currency="USD"
      />
    );

    expect(
      screen.getByText('Payments processed securely by Stripe')
    ).toBeInTheDocument();
  });

  it('disables the Pay Now button when stripe is not loaded', () => {
    mockUseStripe.mockReturnValue(null);

    render(
      <InvoicePaymentForm
        invoiceId="inv-1"
        clientSecret="cs_test_secret"
        amount={5000}
        currency="USD"
      />
    );

    const payButton = screen.getByText('Pay Now').closest('button');
    expect(payButton).toBeDisabled();
  });

  it('renders correctly with different currencies', () => {
    render(
      <InvoicePaymentForm
        invoiceId="inv-2"
        clientSecret="cs_test_secret_2"
        amount={3000}
        currency="EUR"
      />
    );

    // Intl.NumberFormat with EUR currency
    expect(screen.getByText(/3,000/)).toBeInTheDocument();
  });
});
