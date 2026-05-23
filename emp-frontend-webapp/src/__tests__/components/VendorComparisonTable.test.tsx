import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import VendorComparisonTable from '@/components/features/proposals/VendorComparisonTable';
import type { ProposalDTO } from '@/lib/types';

vi.mock('@/actions/project.actions', () => ({
  awardProjectAction: vi.fn(),
}));

vi.mock('@/lib/utils', () => ({
  formatCurrency: (amount: number, currency: string) =>
    `$${amount.toLocaleString('en-US', { minimumFractionDigits: 2 })} ${currency}`,
}));

const mockProposals: ProposalDTO[] = [
  {
    id: 'prop-1',
    projectId: 'proj-1',
    vendorId: 'vendor-1',
    vendorName: 'TechCorp Solutions',
    cost: 50000,
    currency: 'USD',
    timeline: '12 weeks',
    keyPersonnel: ['Alice Johnson', 'Bob Smith'],
    status: 'Submitted',
    submittedAt: '2025-01-10T00:00:00Z',
  },
  {
    id: 'prop-2',
    projectId: 'proj-1',
    vendorId: 'vendor-2',
    vendorName: 'DevHouse Inc',
    cost: 45000,
    currency: 'USD',
    timeline: '16 weeks',
    keyPersonnel: ['Carol White'],
    status: 'Shortlisted',
    submittedAt: '2025-01-12T00:00:00Z',
  },
  {
    id: 'prop-3',
    projectId: 'proj-1',
    vendorId: 'vendor-3',
    vendorName: 'CloudExperts',
    cost: 55000,
    currency: 'USD',
    timeline: '10 weeks',
    status: 'Submitted',
    submittedAt: '2025-01-15T00:00:00Z',
  },
];

describe('VendorComparisonTable', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.spyOn(window, 'confirm').mockReturnValue(false);
  });

  it('renders all vendor names as column headers', () => {
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    expect(screen.getByText('TechCorp Solutions')).toBeInTheDocument();
    expect(screen.getByText('DevHouse Inc')).toBeInTheDocument();
    expect(screen.getByText('CloudExperts')).toBeInTheDocument();
  });

  it('renders all comparison row labels', () => {
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    expect(screen.getByText('Total Cost')).toBeInTheDocument();
    expect(screen.getByText('Timeline')).toBeInTheDocument();
    expect(screen.getByText('Status')).toBeInTheDocument();
    expect(screen.getByText('Key Personnel')).toBeInTheDocument();
    expect(screen.getByText('Submitted')).toBeInTheDocument();
    expect(screen.getByText('Action')).toBeInTheDocument();
  });

  it('displays the Best Price badge on the lowest cost vendor', () => {
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    expect(screen.getByText('Best Price')).toBeInTheDocument();
  });

  it('displays the Shortlisted badge for shortlisted proposals', () => {
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    expect(screen.getByText('Shortlisted')).toBeInTheDocument();
  });

  it('renders timelines for each vendor', () => {
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    expect(screen.getByText('12 weeks')).toBeInTheDocument();
    expect(screen.getByText('16 weeks')).toBeInTheDocument();
    expect(screen.getByText('10 weeks')).toBeInTheDocument();
  });

  it('renders key personnel or Not specified placeholder', () => {
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    expect(
      screen.getByText('Alice Johnson, Bob Smith')
    ).toBeInTheDocument();
    expect(screen.getByText('Carol White')).toBeInTheDocument();
    expect(screen.getByText('Not specified')).toBeInTheDocument();
  });

  it('renders Select Winner buttons for each vendor', () => {
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    const selectButtons = screen.getAllByText('Select Winner');
    expect(selectButtons).toHaveLength(3);
  });

  it('prompts confirm dialog when Select Winner is clicked', async () => {
    const user = userEvent.setup();
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    const selectButtons = screen.getAllByText('Select Winner');
    await user.click(selectButtons[0]);

    expect(window.confirm).toHaveBeenCalledWith(
      'Are you sure you want to award this project to this vendor?'
    );
  });

  it('calls awardProjectAction when user confirms selection', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    const { awardProjectAction } = await import('@/actions/project.actions');
    const mockAward = vi.mocked(awardProjectAction);
    mockAward.mockResolvedValue({});

    const user = userEvent.setup();
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    const selectButtons = screen.getAllByText('Select Winner');
    await user.click(selectButtons[0]);

    expect(mockAward).toHaveBeenCalledWith('proj-1', 'vendor-1');
  });

  it('shows success message after successful award', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    const { awardProjectAction } = await import('@/actions/project.actions');
    const mockAward = vi.mocked(awardProjectAction);
    mockAward.mockResolvedValue({});

    const user = userEvent.setup();
    render(
      <VendorComparisonTable proposals={mockProposals} projectId="proj-1" />
    );

    const selectButtons = screen.getAllByText('Select Winner');
    await user.click(selectButtons[0]);

    expect(
      await screen.findByText(
        'Project successfully awarded! The vendor has been notified.'
      )
    ).toBeInTheDocument();
  });
});
