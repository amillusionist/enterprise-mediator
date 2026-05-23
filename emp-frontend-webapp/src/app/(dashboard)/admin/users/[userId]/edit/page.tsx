import { notFound } from 'next/navigation';
import Link from 'next/link';
import { UserService } from '@/services/user.service';
import { UserEditForm } from './user-edit-form';
import type { User } from '@/lib/types';

interface UserEditPageProps {
  params: { userId: string };
}

export default async function UserEditPage({ params }: UserEditPageProps) {
  let user: User;

  try {
    user = await UserService.getUserById(params.userId);
  } catch {
    notFound();
  }

  return (
    <div className="space-y-6">
      <div>
        <Link
          href="/admin/users"
          className="text-sm text-blue-600 hover:text-blue-800"
        >
          &larr; Back to Users
        </Link>
        <h1 className="mt-2 text-2xl font-bold text-gray-900">
          Edit User: {user.name || user.email}
        </h1>
      </div>

      <div className="rounded-lg border bg-white p-6 shadow-sm">
        <UserEditForm user={user} />
      </div>
    </div>
  );
}
