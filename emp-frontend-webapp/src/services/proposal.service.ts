import { ApiClient } from '@/services/api-client';
import type {
  ProposalDTO,
  PaginatedResult,
} from '@/lib/types';

/**
 * Service responsible for Proposal Management.
 * All methods are static — call via `ProposalService.getProposals(...)`.
 */
export class ProposalService {
  private static readonly baseUrl = '/proposals';

  static async getProjectProposals(projectId: string): Promise<ProposalDTO[]> {
    const result = await ApiClient.get<PaginatedResult<ProposalDTO>>(
      `/projects/${projectId}/proposals`,
      [`proposals-${projectId}`]
    );
    return result.items;
  }

  static async updateProposalStatus(proposalId: string, status: string, notes?: string): Promise<void> {
    return ApiClient.put<void>(`${this.baseUrl}/${proposalId}/status`, { status, notes });
  }

  static async awardProject(proposalId: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/${proposalId}/award`, {});
  }

  static async submitProposal(token: string, formData: FormData): Promise<void> {
    return ApiClient.postForm<void>(`/public/proposals/${token}/submit`, formData, { skipAuth: true });
  }

  static async getPortalBrief(token: string): Promise<{
    projectName: string;
    brief: { requiredSkills: string[]; technologies: string[]; deliverables: string[]; timeline: string; summary: string };
    sowData: { sanitizedContent: string };
  }> {
    return ApiClient.get(`/public/proposals/portal/${token}`, [], { skipAuth: true });
  }
}

export const proposalService = ProposalService;
