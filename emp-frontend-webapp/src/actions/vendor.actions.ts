'use server';

import { revalidatePath, revalidateTag } from 'next/cache';
import { z } from 'zod';
import { VendorService } from '@/services/vendor.service';
import { VendorSchema } from '@/lib/schemas';
import type { VendorDTO } from '@/lib/types';

type VendorActionState = {
  success: boolean;
  message?: string;
  data?: VendorDTO;
  errors?: Record<string, string[]>;
};

/**
 * Creates a new vendor profile.
 */
export async function createVendorAction(
  prevState: VendorActionState | null,
  formData: FormData
): Promise<VendorActionState> {
  try {
    const rawData: Record<string, unknown> = {};
    formData.forEach((value, key) => {
      rawData[key] = value;
    });

    if (typeof rawData.skills === 'string') {
      try {
        rawData.skills = JSON.parse(rawData.skills as string);
      } catch {
        rawData.skills = [];
      }
    }

    const validated = VendorSchema.safeParse(rawData);

    if (!validated.success) {
      return {
        success: false,
        errors: validated.error.flatten().fieldErrors,
        message: 'Validation failed.',
      };
    }

    const newVendor = await VendorService.createVendor(validated.data);

    revalidateTag('vendors');
    revalidatePath('/admin/vendors');

    return { success: true, data: newVendor, message: 'Vendor created successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to create vendor.';
    return { success: false, message };
  }
}

/**
 * Updates an existing vendor profile.
 */
export async function updateVendorAction(
  vendorId: string,
  data: Partial<VendorDTO>
): Promise<VendorActionState> {
  try {
    const updatedVendor = await VendorService.updateVendor(vendorId, data);

    revalidatePath(`/admin/vendors/${vendorId}`);
    revalidateTag(`vendor-${vendorId}`);

    return { success: true, data: updatedVendor, message: 'Vendor profile updated.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to update vendor.';
    return { success: false, message };
  }
}

/**
 * Activates a pending vendor.
 */
export async function activateVendorAction(vendorId: string): Promise<VendorActionState> {
  try {
    await VendorService.activateVendor(vendorId);
    revalidatePath('/admin/vendors');
    revalidatePath(`/admin/vendors/${vendorId}`);
    return { success: true, message: 'Vendor activated successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to activate vendor.';
    return { success: false, message };
  }
}

/**
 * Deactivates a vendor.
 */
export async function deactivateVendorAction(vendorId: string): Promise<VendorActionState> {
  try {
    await VendorService.deactivateVendor(vendorId);
    revalidatePath('/admin/vendors');
    revalidatePath(`/admin/vendors/${vendorId}`);
    return { success: true, message: 'Vendor deactivated.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to deactivate vendor.';
    return { success: false, message };
  }
}

/**
 * Invites a new contact to the vendor account.
 */
export async function inviteVendorContactAction(
  vendorId: string,
  email: string
): Promise<VendorActionState> {
  try {
    const emailSchema = z.string().email();
    const result = emailSchema.safeParse(email);

    if (!result.success) {
      return { success: false, message: 'Invalid email address.' };
    }

    await VendorService.inviteContact(vendorId, email);
    revalidatePath(`/admin/vendors/${vendorId}`);

    return { success: true, message: `Invitation sent to ${email}.` };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to invite contact.';
    return { success: false, message };
  }
}
