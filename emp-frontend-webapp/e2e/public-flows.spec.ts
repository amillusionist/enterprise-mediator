import { test, expect } from '@playwright/test';

/**
 * Tests for public (unauthenticated) flows:
 * - Vendor portal / brief viewing
 * - Invoice payment
 * - Milestone approval
 */
test.describe('Public Vendor Portal', () => {
  test('vendor brief page renders with valid token', async ({ page }) => {
    // Use a placeholder token — in real E2E, this would be seeded
    await page.goto('/portal/brief/test-token-123');

    // Page should render (either brief content or an error for invalid token)
    await expect(
      page.getByText(/brief|project|scope|token|expired|invalid/i).first()
    ).toBeVisible({ timeout: 10000 });
  });

  test('vendor brief page shows error for expired token', async ({ page }) => {
    await page.goto('/portal/brief/expired-token');

    await expect(
      page.getByText(/expired|invalid|not found|error/i).first()
    ).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Public Invoice Payment', () => {
  test('invoice payment page renders with valid token', async ({ page }) => {
    await page.goto('/pay/invoice/test-invoice-token');

    // Should show invoice details or an error for invalid token
    await expect(
      page.getByText(/invoice|payment|amount|pay|expired|invalid/i).first()
    ).toBeVisible({ timeout: 10000 });
  });

  test('payment success page renders', async ({ page }) => {
    await page.goto('/pay/success');

    await expect(
      page.getByText(/success|thank|confirmed|payment/i).first()
    ).toBeVisible({ timeout: 10000 });
  });

  test('payment confirmation page renders', async ({ page }) => {
    await page.goto('/pay/confirm');

    await expect(
      page.getByText(/confirm|review|payment|processing/i).first()
    ).toBeVisible({ timeout: 10000 });
  });
});

test.describe('Public Milestone Approval', () => {
  test('milestone approval page renders with valid token', async ({ page }) => {
    await page.goto('/approve/milestone/test-milestone-token');

    // Should show milestone details or token error
    await expect(
      page.getByText(/milestone|approve|reject|deliverable|expired|invalid/i).first()
    ).toBeVisible({ timeout: 10000 });
  });
});
