import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { PayoutApprovalModal } from '@/components/features/finance/PayoutApprovalModal';

const mockApprovePayout = vi.fn();
const mockRejectPayout = vi.fn();

vi.mock('@/actions/finance.actions', () => ({
  approvePayout: (...args: unknown[]) => mockApprovePayout(...args),
  rejectPayout: (...args: unknown[]) => mockRejectPayout(...args),
}));

vi.mock('@/lib/utils', () => ({
  formatCurrency: (amount: number, currency: string) =>
    new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency,
    }).format(amount),
}));

const defaultProps = {
  payoutId: 'payout-123',
  vendorName: 'TechCorp Solutions',
  amount: 25000,
  currency: 'USD',
  projectTitle: 'Cloud Migration Project',
  isOpen: true,
  onClose: vi.fn(),
  onActionComplete: vi.fn(),
};

describe('PayoutApprovalModal', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    mockApprovePayout.mockResolvedValue({ success: true });
    mockRejectPayout.mockResolvedValue({ success: true });
  });

  it('renders nothing when isOpen is false', () => {
    const { container } = render(
      <PayoutApprovalModal {...defaultProps} isOpen={false} />
    );

    expect(container.innerHTML).toBe('');
  });

  it('renders the modal when isOpen is true', () => {
    render(<PayoutApprovalModal {...defaultProps} />);

    expect(screen.getByText('Approve Payout')).toBeInTheDocument();
  });

  it('displays the payout amount formatted correctly', () => {
    render(<PayoutApprovalModal {...defaultProps} />);

    expect(screen.getByText('$25,000.00')).toBeInTheDocument();
  });

  it('displays the Total Payout Amount label', () => {
    render(<PayoutApprovalModal {...defaultProps} />);

    expect(screen.getByText('Total Payout Amount')).toBeInTheDocument();
  });

  it('displays the vendor name', () => {
    render(<PayoutApprovalModal {...defaultProps} />);

    expect(screen.getByText('TechCorp Solutions')).toBeInTheDocument();
  });

  it('displays the project title', () => {
    render(<PayoutApprovalModal {...defaultProps} />);

    expect(
      screen.getByText('Cloud Migration Project')
    ).toBeInTheDocument();
  });

  it('displays payout detail labels', () => {
    render(<PayoutApprovalModal {...defaultProps} />);

    expect(screen.getByText('Vendor:')).toBeInTheDocument();
    expect(screen.getByText('Project:')).toBeInTheDocument();
  });

  it('shows authorization warning text', () => {
    render(<PayoutApprovalModal {...defaultProps} />);

    expect(
      screen.getByText(/By approving, you authorize the release/)
    ).toBeInTheDocument();
  });

  it('renders Confirm Approval and Reject buttons in VIEW mode', () => {
    render(<PayoutApprovalModal {...defaultProps} />);

    expect(
      screen.getByRole('button', { name: 'Confirm Approval' })
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Reject' })
    ).toBeInTheDocument();
  });

  it('calls onClose when the close button (X) is clicked', async () => {
    const user = userEvent.setup();
    render(<PayoutApprovalModal {...defaultProps} />);

    // The close button is the SVG button in the header
    const closeButtons = screen
      .getByText('Approve Payout')
      .closest('div')!
      .querySelectorAll('button');
    const closeButton = closeButtons[closeButtons.length - 1];
    await user.click(closeButton);

    expect(defaultProps.onClose).toHaveBeenCalledTimes(1);
  });

  it('calls approvePayout and callbacks on approval confirmation', async () => {
    const user = userEvent.setup();
    render(<PayoutApprovalModal {...defaultProps} />);

    await user.click(
      screen.getByRole('button', { name: 'Confirm Approval' })
    );

    expect(mockApprovePayout).toHaveBeenCalledWith('payout-123');
  });

  it('transitions to REJECTING mode when Reject is clicked', async () => {
    const user = userEvent.setup();
    render(<PayoutApprovalModal {...defaultProps} />);

    await user.click(screen.getByRole('button', { name: 'Reject' }));

    expect(screen.getByText('Reject Payout')).toBeInTheDocument();
    expect(
      screen.getByText(/Please provide a reason for rejecting/)
    ).toBeInTheDocument();
    expect(
      screen.getByPlaceholderText(
        /Deliverables incomplete, Invoice discrepancy/
      )
    ).toBeInTheDocument();
  });

  it('renders Back and Confirm Rejection buttons in REJECTING mode', async () => {
    const user = userEvent.setup();
    render(<PayoutApprovalModal {...defaultProps} />);

    await user.click(screen.getByRole('button', { name: 'Reject' }));

    expect(
      screen.getByRole('button', { name: 'Back' })
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: 'Confirm Rejection' })
    ).toBeInTheDocument();
  });

  it('shows error when rejecting without a reason', async () => {
    const user = userEvent.setup();
    render(<PayoutApprovalModal {...defaultProps} />);

    await user.click(screen.getByRole('button', { name: 'Reject' }));
    await user.click(
      screen.getByRole('button', { name: 'Confirm Rejection' })
    );

    expect(
      screen.getByText('Rejection reason is required')
    ).toBeInTheDocument();
    expect(mockRejectPayout).not.toHaveBeenCalled();
  });

  it('calls rejectPayout with reason when rejection is confirmed', async () => {
    const user = userEvent.setup();
    render(<PayoutApprovalModal {...defaultProps} />);

    await user.click(screen.getByRole('button', { name: 'Reject' }));

    const textarea = screen.getByPlaceholderText(
      /Deliverables incomplete/
    );
    await user.type(textarea, 'Invoice does not match deliverables');

    await user.click(
      screen.getByRole('button', { name: 'Confirm Rejection' })
    );

    expect(mockRejectPayout).toHaveBeenCalledWith(
      'payout-123',
      'Invoice does not match deliverables'
    );
  });

  it('returns to VIEW mode when Back button is clicked from REJECTING mode', async () => {
    const user = userEvent.setup();
    render(<PayoutApprovalModal {...defaultProps} />);

    await user.click(screen.getByRole('button', { name: 'Reject' }));
    expect(screen.getByText('Reject Payout')).toBeInTheDocument();

    await user.click(screen.getByRole('button', { name: 'Back' }));
    expect(screen.getByText('Approve Payout')).toBeInTheDocument();
  });

  it('displays error message when approval fails', async () => {
    mockApprovePayout.mockResolvedValue({
      success: false,
      error: 'Insufficient funds in escrow',
    });

    const user = userEvent.setup();
    render(<PayoutApprovalModal {...defaultProps} />);

    await user.click(
      screen.getByRole('button', { name: 'Confirm Approval' })
    );

    expect(
      await screen.findByText('Insufficient funds in escrow')
    ).toBeInTheDocument();
  });
});
