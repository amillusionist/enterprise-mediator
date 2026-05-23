import React from 'react';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { VendorService } from '@/services/vendor.service';
import { formatDate } from '@/lib/utils';
import { VendorStatusActions } from './vendor-status-actions';

interface VendorDetailPageProps {
  params: {
    vendorId: string;
  };
}

const STATUS_BADGE: Record<string, string> = {
  Active: 'bg-green-100 text-green-800',
  Pending: 'bg-yellow-100 text-yellow-800',
  Suspended: 'bg-red-100 text-red-800',
  Deactivated: 'bg-gray-100 text-gray-800',
};

export default async function VendorDetailPage({ params }: VendorDetailPageProps) {
  const { vendorId } = params;

  if (!vendorId) notFound();

  let vendor;

  try {
    vendor = await VendorService.getVendorById(vendorId);
  } catch {
    notFound();
  }

  if (!vendor) notFound();

  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center sm:justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-gray-900">{vendor.companyName}</h1>
            <span
              className={`inline-flex rounded-full px-2.5 py-0.5 text-xs font-semibold ${STATUS_BADGE[vendor.status] || 'bg-gray-100 text-gray-800'}`}
            >
              {vendor.status}
            </span>
          </div>
          <p className="mt-1 text-sm text-gray-500">
            Created: {formatDate(vendor.createdAt)}
          </p>
        </div>
        <div className="mt-4 sm:mt-0 flex gap-2">
          <Link href="/admin/vendors" className="text-sm text-blue-600 hover:text-blue-800">
            &larr; Back to Vendors
          </Link>
        </div>
      </div>

      {/* Vendor Details */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-lg font-medium text-gray-900 mb-4">Company Information</h2>
          <dl className="space-y-3">
            <div>
              <dt className="text-sm font-medium text-gray-500">Company Name</dt>
              <dd className="mt-1 text-sm text-gray-900">{vendor.companyName}</dd>
            </div>
            {vendor.country && (
              <div>
                <dt className="text-sm font-medium text-gray-500">Country</dt>
                <dd className="mt-1 text-sm text-gray-900">{vendor.country}</dd>
              </div>
            )}
            {vendor.city && (
              <div>
                <dt className="text-sm font-medium text-gray-500">City</dt>
                <dd className="mt-1 text-sm text-gray-900">{vendor.city}</dd>
              </div>
            )}
            {vendor.address && (
              <div>
                <dt className="text-sm font-medium text-gray-500">Address</dt>
                <dd className="mt-1 text-sm text-gray-900">{vendor.address}</dd>
              </div>
            )}
          </dl>
        </div>

        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-lg font-medium text-gray-900 mb-4">Primary Contact</h2>
          <dl className="space-y-3">
            {vendor.primaryContactName && (
              <div>
                <dt className="text-sm font-medium text-gray-500">Name</dt>
                <dd className="mt-1 text-sm text-gray-900">{vendor.primaryContactName}</dd>
              </div>
            )}
            {vendor.primaryContactEmail && (
              <div>
                <dt className="text-sm font-medium text-gray-500">Email</dt>
                <dd className="mt-1 text-sm text-gray-900">
                  <a href={`mailto:${vendor.primaryContactEmail}`} className="text-blue-600 hover:text-blue-800">
                    {vendor.primaryContactEmail}
                  </a>
                </dd>
              </div>
            )}
            {vendor.primaryContactPhone && (
              <div>
                <dt className="text-sm font-medium text-gray-500">Phone</dt>
                <dd className="mt-1 text-sm text-gray-900">{vendor.primaryContactPhone}</dd>
              </div>
            )}
          </dl>
        </div>
      </div>

      {/* Performance Metrics */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-sm font-medium text-gray-500">Projects Awarded</h3>
          <p className="mt-1 text-2xl font-semibold text-gray-900">
            {vendor.projectsAwarded ?? 0}
          </p>
        </div>
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-sm font-medium text-gray-500">Average Rating</h3>
          <p className="mt-1 text-2xl font-semibold text-gray-900">
            {vendor.averageRating != null ? `${vendor.averageRating.toFixed(1)} / 5.0` : 'N/A'}
          </p>
        </div>
        <div className="bg-white shadow rounded-lg p-6">
          <h3 className="text-sm font-medium text-gray-500">On-Time Completion</h3>
          <p className="mt-1 text-2xl font-semibold text-gray-900">
            {vendor.onTimeCompletionRate != null ? `${Math.round(vendor.onTimeCompletionRate * 100)}%` : 'N/A'}
          </p>
        </div>
      </div>

      {/* Skills */}
      {vendor.skills.length > 0 && (
        <div className="bg-white shadow rounded-lg p-6">
          <h2 className="text-lg font-medium text-gray-900 mb-4">Skills</h2>
          <div className="flex flex-wrap gap-2">
            {vendor.skills.map((skill) => (
              <span
                key={skill}
                className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-50 text-blue-700"
              >
                {skill}
              </span>
            ))}
          </div>
        </div>
      )}

      {/* Actions */}
      <div className="bg-white shadow rounded-lg p-6">
        <h2 className="text-lg font-medium text-gray-900 mb-4">Actions</h2>
        <VendorStatusActions vendorId={vendorId} currentStatus={vendor.status} />
      </div>
    </div>
  );
}
