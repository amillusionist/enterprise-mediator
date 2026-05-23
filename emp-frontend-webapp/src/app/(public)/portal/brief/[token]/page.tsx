import React from 'react';
import { notFound } from 'next/navigation';
import { ProjectService } from '@/services/project.service';
import { SanitizedSowViewer } from '@/components/features/sow/SanitizedSowViewer';
import { BriefActions } from './brief-actions';

interface ProjectBriefPageProps {
  params: {
    token: string;
  };
}

export default async function ProjectBriefPage({ params }: ProjectBriefPageProps) {
  const { token } = params;

  if (!token) notFound();

  try {
    const briefData = await ProjectService.getProjectBriefByToken(token);

    if (!briefData) {
      return (
        <div className="max-w-lg mx-auto bg-white p-8 rounded-lg shadow text-center mt-10">
          <h2 className="text-xl font-semibold text-gray-800">Brief Not Found</h2>
          <p className="text-gray-600 mt-2">The project brief link is invalid or has expired.</p>
        </div>
      );
    }

    return (
      <div className="max-w-4xl mx-auto space-y-8">
        <div className="bg-white shadow overflow-hidden sm:rounded-lg">
          <div className="px-4 py-5 sm:px-6 border-b border-gray-200">
            <h3 className="text-lg leading-6 font-medium text-gray-900">Project Opportunity</h3>
            <p className="mt-1 max-w-2xl text-sm text-gray-500">
              Review the sanitized project scope and requirements below.
            </p>
          </div>

          <div className="px-4 py-5 sm:p-6 space-y-6">
            {briefData.requiredSkills && briefData.requiredSkills.length > 0 && (
              <div>
                <h4 className="text-sm font-medium text-gray-500 mb-2">Required Skills</h4>
                <div className="flex flex-wrap gap-2">
                  {briefData.requiredSkills.map((skill) => (
                    <span key={skill} className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
                      {skill}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {briefData.technologies && briefData.technologies.length > 0 && (
              <div>
                <h4 className="text-sm font-medium text-gray-500 mb-2">Technologies</h4>
                <div className="flex flex-wrap gap-2">
                  {briefData.technologies.map((tech) => (
                    <span key={tech} className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-gray-100 text-gray-800">
                      {tech}
                    </span>
                  ))}
                </div>
              </div>
            )}

            {briefData.deliverables && briefData.deliverables.length > 0 && (
              <div>
                <h4 className="text-sm font-medium text-gray-500 mb-2">Deliverables</h4>
                <ul className="list-disc list-inside space-y-1 text-sm text-gray-700">
                  {briefData.deliverables.map((d, i) => (
                    <li key={i}>{d}</li>
                  ))}
                </ul>
              </div>
            )}

            {briefData.estimatedDurationWeeks && (
              <div>
                <h4 className="text-sm font-medium text-gray-500 mb-1">Estimated Duration</h4>
                <p className="text-sm text-gray-700">{briefData.estimatedDurationWeeks} weeks</p>
              </div>
            )}

            {briefData.sowData?.sanitizedContent && (
              <div className="border-t border-gray-200 pt-4">
                <h4 className="text-sm font-medium text-gray-500 mb-2">Sanitized SOW Content</h4>
                <SanitizedSowViewer
                  content={briefData.sowData.sanitizedContent}
                />
              </div>
            )}
          </div>

          <BriefActions token={token} />
        </div>
      </div>
    );
  } catch {
    return (
      <div className="max-w-lg mx-auto bg-white p-8 rounded-lg shadow text-center mt-10">
        <h2 className="text-xl font-semibold text-red-600">Service Error</h2>
        <p className="text-gray-600 mt-2">Unable to load project brief. Please try again later.</p>
      </div>
    );
  }
}
