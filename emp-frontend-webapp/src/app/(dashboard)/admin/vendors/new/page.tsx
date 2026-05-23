import React from 'react';
import Link from 'next/link';
import { CreateVendorForm } from '@/components/features/vendors/CreateVendorForm';

export default function NewVendorPage() {
  return (
    <div className="space-y-6">
      <div className="sm:flex sm:items-center">
        <div className="sm:flex-auto">
          <h1 className="text-2xl font-bold text-gray-900">Add New Vendor</h1>
          <p className="mt-2 text-sm text-gray-700">
            Register a new vendor on the platform with their skills and contact details.
          </p>
        </div>
        <div className="mt-4 sm:mt-0">
          <Link href="/admin/vendors" className="text-sm text-blue-600 hover:text-blue-800">
            &larr; Back to Vendors
          </Link>
        </div>
      </div>

      <CreateVendorForm />
    </div>
  );
}
