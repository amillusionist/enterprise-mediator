import { ApiClient } from '@/services/api-client';
import type {
  ProjectDTO,
  ProjectBriefDTO,
  CreateProjectRequest,
  ProjectFilter,
  PaginatedResult,
  SowData,
  SowExtractionData,
  UpdateProjectBriefRequest,
  MilestoneDTO,
  VendorRecommendationDTO,
} from '@/lib/types';

/**
 * Service responsible for Project Lifecycle Management.
 * All methods are static — call via `ProjectService.getProjects(...)`.
 */
export class ProjectService {
  private static readonly baseUrl = '/projects';

  static async getProjects(filters?: ProjectFilter): Promise<PaginatedResult<ProjectDTO>> {
    const queryParams = new URLSearchParams();

    if (filters) {
      if (filters.page) queryParams.append('page', filters.page.toString());
      if (filters.pageSize) queryParams.append('pageSize', filters.pageSize.toString());
      if (filters.status) queryParams.append('status', filters.status);
      if (filters.clientId) queryParams.append('clientId', filters.clientId);
      if (filters.search) queryParams.append('search', filters.search);
    }

    const queryString = queryParams.toString();
    const endpoint = queryString ? `${this.baseUrl}?${queryString}` : this.baseUrl;

    return ApiClient.get<PaginatedResult<ProjectDTO>>(endpoint, ['projects']);
  }

  static async getProjectById(id: string): Promise<ProjectDTO> {
    return ApiClient.get<ProjectDTO>(`${this.baseUrl}/${id}`, [`project-${id}`]);
  }

  static async createProject(data: CreateProjectRequest): Promise<ProjectDTO> {
    return ApiClient.post<ProjectDTO>(this.baseUrl, data);
  }

  static async updateProjectStatus(id: string, status: string): Promise<void> {
    return ApiClient.patch<void>(`${this.baseUrl}/${id}/status`, { status });
  }

  static async uploadSow(projectId: string, formData: FormData): Promise<void> {
    return ApiClient.postForm<void>(`${this.baseUrl}/${projectId}/sow`, formData);
  }

  static async getSowStatus(projectId: string): Promise<SowData> {
    return ApiClient.get<SowData>(`${this.baseUrl}/${projectId}/sow/status`, [`sow-status-${projectId}`]);
  }

  static async getExtractedSowData(projectId: string): Promise<SowExtractionData> {
    return ApiClient.get<SowExtractionData>(`${this.baseUrl}/${projectId}/sow/data`, [`sow-data-${projectId}`]);
  }

  static async getSowExtractionData(projectId: string): Promise<SowExtractionData> {
    return this.getExtractedSowData(projectId);
  }

  static async updateBrief(projectId: string, data: UpdateProjectBriefRequest): Promise<void> {
    return ApiClient.put<void>(`${this.baseUrl}/${projectId}/briefs`, data);
  }

  static async updateSowData(projectId: string, data: UpdateProjectBriefRequest): Promise<void> {
    return this.updateBrief(projectId, data);
  }

  static async approveBrief(projectId: string): Promise<void> {
    return ApiClient.put<void>(`${this.baseUrl}/${projectId}/briefs/approve`, {});
  }

  static async distributeBrief(projectId: string, vendorIds: string[]): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/${projectId}/distribute`, { vendorIds });
  }

  static async getMatchingVendors(projectId: string): Promise<VendorRecommendationDTO[]> {
    return ApiClient.get<VendorRecommendationDTO[]>(
      `${this.baseUrl}/${projectId}/briefs/matching-vendors?maxResults=25&minScore=0.75`,
      [`recommendations-${projectId}`]
    );
  }

  static async getMilestones(projectId: string): Promise<MilestoneDTO[]> {
    return ApiClient.get<MilestoneDTO[]>(`${this.baseUrl}/${projectId}/milestones`, [`milestones-${projectId}`]);
  }

  static async getMilestoneByToken(token: string): Promise<MilestoneDTO> {
    return ApiClient.get<MilestoneDTO>(`/public/milestones/${token}`, [], { skipAuth: true });
  }

  static async approveMilestone(token: string, decision: 'approved' | 'rejected', reason?: string): Promise<void> {
    return ApiClient.put<void>(`/public/milestones/${token}/decide`, { decision, reason }, { skipAuth: true });
  }

  static async getProjectBriefByToken(token: string): Promise<ProjectBriefDTO & { sowData: SowExtractionData }> {
    return ApiClient.get<ProjectBriefDTO & { sowData: SowExtractionData }>(`/public/proposals/portal/${token}`, [], { skipAuth: true });
  }

  static async awardProject(projectId: string, vendorId: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/${projectId}/award`, { vendorId });
  }

  static async getProjectBrief(projectId: string): Promise<ProjectBriefDTO> {
    return ApiClient.get<ProjectBriefDTO>(`${this.baseUrl}/${projectId}/briefs`, [`brief-${projectId}`]);
  }
}

export const projectService = ProjectService;
