/**
 * Core Type Definitions — Single Source of Truth
 * Aligned with emp-shared-contracts DTOs and enums.
 * dependency level: 0
 */

// ─── Enums (aligned with EnterpriseMediator.Contracts.Enums) ────────────────

export type ProjectStatus =
  | 'Pending'
  | 'SowProcessing'
  | 'ReviewPending'
  | 'Proposed'
  | 'Awarded'
  | 'Active'
  | 'OnHold'
  | 'Completed'
  | 'Cancelled';

export type SowStatus =
  | 'Pending'
  | 'Processing'
  | 'Processed'
  | 'Failed';

export type ProposalStatus =
  | 'Submitted'
  | 'InReview'
  | 'Shortlisted'
  | 'Accepted'
  | 'Rejected';

export type InvoiceStatus =
  | 'Draft'
  | 'Sent'
  | 'Paid'
  | 'Overdue'
  | 'Cancelled';

export type PayoutStatus =
  | 'Pending'
  | 'Processing'
  | 'Completed'
  | 'Failed';

export type MilestoneStatus =
  | 'Pending'
  | 'Approved'
  | 'Rejected'
  | 'Completed';

export type TransactionType =
  | 'InvoicePayment'
  | 'VendorPayout'
  | 'Refund'
  | 'PlatformFee'
  | 'Adjustment';

export type UserRole =
  | 'SystemAdministrator'
  | 'FinanceManager'
  | 'ClientContact'
  | 'VendorContact';

export type VendorStatus =
  | 'Pending'
  | 'Active'
  | 'Suspended'
  | 'Deactivated';

export type AuditActionType =
  | 'LOGIN'
  | 'LOGOUT'
  | 'CREATE'
  | 'UPDATE'
  | 'DELETE'
  | 'UPLOAD'
  | 'APPROVE'
  | 'REJECT'
  | 'PAYMENT'
  | 'CONFIGURATION';

export type AuditEntityType =
  | 'USER'
  | 'PROJECT'
  | 'CLIENT'
  | 'VENDOR'
  | 'PROPOSAL'
  | 'INVOICE'
  | 'PAYOUT'
  | 'SOW';

// ─── Generic API Types ──────────────────────────────────────────────────────

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message?: string;
  errors?: Record<string, string[]>;
  meta?: {
    timestamp: string;
    requestId: string;
  };
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PaginationParams {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: 'asc' | 'desc';
}

// ─── Auth & User Types ──────────────────────────────────────────────────────

export interface User {
  id: string;
  email: string;
  name: string;
  role: UserRole;
  isActive: boolean;
  mfaEnabled: boolean;
  avatarUrl?: string;
  lastLoginAt?: string;
}

export interface AuthTokens {
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
}

export interface AuthResponse {
  user: User;
  tokens: AuthTokens;
  requiresMfa?: boolean;
  mfaSessionId?: string;
}

export interface LoginCredentials {
  email: string;
  password: string;
}

export interface MfaVerificationRequest {
  sessionId: string;
  code: string;
}

export interface MfaSetupResponse {
  qrCodeUrl: string;
  secret: string;
  backupCodes: string[];
}

export interface PasswordResetRequest {
  email: string;
}

export interface PasswordResetConfirmation {
  token: string;
  newPassword: string;
}

export interface RegisterRequest {
  inviteToken: string;
  name: string;
  password: string;
}

export interface UserInviteRequest {
  email: string;
  role: UserRole;
  clientId?: string;
  vendorId?: string;
}

// ─── Project Types ──────────────────────────────────────────────────────────

export interface ProjectDTO {
  id: string;
  name: string;
  clientId: string;
  clientName: string;
  status: ProjectStatus;
  description?: string;
  createdAt: string;
  updatedAt: string;
  awardedVendorId?: string;
  awardedVendorName?: string;
  startDate?: string;
  endDate?: string;
  budget?: number;
  currency?: string;
}

export interface CreateProjectRequest {
  name: string;
  clientId: string;
  description?: string;
  startDate?: string;
  endDate?: string;
}

export interface ProjectFilter extends PaginationParams {
  search?: string;
  status?: string;
  clientId?: string;
}

export interface SowData {
  id: string;
  projectId: string;
  originalFileName: string;
  fileUrl: string;
  status: SowStatus;
  uploadedBy: string;
  uploadedAt: string;
  processedAt?: string;
  extractedData?: SowExtractionData;
  sanitizedContent?: string;
  errorDetails?: string;
}

export interface SowExtractionData {
  projectName?: string;
  scopeSummary: string;
  deliverables: string[];
  requiredSkills: string[];
  technologies: string[];
  estimatedDurationWeeks?: number;
  budgetEstimate?: number;
  complexity?: string;
  confidenceScore: number;
  sanitizedContent?: string;
}

export interface ProjectBriefDTO {
  id: string;
  projectId: string;
  title: string;
  summary?: string;
  requiredSkills: string[];
  technologies?: string[];
  scope?: string;
  deliverables?: string[];
  estimatedDurationWeeks?: number;
  estimatedBudget?: number;
  currency?: string;
  complexityLevel?: string;
  extractedAt?: string;
  isApproved: boolean;
}

export interface UpdateProjectBriefRequest {
  title?: string;
  summary?: string;
  requiredSkills?: string[];
  technologies?: string[];
  scope?: string;
  deliverables?: string[];
  estimatedDurationWeeks?: number;
  estimatedBudget?: number;
}

export interface VendorRecommendationDTO {
  vendorId: string;
  companyName: string;
  similarityScore: number;
  matchedSkills: string[];
  contactEmail: string;
}

// ─── Vendor Types ───────────────────────────────────────────────────────────

export interface VendorDTO {
  id: string;
  companyName: string;
  primaryContactEmail?: string;
  primaryContactPhone?: string;
  status: VendorStatus;
  skills: string[];
  country?: string;
  city?: string;
  createdAt: string;
}

export interface VendorDetailDTO extends VendorDTO {
  address?: string;
  primaryContactName?: string;
  paymentDetails?: {
    bankName: string;
    accountNumber: string;
    swiftCode: string;
  };
  projectsAwarded?: number;
  averageRating?: number;
  onTimeCompletionRate?: number;
}

export interface CreateVendorInput {
  companyName: string;
  address: string;
  primaryContactName: string;
  primaryContactEmail: string;
  primaryContactPhone: string;
  skills: string[];
  paymentDetails?: {
    bankName: string;
    accountNumber: string;
    swiftCode: string;
  };
}

export interface UpdateVendorInput {
  companyName?: string;
  address?: string;
  primaryContactName?: string;
  primaryContactEmail?: string;
  primaryContactPhone?: string;
  skills?: string[];
}

export interface VendorFilter extends PaginationParams {
  search?: string;
  status?: string;
  skills?: string[];
}

// ─── Client Types ───────────────────────────────────────────────────────────

export interface ClientDTO {
  id: string;
  companyName: string;
  status: 'Active' | 'Inactive';
  primaryContactName: string;
  primaryContactEmail: string;
  activeProjectsCount: number;
  address?: string;
  createdAt: string;
}

export interface CreateClientInput {
  companyName: string;
  address: string;
  contacts: {
    name: string;
    email: string;
    phone?: string;
  }[];
}

export interface ClientFilter extends PaginationParams {
  search?: string;
  status?: string;
}

// ─── Proposal Types ─────────────────────────────────────────────────────────

export interface ProposalDTO {
  id: string;
  projectId: string;
  vendorId: string;
  vendorName: string;
  cost: number;
  currency: string;
  timeline: string;
  keyPersonnel?: string[];
  status: ProposalStatus;
  submittedAt: string;
  statusChangedAt?: string;
  internalNotes?: string;
}

export interface ProposalSubmissionInput {
  cost: number;
  timeline: string;
  keyPersonnel: string;
  file?: File;
}

// ─── Financial Types ────────────────────────────────────────────────────────

export interface InvoiceDTO {
  id: string;
  invoiceNumber: string;
  projectId: string;
  clientId: string;
  amount: number;
  currency: string;
  status: InvoiceStatus;
  stripeSessionId?: string;
  clientSecret?: string;
  paymentLink?: string;
  createdAt: string;
  dueDate?: string;
  paidAt?: string;
}

export interface GenerateInvoiceInput {
  amount: number;
  currency: string;
  description?: string;
  dueDate?: string;
}

export interface TransactionDTO {
  id: string;
  projectId?: string;
  vendorId?: string;
  clientId?: string;
  type: TransactionType;
  amount: number;
  currency: string;
  status: 'Pending' | 'Completed' | 'Failed' | 'Processing';
  referenceId?: string;
  description?: string;
  createdAt: string;
  completedAt?: string;
}

export interface TransactionFilter extends PaginationParams {
  startDate?: string;
  endDate?: string;
  type?: string;
  status?: string;
  projectId?: string;
}

export interface PayoutDTO {
  id: string;
  projectId: string;
  vendorId: string;
  vendorName: string;
  amount: number;
  currency: string;
  status: PayoutStatus;
  milestoneId?: string;
  wiseTransferId?: string;
  createdAt: string;
  completedAt?: string;
  failureReason?: string;
}

export interface InitiatePayoutInput {
  projectId: string;
  milestoneId: string | null;
  amount: number;
}

export interface TaxSettingsDTO {
  taxRate: number;
  taxId?: string;
  enabled: boolean;
}

export interface ProjectFinancialsDTO {
  projectId: string;
  totalBudget: number;
  totalInvoiced: number;
  totalPaid: number;
  pendingPayouts: number;
  completedPayouts: number;
  netProfit: number;
  currency: string;
  hasOverdue: boolean;
}

// ─── Milestone Types ────────────────────────────────────────────────────────

export interface MilestoneDTO {
  id: string;
  projectId: string;
  title: string;
  description?: string;
  amount: number;
  currency: string;
  status: MilestoneStatus;
  dueDate?: string;
  approvedAt?: string;
  rejectionReason?: string;
}

// ─── Audit Types ────────────────────────────────────────────────────────────

export interface AuditLogDTO {
  id: string;
  timestamp: string;
  userId: string;
  userName?: string;
  userEmail?: string;
  actionType: AuditActionType;
  entityType: AuditEntityType;
  entityId: string;
  entityName?: string;
  changes?: Record<string, unknown>;
  metadata?: Record<string, unknown>;
  status: 'SUCCESS' | 'FAILURE';
}

export interface AuditLogFilter extends PaginationParams {
  userId?: string;
  action?: string;
  entityType?: string;
  entityId?: string;
  startDate?: string;
  endDate?: string;
}

// ─── Notification Types ─────────────────────────────────────────────────────

export interface ServerNotification {
  id: string;
  type: 'ProjectBriefReceived' | 'ProposalStatusChanged' | 'PaymentReceived' | 'PayoutSent' | 'MilestoneApprovalNeeded' | 'General';
  title: string;
  message: string;
  priority: 'LOW' | 'MEDIUM' | 'HIGH';
  relatedEntityId?: string;
  relatedEntityType?: string;
  isRead: boolean;
  createdAt: string;
}

// ─── Common Filter Types ────────────────────────────────────────────────────

export interface BaseFilter extends PaginationParams {
  search?: string;
  status?: string[];
  dateFrom?: string;
  dateTo?: string;
}

// ─── Dashboard Types ────────────────────────────────────────────────────────

export interface DashboardMetrics {
  activeProjectsCount: number;
  pendingSowCount: number;
  proposalsAwaitingCount: number;
  totalRevenue: number;
  totalPayouts: number;
  netProfit: number;
  projectsByStatus: Record<ProjectStatus, number>;
  upcomingMilestones: MilestoneDTO[];
  recentActivity: AuditLogDTO[];
}

// ─── Report Types ───────────────────────────────────────────────────────────

export interface ProfitabilityReportItem {
  projectId: string;
  projectName: string;
  clientName: string;
  invoicedAmount: number;
  vendorPayout: number;
  platformFee: number;
  netProfit: number;
  currency: string;
}

export interface RetentionPolicy {
  auditLogRetentionDays: number;
  financialRecordRetentionDays: number;
  projectDataRetentionDays: number;
}
