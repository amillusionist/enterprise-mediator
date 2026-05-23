'use server';

import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';
import { AuthService } from '@/services/auth.service';
import { LoginSchema, RegisterSchema } from '@/lib/schemas';
import type { User } from '@/lib/types';
import { AUTH_COOKIE_NAME, REFRESH_COOKIE_NAME, COOKIE_OPTIONS } from '@/lib/constants';

export type ActionState<T = null> = {
  success: boolean;
  data?: T;
  error?: string;
  fieldErrors?: Record<string, string[]>;
};

/**
 * Handles user login with validation and secure cookie management.
 */
export async function loginAction(
  prevState: ActionState<User> | null,
  formData: FormData
): Promise<ActionState<User>> {
  try {
    const rawData = Object.fromEntries(formData.entries());
    const validatedFields = LoginSchema.safeParse(rawData);

    if (!validatedFields.success) {
      return {
        success: false,
        fieldErrors: validatedFields.error.flatten().fieldErrors,
        error: 'Invalid credentials format.',
      };
    }

    const response = await AuthService.login(validatedFields.data);

    const cookieStore = cookies();

    cookieStore.set(AUTH_COOKIE_NAME, response.tokens.accessToken, {
      ...COOKIE_OPTIONS,
      expires: new Date(Date.now() + response.tokens.expiresIn * 1000),
    });

    cookieStore.set(REFRESH_COOKIE_NAME, response.tokens.refreshToken, {
      ...COOKIE_OPTIONS,
      expires: new Date(Date.now() + 7 * 24 * 60 * 60 * 1000),
    });

    return { success: true, data: response.user };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Authentication failed. Please try again.';
    console.error('Login Action Error:', error);
    return { success: false, error: message };
  }
}

/**
 * Handles user logout by clearing sessions and cookies.
 */
export async function logoutAction(): Promise<void> {
  const cookieStore = cookies();
  const accessToken = cookieStore.get(AUTH_COOKIE_NAME)?.value;

  if (accessToken) {
    try {
      await AuthService.logout();
    } catch (error) {
      console.warn('Logout service call failed, proceeding to clear cookies:', error);
    }
  }

  cookieStore.delete(AUTH_COOKIE_NAME);
  cookieStore.delete(REFRESH_COOKIE_NAME);
  redirect('/login');
}

/**
 * MFA Verification step.
 */
export async function verifyMfaAction(
  sessionId: string,
  code: string
): Promise<ActionState<User>> {
  try {
    if (!code || code.length !== 6) {
      return { success: false, error: 'Invalid verification code.' };
    }

    const response = await AuthService.verifyMfa(sessionId, code);

    const cookieStore = cookies();
    cookieStore.set(AUTH_COOKIE_NAME, response.tokens.accessToken, COOKIE_OPTIONS);
    cookieStore.set(REFRESH_COOKIE_NAME, response.tokens.refreshToken, COOKIE_OPTIONS);

    return { success: true, data: response.user };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'MFA verification failed.';
    return { success: false, error: message };
  }
}

/**
 * Registers a new user via invite token.
 */
export async function registerAction(
  prevState: ActionState<void> | null,
  formData: FormData
): Promise<ActionState<void>> {
  try {
    const rawData = Object.fromEntries(formData.entries());
    const validatedFields = RegisterSchema.safeParse(rawData);

    if (!validatedFields.success) {
      return {
        success: false,
        fieldErrors: validatedFields.error.flatten().fieldErrors,
        error: 'Registration validation failed.',
      };
    }

    await AuthService.register(validatedFields.data);
    return { success: true };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Registration failed.';
    return { success: false, error: message };
  }
}

/**
 * Refreshes tokens server-side.
 */
export async function refreshSessionAction(): Promise<boolean> {
  const cookieStore = cookies();
  const refreshToken = cookieStore.get(REFRESH_COOKIE_NAME)?.value;

  if (!refreshToken) return false;

  try {
    const response = await AuthService.refreshToken(refreshToken);
    cookieStore.set(AUTH_COOKIE_NAME, response.tokens.accessToken, COOKIE_OPTIONS);
    if (response.tokens.refreshToken) {
      cookieStore.set(REFRESH_COOKIE_NAME, response.tokens.refreshToken, COOKIE_OPTIONS);
    }
    return true;
  } catch {
    return false;
  }
}

/**
 * Requests a password reset email.
 */
export async function requestPasswordResetAction(email: string): Promise<ActionState<void>> {
  try {
    await AuthService.requestPasswordReset({ email });
    return { success: true };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to send reset email.';
    return { success: false, error: message };
  }
}

/**
 * Invites a new user to the platform.
 */
export async function inviteUserAction(
  data: { email: string; role: string; clientId?: string; vendorId?: string }
): Promise<ActionState<void>> {
  try {
    await AuthService.inviteUser(data);
    return { success: true };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to send invitation.';
    return { success: false, error: message };
  }
}
