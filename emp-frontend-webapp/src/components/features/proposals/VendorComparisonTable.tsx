'use client';

import React, { useState } from 'react';
import { formatCurrency } from '@/lib/utils';
import { awardProjectAction } from '@/actions/project.actions';
import type { ProposalDTO } from '@/lib/types';

interface VendorComparisonTableProps {
  proposals: ProposalDTO[];
  projectId: string;
}

export default function VendorComparisonTable({ proposals, projectId }: VendorComparisonTableProps) {
  const [awarding, setAwarding] = useState<string | null>(null);
  const [awardError, setAwardError] = useState<string | null>(null);
  const [awardedVendorId, setAwardedVendorId] = useState<string | null>(null);

  const costs = proposals.map((p) => p.cost);
  const minCost = Math.min(...costs);

  async function handleSelectWinner(vendorId: string) {
    if (!confirm('Are you sure you want to award this project to this vendor?')) return;

    setAwarding(vendorId);
    setAwardError(null);

    try {
      const result = await awardProjectAction(projectId, vendorId);
      if (result?.error) {
        setAwardError(result.error);
      } else {
        setAwardedVendorId(vendorId);
      }
    } catch {
      setAwardError('Failed to award project. Please try again.');
    } finally {
      setAwarding(null);
    }
  }

  return (
    <div className="overflow-x-auto rounded-lg border border-slate-200 shadow-sm">
      <table className="min-w-full divide-y divide-slate-200">
        <thead className="bg-slate-50">
          <tr>
            <th scope="col" className="px-6 py-4 text-left text-xs font-semibold text-slate-500 uppercase tracking-wider bg-slate-50 sticky left-0 z-10 w-48">
              Criteria
            </th>
            {proposals.map((proposal) => (
              <th key={proposal.id} scope="col" className="px-6 py-4 text-center text-sm font-bold text-slate-900 min-w-[200px]">
                {proposal.vendorName}
                {proposal.status === 'Shortlisted' && (
                  <span className="block mt-1 text-[10px] font-normal text-blue-600 bg-blue-50 py-0.5 rounded-full px-2 w-fit mx-auto">
                    Shortlisted
                  </span>
                )}
                {awardedVendorId === proposal.vendorId && (
                  <span className="block mt-1 text-[10px] font-normal text-green-600 bg-green-50 py-0.5 rounded-full px-2 w-fit mx-auto">
                    Awarded
                  </span>
                )}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="bg-white divide-y divide-slate-200">
          {/* Total Cost Row */}
          <tr>
            <td className="px-6 py-4 text-sm font-medium text-slate-900 bg-slate-50 sticky left-0">
              Total Cost
            </td>
            {proposals.map((p) => (
              <td key={p.id} className={`px-6 py-4 text-center text-sm ${p.cost === minCost ? 'bg-green-50 font-semibold text-green-700' : 'text-slate-600'}`}>
                {formatCurrency(p.cost, p.currency)}
                {p.cost === minCost && <span className="block text-[10px] text-green-600">Best Price</span>}
              </td>
            ))}
          </tr>

          {/* Timeline Row */}
          <tr>
            <td className="px-6 py-4 text-sm font-medium text-slate-900 bg-slate-50 sticky left-0">
              Timeline
            </td>
            {proposals.map((p) => (
              <td key={p.id} className="px-6 py-4 text-center text-sm text-slate-600">
                {p.timeline}
              </td>
            ))}
          </tr>

          {/* Status Row */}
          <tr>
            <td className="px-6 py-4 text-sm font-medium text-slate-900 bg-slate-50 sticky left-0">
              Status
            </td>
            {proposals.map((p) => (
              <td key={p.id} className="px-6 py-4 text-center text-sm">
                <span className={`inline-flex rounded-full px-2 text-xs font-semibold leading-5 ${
                  p.status === 'Submitted' ? 'bg-blue-100 text-blue-800' :
                  p.status === 'Shortlisted' ? 'bg-yellow-100 text-yellow-800' :
                  p.status === 'Accepted' ? 'bg-green-100 text-green-800' :
                  p.status === 'Rejected' ? 'bg-red-100 text-red-800' :
                  'bg-gray-100 text-gray-800'
                }`}>
                  {p.status}
                </span>
              </td>
            ))}
          </tr>

          {/* Key Personnel Row */}
          <tr>
            <td className="px-6 py-4 text-sm font-medium text-slate-900 bg-slate-50 sticky left-0">
              Key Personnel
            </td>
            {proposals.map((p) => (
              <td key={p.id} className="px-6 py-4 text-center text-sm text-slate-600">
                {p.keyPersonnel && p.keyPersonnel.length > 0
                  ? p.keyPersonnel.join(', ')
                  : <span className="text-slate-400">Not specified</span>
                }
              </td>
            ))}
          </tr>

          {/* Submitted Date Row */}
          <tr>
            <td className="px-6 py-4 text-sm font-medium text-slate-900 bg-slate-50 sticky left-0">
              Submitted
            </td>
            {proposals.map((p) => (
              <td key={p.id} className="px-6 py-4 text-center text-sm text-slate-600">
                {new Date(p.submittedAt).toLocaleDateString()}
              </td>
            ))}
          </tr>

          {/* Action Row */}
          {!awardedVendorId && (
            <tr>
              <td className="px-6 py-4 text-sm font-medium text-slate-900 bg-slate-50 sticky left-0">
                Action
              </td>
              {proposals.map((p) => (
                <td key={p.id} className="px-6 py-4 text-center">
                  <button
                    onClick={() => handleSelectWinner(p.vendorId)}
                    disabled={awarding !== null}
                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 disabled:opacity-50"
                  >
                    {awarding === p.vendorId ? (
                      <>
                        <svg className="animate-spin -ml-1 mr-2 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
                        </svg>
                        Awarding...
                      </>
                    ) : (
                      <>
                        <svg className="h-4 w-4 mr-2" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                          <path fillRule="evenodd" d="M2.25 12c0-5.385 4.365-9.75 9.75-9.75s9.75 4.365 9.75 9.75-4.365 9.75-9.75 9.75S2.25 17.385 2.25 12zm13.36-1.814a.75.75 0 10-1.22-.872l-3.236 4.53L9.53 12.22a.75.75 0 00-1.06 1.06l2.25 2.25a.75.75 0 001.14-.094l3.75-5.25z" clipRule="evenodd" />
                        </svg>
                        Select Winner
                      </>
                    )}
                  </button>
                </td>
              ))}
            </tr>
          )}
        </tbody>
      </table>

      {awardError && (
        <div className="px-4 py-3 bg-red-50 border-t border-red-200 text-sm text-red-700">
          {awardError}
        </div>
      )}

      {awardedVendorId && (
        <div className="px-4 py-3 bg-green-50 border-t border-green-200 text-sm text-green-700">
          Project successfully awarded! The vendor has been notified.
        </div>
      )}
    </div>
  );
}
