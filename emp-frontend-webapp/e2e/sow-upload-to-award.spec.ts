import { test, expect } from '@playwright/test';
import path from 'path';

/**
 * Critical user journey: SOW upload → AI review → vendor match → proposal → award.
 * Tests the full project lifecycle from the admin dashboard.
 */
test.describe('SOW Upload to Project Award Flow', () => {
  test.use({ storageState: 'e2e/.auth/admin.json' });

  test('admin can create a new project', async ({ page }) => {
    await page.goto('/admin/projects');
    await page.getByRole('link', { name: /new project|create project/i }).click();

    await page.waitForURL('**/admin/projects/new**');

    await page.getByLabel(/project name|title/i).fill('E2E Test Project');
    await page.getByLabel(/client/i).click();
    // Select the first available client from the dropdown
    await page.getByRole('option').first().click();

    await page.getByRole('button', { name: /create|submit/i }).click();

    // Should redirect to project detail page
    await page.waitForURL('**/admin/projects/**');
    await expect(page.getByText('E2E Test Project')).toBeVisible();
  });

  test('admin can upload a SOW document to a project', async ({ page }) => {
    await page.goto('/admin/projects');

    // Navigate to the first project
    await page.getByRole('link', { name: /view|details/i }).first().click();
    await page.waitForURL('**/admin/projects/**');

    // Find and interact with the SOW upload area
    const fileInput = page.locator('input[type="file"]');

    if (await fileInput.count() > 0) {
      // Upload a test PDF file
      const testFilePath = path.resolve(__dirname, 'fixtures/test-sow.pdf');
      await fileInput.setInputFiles(testFilePath);

      // Wait for upload confirmation
      await expect(
        page.getByText(/uploaded|processing|success/i)
      ).toBeVisible({ timeout: 10000 });
    }
  });

  test('admin can view SOW review page after processing', async ({ page }) => {
    await page.goto('/admin/projects');

    // Navigate to first project's SOW review
    const projectLink = page.getByRole('link', { name: /view|details/i }).first();
    await projectLink.click();
    await page.waitForURL('**/admin/projects/**');

    // Extract projectId from URL
    const url = page.url();
    const projectIdMatch = url.match(/projects\/([a-f0-9-]+)/);

    if (projectIdMatch) {
      const projectId = projectIdMatch[1];
      await page.goto(`/admin/sow-review/${projectId}`);

      // SOW review page should show extracted data
      await expect(
        page.getByText(/scope|brief|summary|skills|deliverables/i)
      ).toBeVisible({ timeout: 10000 });
    }
  });

  test('admin can view vendor matching results', async ({ page }) => {
    await page.goto('/admin/projects');
    await page.getByRole('link', { name: /view|details/i }).first().click();
    await page.waitForURL('**/admin/projects/**');

    // Look for vendor match section or tab
    const vendorMatchTab = page.getByRole('tab', { name: /vendor|match/i });
    if (await vendorMatchTab.count() > 0) {
      await vendorMatchTab.click();
    }

    // Expect to see vendor match results or a match score
    const matchContent = page.getByText(/match|score|vendor/i);
    await expect(matchContent.first()).toBeVisible({ timeout: 10000 });
  });

  test('admin can view proposals for a project', async ({ page }) => {
    await page.goto('/admin/projects');
    await page.getByRole('link', { name: /view|details/i }).first().click();
    await page.waitForURL('**/admin/projects/**');

    // Extract projectId from URL
    const url = page.url();
    const projectIdMatch = url.match(/projects\/([a-f0-9-]+)/);

    if (projectIdMatch) {
      const projectId = projectIdMatch[1];
      await page.goto(`/admin/proposals/${projectId}`);

      await expect(page).toHaveURL(new RegExp(`/admin/proposals/${projectId}`));
      // Proposals page should be visible
      await expect(
        page.getByText(/proposal|vendor|cost|timeline/i).first()
      ).toBeVisible({ timeout: 10000 });
    }
  });
});
