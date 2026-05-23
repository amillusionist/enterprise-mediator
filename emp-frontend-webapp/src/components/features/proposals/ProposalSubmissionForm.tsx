'use client';

import React, { useState, useRef } from 'react';
import { submitProposalAction } from '@/actions/proposal.actions';
import { ACCEPTED_FILE_TYPES, MAX_FILE_SIZE } from '@/lib/constants';

interface ProposalSubmissionFormProps {
  token: string;
  onCancel: () => void;
}

export function ProposalSubmissionForm({ token, onCancel }: ProposalSubmissionFormProps) {
  const [isPending, setIsPending] = useState(false);
  const [result, setResult] = useState<{ success: boolean; message?: string } | null>(null);
  const formRef = useRef<HTMLFormElement>(null);

  async function handleSubmit(e: React.FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setIsPending(true);
    setResult(null);

    const formData = new FormData(e.currentTarget);

    const file = formData.get('file') as File | null;
    if (file && file.size > 0) {
      if (!ACCEPTED_FILE_TYPES.includes(file.type)) {
        setResult({ success: false, message: 'Only .pdf, .docx, and .doc formats are supported.' });
        setIsPending(false);
        return;
      }
      if (file.size > MAX_FILE_SIZE) {
        setResult({ success: false, message: 'File size must be under 10MB.' });
        setIsPending(false);
        return;
      }
    }

    const res = await submitProposalAction(token, formData);
    setResult(res);
    setIsPending(false);

    if (res.success) {
      formRef.current?.reset();
    }
  }

  if (result?.success) {
    return (
      <div className="bg-green-50 border border-green-200 rounded-lg p-6 text-center">
        <h4 className="text-lg font-semibold text-green-800">Proposal Submitted</h4>
        <p className="text-sm text-green-700 mt-2">
          Thank you for your proposal. You will be notified once it has been reviewed.
        </p>
      </div>
    );
  }

  return (
    <form ref={formRef} onSubmit={handleSubmit} className="space-y-4">
      <h4 className="text-base font-semibold text-gray-900">Submit Your Proposal</h4>

      {result && !result.success && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded text-sm">
          {result.message}
        </div>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div>
          <label htmlFor="cost" className="block text-sm font-medium text-gray-700">
            Proposed Cost (USD)
          </label>
          <input
            id="cost"
            name="cost"
            type="number"
            min="0"
            step="0.01"
            required
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            placeholder="e.g. 50000"
          />
        </div>

        <div>
          <label htmlFor="timeline" className="block text-sm font-medium text-gray-700">
            Proposed Timeline
          </label>
          <input
            id="timeline"
            name="timeline"
            type="text"
            required
            className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
            placeholder="e.g. 12 weeks"
          />
        </div>
      </div>

      <div>
        <label htmlFor="keyPersonnel" className="block text-sm font-medium text-gray-700">
          Key Personnel
        </label>
        <input
          id="keyPersonnel"
          name="keyPersonnel"
          type="text"
          className="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
          placeholder="Comma-separated names (e.g. Jane Doe, John Smith)"
        />
      </div>

      <div>
        <label htmlFor="file" className="block text-sm font-medium text-gray-700">
          Proposal Document (optional)
        </label>
        <input
          id="file"
          name="file"
          type="file"
          accept=".pdf,.docx,.doc"
          className="mt-1 block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-blue-50 file:text-blue-700 hover:file:bg-blue-100"
        />
        <p className="mt-1 text-xs text-gray-500">PDF, DOCX, or DOC. Max 10MB.</p>
      </div>

      <div className="flex justify-end space-x-3">
        <button
          type="button"
          onClick={onCancel}
          disabled={isPending}
          className="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
        >
          Cancel
        </button>
        <button
          type="submit"
          disabled={isPending}
          className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
        >
          {isPending ? 'Submitting...' : 'Submit Proposal'}
        </button>
      </div>
    </form>
  );
}
