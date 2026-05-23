'use client';

import React, { useTransition } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { projectBriefSchema } from '@/lib/schemas';
import { saveSowDataAction } from '@/actions/project.actions';
import type { ProjectDTO } from '@/lib/types';
import { z } from 'zod';

type FormData = z.infer<typeof projectBriefSchema>;

interface SowExtractionFormProps {
  project: ProjectDTO;
  onSaveSuccess?: () => void;
}

export function SowExtractionForm({ project, onSaveSuccess }: SowExtractionFormProps) {
  const [isPending, startTransition] = useTransition();
  const [serverError, setServerError] = React.useState<string | null>(null);

  const { register, handleSubmit, formState: { errors, isDirty } } = useForm<FormData>({
    resolver: zodResolver(projectBriefSchema),
    defaultValues: {
      title: project.name || '',
      summary: project.description || '',
      deliverables: [],
      requiredSkills: [],
      technologies: [],
    }
  });

  const onSubmit = (data: FormData) => {
    setServerError(null);
    startTransition(async () => {
      try {
        const result = await saveSowDataAction(project.id, {
          title: data.title,
          summary: data.summary,
          requiredSkills: data.requiredSkills,
          deliverables: data.deliverables,
          technologies: data.technologies,
          scope: data.scope,
          estimatedDurationWeeks: data.estimatedDurationWeeks,
          estimatedBudget: data.estimatedBudget,
        });
        if (result.success) {
          if (onSaveSuccess) onSaveSuccess();
        } else {
          setServerError(result.message || 'Failed to update project brief');
        }
      } catch {
        setServerError('An unexpected error occurred.');
      }
    });
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-6 bg-white p-6 rounded-lg shadow-sm border border-slate-200">
      <div className="space-y-4">
        <div>
          <label htmlFor="title" className="block text-sm font-medium text-slate-700">Project Title</label>
          <input
            {...register('title')}
            type="text"
            id="title"
            className="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2 border"
            placeholder="e.g., Cloud Migration Strategy"
          />
          {errors.title && <p className="mt-1 text-xs text-red-500">{errors.title.message}</p>}
        </div>

        <div>
          <label htmlFor="summary" className="block text-sm font-medium text-slate-700">Scope Summary</label>
          <textarea
            {...register('summary')}
            id="summary"
            rows={4}
            className="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2 border"
            placeholder="Executive summary of the SOW..."
          />
          {errors.summary && <p className="mt-1 text-xs text-red-500">{errors.summary.message}</p>}
        </div>

        <div>
          <label htmlFor="scope" className="block text-sm font-medium text-slate-700">Scope Details</label>
          <textarea
            {...register('scope')}
            id="scope"
            rows={3}
            className="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2 border"
            placeholder="Detailed scope description..."
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label htmlFor="estimatedDurationWeeks" className="block text-sm font-medium text-slate-700">Estimated Duration (weeks)</label>
            <input
              {...register('estimatedDurationWeeks')}
              type="number"
              id="estimatedDurationWeeks"
              className="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2 border"
              placeholder="e.g., 12"
            />
          </div>
          <div>
            <label htmlFor="estimatedBudget" className="block text-sm font-medium text-slate-700">Estimated Budget</label>
            <input
              {...register('estimatedBudget')}
              type="number"
              id="estimatedBudget"
              className="mt-1 block w-full rounded-md border-slate-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2 border"
              placeholder="e.g., 150000"
            />
          </div>
        </div>
      </div>

      {serverError && (
        <div className="p-3 bg-red-50 border border-red-200 rounded text-sm text-red-600">
          {serverError}
        </div>
      )}

      <div className="pt-4 flex justify-end gap-3">
        <button
          type="button"
          className="px-4 py-2 border border-slate-300 rounded-md shadow-sm text-sm font-medium text-slate-700 bg-white hover:bg-slate-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
          disabled={isPending}
        >
          Cancel
        </button>
        <button
          type="submit"
          className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
          disabled={isPending || !isDirty}
        >
          {isPending && (
            <svg className="h-4 w-4 mr-2 animate-spin" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
            </svg>
          )}
          {isPending ? 'Saving...' : 'Save & Finalize Brief'}
        </button>
      </div>
    </form>
  );
}
