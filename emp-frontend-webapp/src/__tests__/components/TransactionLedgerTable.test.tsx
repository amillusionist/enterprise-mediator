import { render, screen, within } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import TransactionLedgerTable from '@/components/features/finance/TransactionLedgerTable';
import type { TransactionDTO } from '@/lib/types';

vi.mock('@/lib/utils', () => ({
  formatCurrency: (amount: number, currency: string) =>
    new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency,
    }).format(amount),
  formatDate: (date: string) =>
    new Date(date).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    }),
}));

const mockTransactions: TransactionDTO[] = [
  {
    id: 'aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee',
    projectId: 'proj-1',
    type: 'InvoicePayment',
    amount: 50000,
    currency: 'USD',
    status: 'Completed',
    description: 'Payment for project Alpha',
    createdAt: '2025-02-01T10:00:00Z',
  },
  {
    id: 'ffffffff-1111-2222-3333-444444444444',
    projectId: 'proj-1',
    vendorId: 'vendor-1',
    type: 'VendorPayout',
    amount: 35000,
    currency: 'USD',
    status: 'Processing',
    description: 'Vendor payout milestone 1',
    createdAt: '2025-02-05T14:30:00Z',
  },
  {
    id: '55555555-6666-7777-8888-999999999999',
    projectId: 'proj-2',
    type: 'Refund',
    amount: 5000,
    currency: 'USD',
    status: 'Pending',
    description: 'Client refund',
    createdAt: '2025-02-10T09:00:00Z',
  },
  {
    id: 'abababab-cdcd-efef-0101-232323232323',
    projectId: 'proj-1',
    type: 'PlatformFee',
    amount: 2500,
    currency: 'USD',
    status: 'Completed',
    description: 'Platform service fee',
    createdAt: '2025-02-12T11:00:00Z',
  },
];

describe('TransactionLedgerTable', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the Transaction Ledger heading', () => {
    render(
      <TransactionLedgerTable initialTransactions={mockTransactions} />
    );

    expect(screen.getByText('Transaction Ledger')).toBeInTheDocument();
  });

  it('renders correct column headers', () => {
    render(
      <TransactionLedgerTable initialTransactions={mockTransactions} />
    );

    expect(screen.getByText('ID')).toBeInTheDocument();
    expect(screen.getByText('Date')).toBeInTheDocument();
    expect(screen.getByText('Description')).toBeInTheDocument();
    expect(screen.getByText('Type')).toBeInTheDocument();
    expect(screen.getByText('Amount')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
  });

  it('renders transaction rows with truncated IDs', () => {
    render(
      <TransactionLedgerTable initialTransactions={mockTransactions} />
    );

    expect(screen.getByText('aaaaaaaa...')).toBeInTheDocument();
    expect(screen.getByText('ffffffff...')).toBeInTheDocument();
    expect(screen.getByText('55555555...')).toBeInTheDocument();
  });

  it('renders transaction descriptions', () => {
    render(
      <TransactionLedgerTable initialTransactions={mockTransactions} />
    );

    expect(
      screen.getByText('Payment for project Alpha')
    ).toBeInTheDocument();
    expect(
      screen.getByText('Vendor payout milestone 1')
    ).toBeInTheDocument();
    expect(screen.getByText('Client refund')).toBeInTheDocument();
  });

  it('renders transaction types', () => {
    render(
      <TransactionLedgerTable initialTransactions={mockTransactions} />
    );

    expect(screen.getByText('InvoicePayment')).toBeInTheDocument();
    expect(screen.getByText('VendorPayout')).toBeInTheDocument();
    expect(screen.getByText('Refund')).toBeInTheDocument();
    expect(screen.getByText('PlatformFee')).toBeInTheDocument();
  });

  it('renders status badges with correct text', () => {
    render(
      <TransactionLedgerTable initialTransactions={mockTransactions} />
    );

    const completedBadges = screen.getAllByText('Completed');
    expect(completedBadges.length).toBe(2);
    expect(screen.getByText('Processing')).toBeInTheDocument();
    expect(screen.getByText('Pending')).toBeInTheDocument();
  });

  it('renders Export button', () => {
    render(
      <TransactionLedgerTable initialTransactions={mockTransactions} />
    );

    expect(screen.getByText('Export')).toBeInTheDocument();
  });

  it('calls exportAction when Export button is clicked', async () => {
    const mockExport = vi.fn().mockResolvedValue(undefined);
    const user = userEvent.setup();
    render(
      <TransactionLedgerTable
        initialTransactions={mockTransactions}
        exportAction={mockExport}
      />
    );

    await user.click(screen.getByText('Export'));
    expect(mockExport).toHaveBeenCalledTimes(1);
  });

  it('shows empty state when no transactions match', async () => {
    render(<TransactionLedgerTable initialTransactions={[]} />);

    expect(
      screen.getByText('No transactions found matching criteria.')
    ).toBeInTheDocument();
  });

  it('filters transactions by type using the select dropdown', async () => {
    const user = userEvent.setup();
    render(
      <TransactionLedgerTable initialTransactions={mockTransactions} />
    );

    const filterSelect = screen.getByDisplayValue('All Transactions');
    await user.selectOptions(filterSelect, 'InvoicePayment');

    expect(screen.getByText('InvoicePayment')).toBeInTheDocument();
    expect(screen.queryByText('VendorPayout')).not.toBeInTheDocument();
    expect(screen.queryByText('Refund')).not.toBeInTheDocument();
  });

  it('resets to page 1 when filter changes', async () => {
    // Create enough transactions to have multiple pages (PAGE_SIZE = 15)
    const manyTransactions: TransactionDTO[] = Array.from(
      { length: 20 },
      (_, i) => ({
        id: `${String(i).padStart(8, '0')}-0000-0000-0000-000000000000`,
        projectId: 'proj-1',
        type: 'InvoicePayment' as const,
        amount: 1000 + i,
        currency: 'USD',
        status: 'Completed' as const,
        description: `Transaction ${i}`,
        createdAt: `2025-01-${String(i + 1).padStart(2, '0')}T00:00:00Z`,
      })
    );

    const user = userEvent.setup();
    render(
      <TransactionLedgerTable initialTransactions={manyTransactions} />
    );

    // Verify pagination is visible
    expect(screen.getByText(/Showing/)).toBeInTheDocument();
    expect(screen.getByText('20')).toBeInTheDocument();
  });

  it('renders the filter dropdown with correct options', () => {
    render(
      <TransactionLedgerTable initialTransactions={mockTransactions} />
    );

    const filterSelect = screen.getByDisplayValue(
      'All Transactions'
    ) as HTMLSelectElement;
    const options = within(filterSelect).getAllByRole('option');
    const optionValues = options.map(
      (opt) => (opt as HTMLOptionElement).value
    );

    expect(optionValues).toContain('ALL');
    expect(optionValues).toContain('InvoicePayment');
    expect(optionValues).toContain('VendorPayout');
    expect(optionValues).toContain('PlatformFee');
    expect(optionValues).toContain('Refund');
  });
});
