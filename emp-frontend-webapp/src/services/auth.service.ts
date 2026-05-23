import { ApiClient } from '@/services/api-client';
import type {
  AuthResponse,
  LoginCredentials,
  MfaSetupResponse,
  User,
  RegisterRequest,
  PasswordResetRequest,
  PasswordResetConfirmation,
  PaginatedResult,
} from '@/lib/types';

/**
 * Service responsible for handling authentication and authorization workflows.
 * All methods are static — call via `AuthService.login(...)`.
 */
export class AuthService {
  private static readonly baseUrl = '/auth';

  static async login(credentials: LoginCredentials): Promise<AuthResponse> {
    return ApiClient.post<AuthResponse>(`${this.baseUrl}/login`, credentials);
  }

  static async logout(): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/logout`, {});
  }

  static async verifyMfa(sessionId: string, code: string): Promise<AuthResponse> {
    return ApiClient.post<AuthResponse>(`${this.baseUrl}/mfa/verify`, { sessionId, code });
  }

  static async setupMfa(): Promise<MfaSetupResponse> {
    return ApiClient.post<MfaSetupResponse>(`${this.baseUrl}/mfa/setup`, {});
  }

  static async enableMfa(code: string): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/mfa/enable`, { code });
  }

  static async refreshToken(refreshToken?: string): Promise<AuthResponse> {
    return ApiClient.post<AuthResponse>(`${this.baseUrl}/refresh`, refreshToken ? { refreshToken } : {});
  }

  static async requestPasswordReset(data: PasswordResetRequest): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/password/forgot`, data);
  }

  static async confirmPasswordReset(data: PasswordResetConfirmation): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/password/reset`, data);
  }

  static async register(data: RegisterRequest): Promise<void> {
    return ApiClient.post<void>(`${this.baseUrl}/register`, data);
  }

  static async getCurrentUser(): Promise<User> {
    const me = await ApiClient.get<{
      id: string;
      email?: string;
      name?: string;
      roles: string[];
      isAuthenticated: boolean;
    }>('/users/me', ['user-profile']);

    const primaryRole = (me.roles[0] ?? 'SystemAdministrator') as User['role'];

    return {
      id: me.id,
      email: me.email ?? '',
      name: me.name ?? '',
      role: primaryRole,
      isActive: true,
      mfaEnabled: false,
    };
  }

  static async getAllUsers(): Promise<User[]> {
    const result = await ApiClient.get<PaginatedResult<User>>('/users', ['users']);
    return result.items;
  }

  static async inviteUser(data: { email: string; role: string; clientId?: string; vendorId?: string }): Promise<void> {
    return ApiClient.post<void>('/users/invite', data);
  }
}
