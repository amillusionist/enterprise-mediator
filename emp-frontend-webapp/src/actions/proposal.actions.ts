'use server';

import { ProposalService } from '@/services/proposal.service';

type ProposalActionState = {
  success: boolean;
  message?: string;
};

/**
 * Submit a vendor proposal against a project brief (public, no auth required).
 */
export async function submitProposalAction(
  token: string,
  formData: FormData
): Promise<ProposalActionState> {
  try {
    const cost = formData.get('cost');
    const timeline = formData.get('timeline');

    if (!cost || !timeline) {
      return { success: false, message: 'Cost and timeline are required.' };
    }

    const costNum = parseFloat(cost.toString());
    if (isNaN(costNum) || costNum <= 0) {
      return { success: false, message: 'Cost must be a positive number.' };
    }

    await ProposalService.submitProposal(token, formData);

    return { success: true, message: 'Proposal submitted successfully.' };
  } catch (err) {
    const message = err instanceof Error ? err.message : 'Failed to submit proposal.';
    return { success: false, message };
  }
}
