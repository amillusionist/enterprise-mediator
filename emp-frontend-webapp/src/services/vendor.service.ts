import { ApiClient } from '@/services/api-client';
import type {
  VendorDTO,
  VendorDetailDTO,
  CreateVendorInput,
  UpdateVendorInput,
  VendorFilter,
  PaginatedResult,
  VendorRecommendationDTO,
} from '@/lib/types';

/**
 * Service responsible for Vendor Entity Management.
 * All methods are static — call via `VendorService.getVendors(...)`.
 */
export class VendorService {
  private static readonly baseUrl = '/vendors';

  static async getVendors(filters?: VendorFilter): Promise<PaginatedResult<VendorDTO>> {
    const queryParams = new URLSearchParams();

    if (filters) {
      if (filters.page) queryParams.append('page', filters.page.toString());
      if (filters.pageSize) queryParams.append('pageSize', filters.pageSize.toString());
      if (filters.search) queryParams.append('search', filters.search);
      if (filters.status) queryParams.append('status', filters.status);
      if (filters.skills && filters.skills.length > 0) {
        queryParams.append('skills', filters.skills.join(','));
      }
    }

    const queryString = queryParams.toString();
    const endpoint = queryString ? `${this.baseUrl}?${queryString}` : this.baseUrl;

    return ApiClient.get<PaginatedResult<VendorDTO>>(endpoint, ['vendors']);
  }

  static async getVendorById(id: string): Promise<VendorDetailDTO> {
    return ApiClient.get<VendorDetailDTO>(`${this.baseUrl}/${id}`, [`vendor-${id}`]);
  }

  static async createVendor(data: CreateVendorInput): Promise<VendorDTO> {
    return ApiClient.post<VendorDTO>(this.baseUrl, data);
  }

  static async updateVendor(id: string, data: UpdateVendorInput): Promise<VendorDTO> {
    return ApiClient.patch<VendorDTO>(`${this.baseUrl}/${id}`, data);
  }

  static async activateVendor(id: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/${id}/activate`, {});
  }

  static async deactivateVendor(id: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/${id}/deactivate`, {});
  }

  static async updateStatus(id: string, status: string): Promise<void> {
    if (status === 'Active') {
      return this.activateVendor(id);
    }
    return this.deactivateVendor(id);
  }

  static async inviteContact(vendorId: string, email: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/${vendorId}/contacts/invite`, { email });
  }

  static async getRecommendations(projectId: string): Promise<VendorRecommendationDTO[]> {
    return ApiClient.get<VendorRecommendationDTO[]>(
      `/projects/${projectId}/briefs/matching-vendors?maxResults=25&minScore=0.75`,
      [`recommendations-${projectId}`]
    );
  }
}

export const vendorService = VendorService;
