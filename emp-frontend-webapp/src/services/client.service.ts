import { ApiClient } from '@/services/api-client';
import type {
  ClientDTO,
  CreateClientInput,
  ClientFilter,
  PaginatedResult,
} from '@/lib/types';

/**
 * Service responsible for Client Entity Management.
 * All methods are static — call via `ClientService.getClients(...)`.
 */
export class ClientService {
  private static readonly baseUrl = '/clients';

  static async getClients(filters?: ClientFilter): Promise<PaginatedResult<ClientDTO>> {
    const queryParams = new URLSearchParams();

    if (filters) {
      if (filters.page) queryParams.append('page', filters.page.toString());
      if (filters.pageSize) queryParams.append('pageSize', filters.pageSize.toString());
      if (filters.search) queryParams.append('search', filters.search);
      if (filters.status) queryParams.append('status', filters.status);
    }

    const queryString = queryParams.toString();
    const endpoint = queryString ? `${this.baseUrl}?${queryString}` : this.baseUrl;

    return ApiClient.get<PaginatedResult<ClientDTO>>(endpoint, ['clients']);
  }

  static async getClientById(id: string): Promise<ClientDTO> {
    return ApiClient.get<ClientDTO>(`${this.baseUrl}/${id}`, [`client-${id}`]);
  }

  static async createClient(data: CreateClientInput): Promise<ClientDTO> {
    return ApiClient.post<ClientDTO>(this.baseUrl, data);
  }

  static async updateClient(id: string, data: Partial<CreateClientInput>): Promise<ClientDTO> {
    return ApiClient.patch<ClientDTO>(`${this.baseUrl}/${id}`, data);
  }

  static async deactivateClient(id: string): Promise<void> {
    return ApiClient.put<void>(`${this.baseUrl}/${id}/deactivate`, {});
  }

  static async reactivateClient(id: string): Promise<void> {
    return ApiClient.put<void>(`${this.baseUrl}/${id}/reactivate`, {});
  }
}

export const clientService = ClientService;
