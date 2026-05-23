'use server';

import { revalidatePath } from 'next/cache';
import { FinanceService } from '@/services/finance.service';
import { InvoiceSchema } from '@/lib/schemas';
import type { GenerateInvoiceInput, RetentionPolicy } from '@/lib/types';

type FinanceActionState = {
  success: boolean;
  message?: string;
  data?: unknown;
  errors?: Record<string, string[]>;
};

/**
 * Generates and sends an invoice for a specific project.
 */
export async function createInvoiceAction(
  projectId: string,
  data: GenerateInvoiceInput
): Promise<FinanceActionState> {
  try {
    const validated = InvoiceSchema.safeParse(data);

    if (!validated.success) {
      return {
        success: false,
        errors: validated.error.flatten().fieldErrors,
        message: 'Invalid invoice data.',
      };
    }

    const invoice = await FinanceService.generateInvoice(projectId, validated.data);

    revalidatePath(`/admin/projects/${projectId}`);
    return { success: true, data: invoice, message: 'Invoice generated and sent.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to create invoice.';
    return { success: false, message };
  }
}

/**
 * Initiates a payout workflow for a vendor.
 */
export async function initiatePayoutAction(
  projectId: string,
  milestoneId: string | null,
  amount: number
): Promise<FinanceActionState> {
  try {
    if (amount <= 0) {
      return { success: false, message: 'Payout amount must be greater than zero.' };
    }

    await FinanceService.initiatePayout({ projectId, milestoneId, amount });

    revalidatePath('/admin/finance/payouts');
    revalidatePath(`/admin/projects/${projectId}`);

    return { success: true, message: 'Payout initiated successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to initiate payout.';
    return { success: false, message };
  }
}

/**
 * Approves a pending payout request.
 */
export async function approvePayoutAction(payoutId: string): Promise<FinanceActionState> {
  try {
    await FinanceService.approvePayout(payoutId);
    revalidatePath('/admin/finance/payouts');
    return { success: true, message: 'Payout approved successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to approve payout.';
    return { success: false, message };
  }
}

/**
 * Rejects a pending payout request.
 */
export async function rejectPayoutAction(
  payoutId: string,
  reason: string
): Promise<FinanceActionState> {
  try {
    if (!reason) return { success: false, message: 'Rejection reason is required.' };

    await FinanceService.rejectPayout(payoutId, reason);
    revalidatePath('/admin/finance/payouts');
    return { success: true, message: 'Payout rejected.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to reject payout.';
    return { success: false, message };
  }
}

/**
 * Processes a refund for a project.
 */
export async function processRefundAction(
  projectId: string,
  amount: number,
  reason: string
): Promise<FinanceActionState> {
  try {
    await FinanceService.processRefund(projectId, amount, reason);
    revalidatePath(`/admin/projects/${projectId}`);
    return { success: true, message: 'Refund processed successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to process refund.';
    return { success: false, message };
  }
}

/**
 * Confirms a Stripe payment (called after successful client-side Stripe flow).
 */
export async function processInvoicePayment(
  invoiceId: string,
  paymentIntentId: string
): Promise<FinanceActionState> {
  try {
    await FinanceService.processInvoicePayment(invoiceId, paymentIntentId);
    return { success: true, message: 'Payment confirmed.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Payment confirmation failed.';
    return { success: false, message };
  }
}

/**
 * Updates data retention policy settings.
 */
export async function updateRetentionPolicyAction(
  data: RetentionPolicy
): Promise<FinanceActionState> {
  try {
    await FinanceService.updateRetentionPolicy(data);
    return { success: true, message: 'Retention policy updated.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to update retention policy.';
    return { success: false, message };
  }
}
