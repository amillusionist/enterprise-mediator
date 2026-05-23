'use client';

import React, { useState, useEffect, useCallback, useTransition } from 'react';
import { useRouter } from 'next/navigation';

import type { SowExtractionData } from '@/lib/types';
import { useNotificationStore } from '@/store/use-notification-store';
import { saveSowDataAction, approveProjectBriefAction } from '@/actions/project.actions';
import { SanitizedSowViewer } from '@/components/features/sow/SanitizedSowViewer';
import { ComparisonSplitView } from '@/components/features/sow/ComparisonSplitView';

interface SowReviewCompositeProps {
  projectId: string;
  initialData: SowExtractionData;
}

export default function SowReviewComposite({ projectId, initialData }: SowReviewCompositeProps) {
  const router = useRouter();
  const addNotification = useNotificationStore((s) => s.addNotification);
  const [isPending, startTransition] = useTransition();

  const [formData, setFormData] = useState<SowExtractionData>(() => initialData);
  const [initialSnapshot] = useState<string>(() => JSON.stringify(initialData));
  const [isDirty, setIsDirty] = useState(false);
  const [showApproveModal, setShowApproveModal] = useState(false);

  useEffect(() => {
    setIsDirty(JSON.stringify(formData) !== initialSnapshot);
  }, [formData, initialSnapshot]);

  useEffect(() => {
    const handleBeforeUnload = (e: BeforeUnloadEvent) => {
      if (isDirty) {
        e.preventDefault();
        e.returnValue = '';
      }
    };
    window.addEventListener('beforeunload', handleBeforeUnload);
    return () => window.removeEventListener('beforeunload', handleBeforeUnload);
  }, [isDirty]);

  const handleFieldChange = useCallback((field: keyof SowExtractionData, value: string | string[] | Record<string, unknown>) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  }, []);

  const handleSave = () => {
    startTransition(async () => {
      try {
        const result = await saveSowDataAction(projectId, {
          title: formData.projectName || '',
          summary: formData.scopeSummary || '',
          requiredSkills: formData.requiredSkills || [],
          deliverables: formData.deliverables || [],
          technologies: formData.technologies || [],
        });

        if (result.success) {
          addNotification({
            type: 'success',
            title: 'Changes Saved',
            message: 'The project brief has been updated successfully.',
          });
          router.refresh();
        } else {
          addNotification({
            type: 'error',
            title: 'Save Failed',
            message: result.message || 'An unexpected error occurred while saving.',
          });
        }
      } catch {
        addNotification({
          type: 'error',
          title: 'System Error',
          message: 'Failed to communicate with the server.',
        });
      }
    });
  };

  const handleApproveClick = () => {
    if (isDirty) {
      addNotification({
        type: 'error',
        title: 'Unsaved Changes',
        message: 'Please save your changes before approving the brief.',
      });
      return;
    }
    setShowApproveModal(true);
  };

  const confirmApproval = () => {
    setShowApproveModal(false);
    startTransition(async () => {
      try {
        const result = await approveProjectBriefAction(projectId);
        if (result.success) {
          addNotification({
            type: 'success',
            title: 'Project Brief Approved',
            message: 'The brief is now locked and vendor matching has started.',
          });
          router.refresh();
        } else {
          addNotification({
            type: 'error',
            title: 'Approval Failed',
            message: result.message || 'Could not approve the project brief.',
          });
        }
      } catch {
        addNotification({
          type: 'error',
          title: 'Operation Failed',
          message: 'An unknown error occurred.',
        });
      }
    });
  };

  return (
    <div className="flex flex-col h-full w-full">
      {/* Toolbar */}
      <div className="flex items-center justify-between px-6 py-4 border-b bg-white">
        <div>
          <h2 className="text-lg font-semibold text-gray-900">Review Project Brief</h2>
          <p className="text-sm text-gray-500">
            Verify AI-extracted data against the sanitized SOW.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <span className="text-sm mr-2">
            {isDirty ? (
              <span className="text-amber-500 font-medium">Unsaved Changes</span>
            ) : (
              <span className="text-green-600">All changes saved</span>
            )}
          </span>
          <button
            onClick={handleSave}
            disabled={!isDirty || isPending}
            className="px-4 py-2 text-sm font-medium border border-gray-300 rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50 disabled:opacity-50"
          >
            {isPending ? 'Saving...' : 'Save Draft'}
          </button>
          <button
            onClick={handleApproveClick}
            disabled={isPending}
            className="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-md shadow-sm disabled:opacity-50"
          >
            {isPending ? 'Processing...' : 'Approve & Finalize'}
          </button>
        </div>
      </div>

      {/* Split View */}
      <div className="flex-1 overflow-hidden p-6">
        <ComparisonSplitView
          leftPanel={
            <SanitizedSowViewer
              content={formData.sanitizedContent || ''}
              isLoading={false}
            />
          }
          rightPanel={
            <div className="space-y-6 bg-white p-6 rounded-lg shadow-sm border border-gray-200">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Project Name</label>
                <input
                  type="text"
                  value={formData.projectName || ''}
                  onChange={(e) => handleFieldChange('projectName', e.target.value)}
                  className="w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Scope Summary</label>
                <textarea
                  rows={4}
                  value={formData.scopeSummary || ''}
                  onChange={(e) => handleFieldChange('scopeSummary', e.target.value)}
                  className="w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Required Skills</label>
                <input
                  type="text"
                  value={(formData.requiredSkills || []).join(', ')}
                  onChange={(e) => handleFieldChange('requiredSkills', e.target.value.split(',').map((s) => s.trim()).filter(Boolean))}
                  className="w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
                  placeholder="React, AWS, Python (comma separated)"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Technologies</label>
                <input
                  type="text"
                  value={(formData.technologies || []).join(', ')}
                  onChange={(e) => handleFieldChange('technologies', e.target.value.split(',').map((s) => s.trim()).filter(Boolean))}
                  className="w-full rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
                  placeholder="PostgreSQL, Docker, Kubernetes (comma separated)"
                />
              </div>

              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Key Deliverables</label>
                <div className="space-y-2">
                  {(formData.deliverables || []).map((d, i) => (
                    <div key={i} className="flex gap-2">
                      <input
                        type="text"
                        value={d}
                        onChange={(e) => {
                          const updated = [...(formData.deliverables || [])];
                          updated[i] = e.target.value;
                          handleFieldChange('deliverables', updated);
                        }}
                        className="flex-1 rounded-md border border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm p-2"
                      />
                      <button
                        type="button"
                        onClick={() => {
                          const updated = (formData.deliverables || []).filter((_, idx) => idx !== i);
                          handleFieldChange('deliverables', updated);
                        }}
                        className="p-2 text-gray-400 hover:text-red-500"
                        aria-label="Remove deliverable"
                      >
                        <svg className="h-5 w-5" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                        </svg>
                      </button>
                    </div>
                  ))}
                  <button
                    type="button"
                    onClick={() => handleFieldChange('deliverables', [...(formData.deliverables || []), ''])}
                    className="inline-flex items-center text-xs font-medium text-blue-600 hover:text-blue-800"
                  >
                    <svg className="h-4 w-4 mr-1" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                    </svg>
                    Add Deliverable
                  </button>
                </div>
              </div>
            </div>
          }
        />
      </div>

      {/* Approval Confirmation Modal */}
      {showApproveModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50">
          <div className="bg-white rounded-lg shadow-xl max-w-md w-full p-6">
            <h3 className="text-lg font-semibold text-gray-900 mb-2">Approve Project Brief?</h3>
            <p className="text-sm text-gray-600 mb-4">
              This action will finalize the Project Brief and lock it for editing.
              The system will immediately begin matching vendors based on these requirements.
            </p>
            <p className="text-sm font-semibold text-gray-800 mb-6">This action cannot be undone.</p>
            <div className="flex justify-end gap-3">
              <button
                onClick={() => setShowApproveModal(false)}
                className="px-4 py-2 text-sm font-medium border border-gray-300 rounded-md text-gray-700 bg-white hover:bg-gray-50"
              >
                Cancel
              </button>
              <button
                onClick={confirmApproval}
                className="px-4 py-2 text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 rounded-md shadow-sm"
              >
                Confirm Approval
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
