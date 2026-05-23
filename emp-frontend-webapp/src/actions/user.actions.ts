'use server';

import { revalidatePath, revalidateTag } from 'next/cache';
import { UserService } from '@/services/user.service';
import { userEditSchema } from '@/lib/schemas';
import type { User, UserRole } from '@/lib/types';

type UserActionState = {
  success: boolean;
  message?: string;
  data?: User;
  errors?: Record<string, string[]>;
};

/**
 * Updates a user's role and active status.
 */
export async function updateUserAction(
  userId: string,
  data: { role: UserRole; isActive: boolean }
): Promise<UserActionState> {
  try {
    const validated = userEditSchema.safeParse(data);

    if (!validated.success) {
      return {
        success: false,
        errors: validated.error.flatten().fieldErrors,
        message: 'Validation failed.',
      };
    }

    const updatedUser = await UserService.updateUser(userId, validated.data);

    revalidatePath('/admin/users');
    revalidatePath(`/admin/users/${userId}`);
    revalidateTag(`user-${userId}`);

    return { success: true, data: updatedUser, message: 'User updated successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to update user.';
    return { success: false, message };
  }
}
