import React from 'react';
import { notFound } from 'next/navigation';
import { ProjectService } from '@/services/project.service';
import { ProposalService } from '@/services/proposal.service';
import VendorComparisonTable from '@/components/features/proposals/VendorComparisonTable';
import { formatCurrency } from '@/lib/utils';
import type { ProposalDTO } from '@/lib/types';

interface ProposalComparisonPageProps {
  params: {
    projectId: string;
  };
}

export default async function ProposalComparisonPage({ params }: ProposalComparisonPageProps) {
  const { projectId } = params;

  if (!projectId) notFound();

  try {
    const [project, proposals] = await Promise.all([
      ProjectService.getProjectById(projectId),
      ProposalService.getProjectProposals(projectId),
    ]);

    if (!project) notFound();

    return (
      <div className="space-y-6">
        <div className="sm:flex sm:items-center">
          <div className="sm:flex-auto">
            <h1 className="text-2xl font-bold text-gray-900">Proposal Comparison</h1>
            <p className="mt-2 text-sm text-gray-700">
              Project: <span className="font-medium">{project.name}</span> — {proposals.length} proposal(s) received
            </p>
          </div>
        </div>

        {proposals.length === 0 ? (
          <div className="bg-white p-8 rounded-lg shadow text-center">
            <h3 className="text-lg font-medium text-gray-900">No Proposals Yet</h3>
            <p className="mt-2 text-sm text-gray-500">
              Vendors have not yet submitted proposals for this project. Check back later.
            </p>
          </div>
        ) : (
          <VendorComparisonTable proposals={proposals} projectId={projectId} />
        )}
      </div>
    );
  } catch (error) {
    console.error('Failed to load proposals:', error);
    return (
      <div className="bg-white p-8 rounded-lg shadow text-center">
        <h3 className="text-lg font-medium text-red-600">Failed to load proposals</h3>
        <p className="mt-2 text-sm text-gray-500">Please try again later.</p>
      </div>
    );
  }
}
