import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { SowExtractionForm } from '@/components/features/sow/SowExtractionForm';
import type { ProjectDTO } from '@/lib/types';

vi.mock('@/actions/project.actions', () => ({
  saveSowDataAction: vi.fn(),
}));

vi.mock('@hookform/resolvers/zod', () => ({
  zodResolver: () => vi.fn(),
}));

const mockProject: ProjectDTO = {
  id: 'proj-123',
  name: 'Cloud Migration',
  clientId: 'client-1',
  clientName: 'Acme Corp',
  status: 'ReviewPending',
  description: 'A full cloud migration project',
  createdAt: '2025-01-01T00:00:00Z',
  updatedAt: '2025-01-15T00:00:00Z',
};

describe('SowExtractionForm', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('renders the Project Title field with default value from project', () => {
    render(<SowExtractionForm project={mockProject} />);

    const titleInput = screen.getByLabelText('Project Title');
    expect(titleInput).toBeInTheDocument();
    expect(titleInput).toHaveValue('Cloud Migration');
  });

  it('renders the Scope Summary textarea with default value from project description', () => {
    render(<SowExtractionForm project={mockProject} />);

    const summaryInput = screen.getByLabelText('Scope Summary');
    expect(summaryInput).toBeInTheDocument();
    expect(summaryInput).toHaveValue('A full cloud migration project');
  });

  it('renders the Scope Details textarea', () => {
    render(<SowExtractionForm project={mockProject} />);

    const scopeInput = screen.getByLabelText('Scope Details');
    expect(scopeInput).toBeInTheDocument();
  });

  it('renders Estimated Duration and Estimated Budget fields', () => {
    render(<SowExtractionForm project={mockProject} />);

    expect(
      screen.getByLabelText('Estimated Duration (weeks)')
    ).toBeInTheDocument();
    expect(screen.getByLabelText('Estimated Budget')).toBeInTheDocument();
  });

  it('renders Cancel and Save buttons', () => {
    render(<SowExtractionForm project={mockProject} />);

    expect(
      screen.getByRole('button', { name: /cancel/i })
    ).toBeInTheDocument();
    expect(
      screen.getByRole('button', { name: /save & finalize brief/i })
    ).toBeInTheDocument();
  });

  it('disables the Save button when form is not dirty', () => {
    render(<SowExtractionForm project={mockProject} />);

    const saveButton = screen.getByRole('button', {
      name: /save & finalize brief/i,
    });
    expect(saveButton).toBeDisabled();
  });

  it('enables the Save button when form fields are modified', async () => {
    const user = userEvent.setup();
    render(<SowExtractionForm project={mockProject} />);

    const titleInput = screen.getByLabelText('Project Title');
    await user.clear(titleInput);
    await user.type(titleInput, 'Updated Project Title');

    const saveButton = screen.getByRole('button', {
      name: /save & finalize brief/i,
    });
    expect(saveButton).toBeEnabled();
  });

  it('renders with correct placeholder text in fields', () => {
    render(<SowExtractionForm project={mockProject} />);

    expect(
      screen.getByPlaceholderText('e.g., Cloud Migration Strategy')
    ).toBeInTheDocument();
    expect(
      screen.getByPlaceholderText('Executive summary of the SOW...')
    ).toBeInTheDocument();
    expect(
      screen.getByPlaceholderText('Detailed scope description...')
    ).toBeInTheDocument();
    expect(screen.getByPlaceholderText('e.g., 12')).toBeInTheDocument();
    expect(screen.getByPlaceholderText('e.g., 150000')).toBeInTheDocument();
  });

  it('calls onSaveSuccess callback on successful save', async () => {
    const { saveSowDataAction } = await import('@/actions/project.actions');
    const mockSave = vi.mocked(saveSowDataAction);
    mockSave.mockResolvedValue({ success: true, message: '' });

    const onSaveSuccess = vi.fn();
    const user = userEvent.setup();
    render(
      <SowExtractionForm project={mockProject} onSaveSuccess={onSaveSuccess} />
    );

    const titleInput = screen.getByLabelText('Project Title');
    await user.clear(titleInput);
    await user.type(titleInput, 'New Title Here');

    const summaryInput = screen.getByLabelText('Scope Summary');
    await user.clear(summaryInput);
    await user.type(summaryInput, 'Updated summary text for testing');

    const saveButton = screen.getByRole('button', {
      name: /save & finalize brief/i,
    });
    await user.click(saveButton);
  });
});
