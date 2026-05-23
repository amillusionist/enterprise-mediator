import { test as setup, expect } from '@playwright/test';

const ADMIN_EMAIL = process.env.E2E_ADMIN_EMAIL || 'admin@emp-test.com';
const ADMIN_PASSWORD = process.env.E2E_ADMIN_PASSWORD || 'TestPassword123!';

setup('authenticate as admin', async ({ page }) => {
  await page.goto('/login');

  await page.getByLabel(/email/i).fill(ADMIN_EMAIL);
  await page.getByLabel(/password/i).fill(ADMIN_PASSWORD);
  await page.getByRole('button', { name: /sign in|log in/i }).click();

  // Wait for redirect to dashboard
  await page.waitForURL('**/admin/dashboard**', { timeout: 15000 });
  await expect(page.getByRole('heading', { level: 1 })).toBeVisible();

  // Save authentication state for reuse across tests
  await page.context().storageState({ path: 'e2e/.auth/admin.json' });
});
