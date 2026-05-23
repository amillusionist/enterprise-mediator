'use server';

import { revalidatePath, revalidateTag } from 'next/cache';
import { ClientService } from '@/services/client.service';
import type { CreateClientInput, ClientDTO } from '@/lib/types';

type ClientActionState = {
  success: boolean;
  message?: string;
  data?: ClientDTO;
  errors?: Record<string, string[]>;
};

export async function createClientAction(
  data: CreateClientInput
): Promise<ClientActionState> {
  try {
    if (!data.companyName) {
      return { success: false, message: 'Company name is required.' };
    }
    if (!data.contacts || data.contacts.length === 0) {
      return { success: false, message: 'At least one contact is required.' };
    }

    const client = await ClientService.createClient(data);

    revalidateTag('clients');
    revalidatePath('/admin/clients');

    return { success: true, data: client, message: 'Client created successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to create client.';
    console.error('Create Client Action Error:', error);
    return { success: false, message };
  }
}
