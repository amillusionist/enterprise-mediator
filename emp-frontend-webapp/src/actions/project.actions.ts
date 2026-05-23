'use server';

import { revalidatePath, revalidateTag } from 'next/cache';
import { ProjectService } from '@/services/project.service';
import { SowValidationSchema } from '@/lib/schemas';
import type { ProjectDTO, CreateProjectRequest, UpdateProjectBriefRequest } from '@/lib/types';

type ProjectActionState = {
  success: boolean;
  message?: string;
  data?: ProjectDTO;
  errors?: Record<string, string[]>;
};

/**
 * Creates a new project.
 */
export async function createProjectAction(
  data: CreateProjectRequest
): Promise<ProjectActionState> {
  try {
    if (!data.name || !data.clientId) {
      return { success: false, message: 'Project name and client are required.' };
    }

    const newProject = await ProjectService.createProject(data);

    revalidateTag('projects');
    revalidatePath('/admin/dashboard');

    return { success: true, data: newProject, message: 'Project created successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to create project.';
    console.error('Create Project Action Error:', error);
    return { success: false, message };
  }
}

/**
 * Handles the upload of an SOW document.
 */
export async function uploadSowAction(
  projectId: string,
  formData: FormData
): Promise<ProjectActionState> {
  try {
    const file = formData.get('file') as File;

    if (!file) {
      return { success: false, message: 'No file provided.' };
    }

    const validationResult = SowValidationSchema.safeParse({
      size: file.size,
      type: file.type,
      name: file.name,
    });

    if (!validationResult.success) {
      return {
        success: false,
        message: 'File validation failed.',
        errors: validationResult.error.flatten().fieldErrors,
      };
    }

    await ProjectService.uploadSow(projectId, formData);

    revalidatePath(`/admin/sow-review/${projectId}`);
    revalidateTag(`project-${projectId}`);

    return { success: true, message: 'SOW uploaded successfully. Processing started.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to upload SOW.';
    console.error('SOW Upload Action Error:', error);
    return { success: false, message };
  }
}

/**
 * Updates the status of a project.
 */
export async function updateProjectStatusAction(
  projectId: string,
  status: string
): Promise<ProjectActionState> {
  try {
    await ProjectService.updateProjectStatus(projectId, status);
    revalidatePath(`/admin/projects/${projectId}`);
    return { success: true, message: 'Project status updated.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to update status.';
    return { success: false, message };
  }
}

/**
 * Approves the Project Brief, locking SOW data and triggering vendor matching.
 */
export async function approveProjectBriefAction(
  projectId: string
): Promise<ProjectActionState> {
  try {
    await ProjectService.approveBrief(projectId);
    revalidatePath(`/admin/sow-review/${projectId}`);
    return { success: true, message: 'Project Brief approved. Vendor matching initiated.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to approve brief.';
    return { success: false, message };
  }
}

/**
 * Saves edits to the extracted SOW data (Human-in-the-loop).
 */
export async function saveSowDataAction(
  projectId: string,
  data: UpdateProjectBriefRequest
): Promise<ProjectActionState> {
  try {
    await ProjectService.updateBrief(projectId, data);
    revalidatePath(`/admin/sow-review/${projectId}`);
    return { success: true, message: 'SOW data saved successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to save SOW data.';
    return { success: false, message };
  }
}

/**
 * Distributes the project brief to selected vendors.
 */
export async function distributeBriefAction(
  projectId: string,
  vendorIds: string[]
): Promise<ProjectActionState> {
  try {
    await ProjectService.distributeBrief(projectId, vendorIds);
    revalidatePath(`/admin/projects/${projectId}`);
    return { success: true, message: `Brief distributed to ${vendorIds.length} vendor(s).` };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to distribute brief.';
    return { success: false, message };
  }
}

/**
 * Approves a milestone via token (public, unauthenticated).
 */
export async function approveMilestone(token: string): Promise<ProjectActionState> {
  try {
    await ProjectService.approveMilestone(token, 'approved');
    return { success: true, message: 'Milestone approved successfully.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to approve milestone.';
    return { success: false, message };
  }
}

/**
 * Awards the project to a specific vendor.
 */
export async function awardProjectAction(
  projectId: string,
  vendorId: string
): Promise<{ error?: string }> {
  try {
    await ProjectService.awardProject(projectId, vendorId);
    revalidatePath(`/admin/proposals/${projectId}`);
    revalidatePath(`/admin/projects/${projectId}`);
    revalidateTag('projects');
    return {};
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to award project.';
    console.error('Award Project Action Error:', error);
    return { error: message };
  }
}

/**
 * Rejects a milestone via token (public, unauthenticated).
 */
export async function rejectMilestone(token: string, reason: string): Promise<ProjectActionState> {
  try {
    await ProjectService.approveMilestone(token, 'rejected', reason);
    return { success: true, message: 'Milestone rejected.' };
  } catch (error: unknown) {
    const message = error instanceof Error ? error.message : 'Failed to reject milestone.';
    return { success: false, message };
  }
}
