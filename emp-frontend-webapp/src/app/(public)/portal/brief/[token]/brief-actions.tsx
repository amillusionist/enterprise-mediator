'use client';

import React, { useState } from 'react';
import { ProposalSubmissionForm } from '@/components/features/proposals/ProposalSubmissionForm';

interface BriefActionsProps {
  token: string;
}

export function BriefActions({ token }: BriefActionsProps) {
  const [showForm, setShowForm] = useState(false);
  const [declined, setDeclined] = useState(false);

  if (declined) {
    return (
      <div className="px-4 py-4 sm:px-6 bg-gray-50 border-t border-gray-200 text-center">
        <p className="text-sm text-gray-600">You have declined this opportunity.</p>
      </div>
    );
  }

  if (showForm) {
    return (
      <div className="px-4 py-5 sm:px-6 border-t border-gray-200">
        <ProposalSubmissionForm token={token} onCancel={() => setShowForm(false)} />
      </div>
    );
  }

  return (
    <div className="px-4 py-4 sm:px-6 bg-gray-50 border-t border-gray-200 flex justify-end space-x-3">
      <button
        onClick={() => setDeclined(true)}
        className="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
      >
        Decline
      </button>
      <button
        onClick={() => setShowForm(true)}
        className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500"
      >
        Submit Proposal
      </button>
    </div>
  );
}
