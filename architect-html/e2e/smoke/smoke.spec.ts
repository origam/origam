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

// These assertions only touch the application shell, which renders even when
// Origam.Architect.Server is not running (translations fall back to their
// English defaults). They are a sanity check that the app boots and a working
// example you can clone for real, backend-backed flows.
test.describe('Architect shell', () => {
  test('loads and shows the application title', async ({ page }) => {
    await page.goto('/');
    await expect(page).toHaveTitle('Origam Architect');
    await expect(page.locator('#root')).not.toBeEmpty();
  });

  test('renders the sidebar tabs', async ({ page }) => {
    await page.goto('/');
    for (const label of ['Packages', 'Model', 'Properties', 'Output']) {
      await expect(page.getByText(label, { exact: true }).first()).toBeVisible();
    }
  });
});
