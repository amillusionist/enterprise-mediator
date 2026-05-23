import { cookies } from 'next/headers';

/**
 * Configuration options for Next.js specific fetch behaviors
 */
export interface RequestConfig extends RequestInit {
  skipAuth?: boolean;
  params?: Record<string, string | number | boolean | undefined>;
}

/**
 * Custom Error class for API related failures
 */
export class ApiError extends Error {
  public status: number;
  public data: unknown;
  public errors?: Record<string, string[]>;

  constructor(message: string, status: number, data?: unknown, errors?: Record<string, string[]>) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.data = data;
    this.errors = errors;
  }
}

/**
 * Core API Client for Server-Side communication (Server Actions/RSC)
 * Implements the BFF (Backend for Frontend) pattern connector.
 *
 * All methods are static. Use as `ApiClient.get(...)` or via the `apiClient` alias.
 */
export class ApiClient {
  private static baseUrl = process.env.API_URL || process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api/v1';

  /**
   * Retrieves headers including Authentication token from HttpOnly cookies
   */
  private static async getHeaders(config?: RequestConfig, isFormData = false): Promise<Headers> {
    const headers = new Headers(config?.headers);

    if (!config?.skipAuth) {
      const cookieStore = cookies();
      const token = cookieStore.get('access_token')?.value;
      if (token) {
        headers.set('Authorization', `Bearer ${token}`);
      }
    }

    if (!isFormData && !headers.has('Content-Type')) {
      headers.set('Content-Type', 'application/json');
    }

    if (!headers.has('Accept')) {
      headers.set('Accept', 'application/json');
    }

    return headers;
  }

  /**
   * Constructs the full URL with query parameters
   */
  private static buildUrl(endpoint: string, params?: Record<string, string | number | boolean | undefined>): string {
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint.substring(1) : endpoint;
    const url = new URL(`${this.baseUrl}/${cleanEndpoint}`);

    if (params) {
      Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null) {
          url.searchParams.append(key, String(value));
        }
      });
    }

    return url.toString();
  }

  /**
   * Generic handler for Fetch responses
   */
  private static async handleResponse<T>(response: Response): Promise<T> {
    const contentType = response.headers.get('content-type');
    const isJson = contentType?.includes('application/json');

    let data: unknown;

    try {
      if (isJson) {
        data = await response.json();
      } else {
        data = await response.text();
      }
    } catch {
      data = null;
    }

    if (!response.ok) {
      const body = data as Record<string, unknown> | null;
      const errorMessage = (body?.message ?? body?.title ?? `API Error: ${response.statusText}`) as string;
      const validationErrors = body?.errors as Record<string, string[]> | undefined;
      throw new ApiError(errorMessage, response.status, data, validationErrors);
    }

    return data as T;
  }

  /**
   * GET Request
   * @param tags - Optional Next.js cache tags for revalidation
   */
  public static async get<T>(endpoint: string, tags?: string[], config?: RequestConfig): Promise<T> {
    const url = this.buildUrl(endpoint, config?.params);
    const headers = await this.getHeaders(config);

    const fetchOptions: RequestInit = {
      ...config,
      method: 'GET',
      headers,
    };

    if (tags && tags.length > 0) {
      (fetchOptions as Record<string, unknown>).next = { tags };
    }

    const response = await fetch(url, fetchOptions);
    return this.handleResponse<T>(response);
  }

  /**
   * POST Request (JSON body)
   */
  public static async post<T>(endpoint: string, body?: unknown, config?: RequestConfig): Promise<T> {
    const url = this.buildUrl(endpoint, config?.params);
    const isFormData = body instanceof FormData;
    const headers = await this.getHeaders(config, isFormData);
    const payload = isFormData ? body : JSON.stringify(body);

    const response = await fetch(url, {
      ...config,
      method: 'POST',
      headers,
      body: payload,
    });

    return this.handleResponse<T>(response);
  }

  /**
   * POST Request with FormData (multipart/form-data)
   * Content-Type is NOT set — the runtime sets the boundary automatically.
   */
  public static async postForm<T>(endpoint: string, formData: FormData, config?: RequestConfig): Promise<T> {
    const url = this.buildUrl(endpoint, config?.params);
    const headers = await this.getHeaders(config, true);

    const response = await fetch(url, {
      ...config,
      method: 'POST',
      headers,
      body: formData,
    });

    return this.handleResponse<T>(response);
  }

  /**
   * PUT Request
   */
  public static async put<T>(endpoint: string, body?: unknown, config?: RequestConfig): Promise<T> {
    const url = this.buildUrl(endpoint, config?.params);
    const isFormData = body instanceof FormData;
    const headers = await this.getHeaders(config, isFormData);
    const payload = isFormData ? body : JSON.stringify(body);

    const response = await fetch(url, {
      ...config,
      method: 'PUT',
      headers,
      body: payload,
    });

    return this.handleResponse<T>(response);
  }

  /**
   * PATCH Request
   */
  public static async patch<T>(endpoint: string, body?: unknown, config?: RequestConfig): Promise<T> {
    const url = this.buildUrl(endpoint, config?.params);
    const isFormData = body instanceof FormData;
    const headers = await this.getHeaders(config, isFormData);
    const payload = isFormData ? body : JSON.stringify(body);

    const response = await fetch(url, {
      ...config,
      method: 'PATCH',
      headers,
      body: payload,
    });

    return this.handleResponse<T>(response);
  }

  /**
   * DELETE Request
   */
  public static async delete<T>(endpoint: string, config?: RequestConfig): Promise<T> {
    const url = this.buildUrl(endpoint, config?.params);
    const headers = await this.getHeaders(config);

    const response = await fetch(url, {
      ...config,
      method: 'DELETE',
      headers,
    });

    return this.handleResponse<T>(response);
  }

  /**
   * GET Request returning a Blob (for CSV exports, file downloads)
   */
  public static async getBlob(endpoint: string, config?: RequestConfig): Promise<Blob> {
    const url = this.buildUrl(endpoint, config?.params);
    const headers = await this.getHeaders(config);

    const response = await fetch(url, {
      ...config,
      method: 'GET',
      headers,
    });

    if (!response.ok) {
      throw new ApiError(`Download failed: ${response.statusText}`, response.status);
    }

    return response.blob();
  }
}

/**
 * Convenience alias for static access — services can import either
 * `{ ApiClient }` and call `ApiClient.get(...)`, or
 * `{ apiClient }` and call `apiClient.get(...)`.
 */
export const apiClient = ApiClient;
