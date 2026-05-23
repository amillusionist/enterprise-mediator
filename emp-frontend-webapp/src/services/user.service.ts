import { ApiClient } from '@/services/api-client';
import type { User, PaginatedResult, PaginationParams, UserRole } from '@/lib/types';

/**
 * Service responsible for User Entity Management.
 * All methods are static -- call via `UserService.getUsers(...)`.
 */
export class UserService {
  private static readonly baseUrl = '/users';

  static async getUsers(filters?: PaginationParams): Promise<PaginatedResult<User>> {
    const queryParams = new URLSearchParams();

    if (filters) {
      if (filters.page) queryParams.append('page', filters.page.toString());
      if (filters.pageSize) queryParams.append('pageSize', filters.pageSize.toString());
    }

    const queryString = queryParams.toString();
    const endpoint = queryString ? `${this.baseUrl}?${queryString}` : this.baseUrl;

    return ApiClient.get<PaginatedResult<User>>(endpoint, ['users']);
  }

  static async getUserById(id: string): Promise<User> {
    return ApiClient.get<User>(`${this.baseUrl}/${id}`, [`user-${id}`]);
  }

  static async updateUser(id: string, data: { role: UserRole; isActive: boolean }): Promise<User> {
    return ApiClient.patch<User>(`${this.baseUrl}/${id}`, data);
  }

  static async deactivateUser(id: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/${id}/deactivate`, {});
  }

  static async activateUser(id: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/${id}/activate`, {});
  }
}

export const userService = UserService;
