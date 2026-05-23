import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test('login page renders correctly', async ({ page }) => {
    await page.goto('/login');

    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel(/password/i)).toBeVisible();
    await expect(page.getByRole('button', { name: /sign in|log in/i })).toBeVisible();
  });

  test('shows validation errors for empty form submission', async ({ page }) => {
    await page.goto('/login');

    await page.getByRole('button', { name: /sign in|log in/i }).click();

    // Expect validation errors to appear
    await expect(page.getByText(/required|invalid|enter/i).first()).toBeVisible();
  });

  test('shows error for invalid credentials', async ({ page }) => {
    await page.goto('/login');

    await page.getByLabel(/email/i).fill('invalid@example.com');
    await page.getByLabel(/password/i).fill('WrongPassword123!');
    await page.getByRole('button', { name: /sign in|log in/i }).click();

    // Expect an error message
    await expect(
      page.getByText(/invalid|incorrect|unauthorized|failed/i)
    ).toBeVisible({ timeout: 10000 });
  });

  test('register page renders correctly', async ({ page }) => {
    await page.goto('/register');

    await expect(page.getByLabel(/email/i)).toBeVisible();
    await expect(page.getByLabel(/password/i).first()).toBeVisible();
    await expect(page.getByRole('button', { name: /register|sign up|create/i })).toBeVisible();
  });

  test('unauthenticated user is redirected from dashboard', async ({ page }) => {
    await page.goto('/admin/dashboard');

    // Should redirect to login
    await page.waitForURL('**/login**', { timeout: 10000 });
    await expect(page.getByLabel(/email/i)).toBeVisible();
  });
});
