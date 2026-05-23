import { ApiClient } from '@/services/api-client';
import type {
  AuditLogDTO,
  AuditLogFilter,
  PaginatedResult,
} from '@/lib/types';

/**
 * Service responsible for Observability and Compliance.
 * All methods are static — call via `AuditService.getAuditLogs(...)`.
 */
export class AuditService {
  private static readonly baseUrl = '/audit-logs';

  static async getAuditLogs(filters?: AuditLogFilter): Promise<AuditLogDTO[]> {
    const queryParams = new URLSearchParams();

    if (filters) {
      if (filters.page) queryParams.append('page', filters.page.toString());
      if (filters.pageSize) queryParams.append('pageSize', filters.pageSize.toString());
      if (filters.userId) queryParams.append('userId', filters.userId);
      if (filters.action) queryParams.append('action', filters.action);
      if (filters.entityId) queryParams.append('entityId', filters.entityId);
      if (filters.startDate) queryParams.append('startDate', filters.startDate);
      if (filters.endDate) queryParams.append('endDate', filters.endDate);
    }

    const queryString = queryParams.toString();
    const endpoint = queryString ? `${this.baseUrl}?${queryString}` : this.baseUrl;

    const result = await ApiClient.get<PaginatedResult<AuditLogDTO>>(endpoint, ['audit-logs']);
    return result.items;
  }

  static async getAuditLogById(id: string): Promise<AuditLogDTO> {
    return ApiClient.get<AuditLogDTO>(`${this.baseUrl}/${id}`, [`audit-log-${id}`]);
  }

  static async exportAuditLogs(filters?: AuditLogFilter): Promise<Blob> {
    const queryParams = new URLSearchParams();

    if (filters) {
      if (filters.startDate) queryParams.append('startDate', filters.startDate);
      if (filters.endDate) queryParams.append('endDate', filters.endDate);
      if (filters.action) queryParams.append('action', filters.action);
    }

    const queryString = queryParams.toString();
    const endpoint = queryString ? `${this.baseUrl}/export?${queryString}` : `${this.baseUrl}/export`;

    return ApiClient.getBlob(endpoint);
  }
}

export const auditService = AuditService;
