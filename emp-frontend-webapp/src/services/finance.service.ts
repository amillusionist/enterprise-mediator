import { ApiClient } from '@/services/api-client';
import type {
  TransactionDTO,
  InvoiceDTO,
  TransactionFilter,
  PaginatedResult,
  InitiatePayoutInput,
  GenerateInvoiceInput,
  TaxSettingsDTO,
  ProjectFinancialsDTO,
  PayoutDTO,
  DashboardMetrics,
  ProfitabilityReportItem,
  RetentionPolicy,
} from '@/lib/types';

/**
 * Service responsible for Financial Management.
 * All methods are static — call via `FinanceService.getTransactions(...)`.
 */
export class FinanceService {
  private static readonly baseUrl = '/finance';

  static async getTransactions(filters?: TransactionFilter): Promise<PaginatedResult<TransactionDTO>> {
    const queryParams = new URLSearchParams();

    if (filters) {
      if (filters.page) queryParams.append('page', filters.page.toString());
      if (filters.pageSize) queryParams.append('pageSize', filters.pageSize.toString());
      if (filters.startDate) queryParams.append('startDate', filters.startDate);
      if (filters.endDate) queryParams.append('endDate', filters.endDate);
      if (filters.type) queryParams.append('type', filters.type);
      if (filters.status) queryParams.append('status', filters.status);
      if (filters.projectId) queryParams.append('projectId', filters.projectId);
    }

    const queryString = queryParams.toString();
    const endpoint = queryString ? `${this.baseUrl}/transactions?${queryString}` : `${this.baseUrl}/transactions`;

    return ApiClient.get<PaginatedResult<TransactionDTO>>(endpoint, ['transactions']);
  }

  static async generateInvoice(projectId: string, data: GenerateInvoiceInput): Promise<InvoiceDTO> {
    return ApiClient.post<InvoiceDTO>(`/financials/projects/${projectId}/invoices/generate`, { ...data, projectId });
  }

  static async createInvoice(projectId: string, data: GenerateInvoiceInput): Promise<InvoiceDTO> {
    return this.generateInvoice(projectId, data);
  }

  static async getProjectFinancials(projectId: string): Promise<ProjectFinancialsDTO> {
    return ApiClient.get<ProjectFinancialsDTO>(`/financials/projects/${projectId}`, [`financials-${projectId}`]);
  }

  static async initiatePayout(data: InitiatePayoutInput): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/payouts/initiate`, data);
  }

  static async approvePayout(payoutId: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/payouts/${payoutId}/approve`, {});
  }

  static async rejectPayout(payoutId: string, reason: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/payouts/${payoutId}/reject`, { reason });
  }

  static async getPendingPayouts(): Promise<PayoutDTO[]> {
    return ApiClient.get<PayoutDTO[]>(`${this.baseUrl}/payouts?status=Pending`, ['pending-payouts']);
  }

  static async getTaxSettings(): Promise<TaxSettingsDTO> {
    return ApiClient.get<TaxSettingsDTO>(`${this.baseUrl}/config/tax`, ['tax-settings']);
  }

  static async updateTaxSettings(data: TaxSettingsDTO): Promise<TaxSettingsDTO> {
    return ApiClient.put<TaxSettingsDTO>(`${this.baseUrl}/config/tax`, data);
  }

  static async exportTransactionsCsv(filters?: TransactionFilter): Promise<Blob> {
    const queryParams = new URLSearchParams();
    if (filters) {
      if (filters.startDate) queryParams.append('startDate', filters.startDate);
      if (filters.endDate) queryParams.append('endDate', filters.endDate);
      if (filters.type) queryParams.append('type', filters.type);
    }
    const queryString = queryParams.toString();
    const endpoint = queryString ? `${this.baseUrl}/reports/transactions?${queryString}` : `${this.baseUrl}/reports/transactions`;

    return ApiClient.getBlob(endpoint);
  }

  static async getInvoiceByToken(token: string): Promise<InvoiceDTO> {
    return ApiClient.get<InvoiceDTO>(`/public/invoices/pay/${token}`, [], { skipAuth: true });
  }

  static async processInvoicePayment(invoiceId: string, paymentIntentId: string): Promise<void> {
    return ApiClient.post<void>(`/public/invoices/${invoiceId}/confirm-payment`, { paymentIntentId }, { skipAuth: true });
  }

  static async processRefund(projectId: string, amount: number, reason: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/refunds`, { projectId, amount, reason });
  }

  static async getDashboardMetrics(): Promise<DashboardMetrics> {
    return ApiClient.get<DashboardMetrics>('/dashboard/metrics', ['dashboard']);
  }

  static async getProfitabilityReport(): Promise<ProfitabilityReportItem[]> {
    return ApiClient.get<ProfitabilityReportItem[]>(`${this.baseUrl}/reports/profitability`, ['profitability']);
  }

  static async getRetentionPolicy(): Promise<RetentionPolicy> {
    return ApiClient.get<RetentionPolicy>(`${this.baseUrl}/config/retention`, ['retention']);
  }

  static async updateRetentionPolicy(data: RetentionPolicy): Promise<RetentionPolicy> {
    return ApiClient.put<RetentionPolicy>(`${this.baseUrl}/config/retention`, data);
  }
}

export const financeService = FinanceService;
