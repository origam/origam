/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { expect, test } from '@playwright/test';

// These tests hit the real Origam.Architect.Server. The packages below come
// from the model in model-tests/model, served by /Package/GetAll.
test.describe('Packages (real backend)', () => {
  test('loads the package list from the server', async ({ page }) => {
    // Assert the frontend actually called the backend and rendered the result.
    const packagesLoaded = page.waitForResponse(
      response => response.url().includes('/Package/GetAll') && response.ok(),
    );
    await page.goto('/');
    await packagesLoaded;

    await expect(page.getByText('Attachments', { exact: true })).toBeVisible();
    await expect(page.getByText('Audit', { exact: true })).toBeVisible();
  });

  test('clicking a package activates it on the backend', async ({ page }) => {
    await page.goto('/');
    const attachments = page.getByText('Attachments', { exact: true });
    await expect(attachments).toBeVisible();

    // Clicking a package posts its id to /Package/SetActive, which loads the
    // schema on the server (this is the path that requires the database).
    const setActive = page.waitForResponse(
      response =>
        response.url().includes('/Package/SetActive') &&
        response.request().method() === 'POST',
    );
    await attachments.click();

    expect((await setActive).ok()).toBeTruthy();
  });
});
