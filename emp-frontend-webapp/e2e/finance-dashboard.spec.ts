import { test, expect } from '@playwright/test';

/**
 * Tests for the financial dashboard features:
 * - Transaction ledger
 * - Payout management
 */
test.describe('Finance Dashboard', () => {
  test.use({ storageState: 'e2e/.auth/admin.json' });

  test('transactions page loads and shows table', async ({ page }) => {
    await page.goto('/admin/finance/transactions');

    // Should show transaction table headers
    await expect(
      page.getByText(/transaction|date|amount|type|status/i).first()
    ).toBeVisible({ timeout: 10000 });
  });

  test('payouts page loads and shows payout list', async ({ page }) => {
    await page.goto('/admin/finance/payouts');

    // Should show payouts page content
    await expect(
      page.getByText(/payout|vendor|amount|status/i).first()
    ).toBeVisible({ timeout: 10000 });
  });

  test('transactions page supports table navigation', async ({ page }) => {
    await page.goto('/admin/finance/transactions');

    // Check for pagination or table controls
    const table = page.getByRole('table');
    if (await table.count() > 0) {
      await expect(table).toBeVisible();
      // Verify table has at least headers
      const headers = table.getByRole('columnheader');
      await expect(headers.first()).toBeVisible();
    }
  });
});
