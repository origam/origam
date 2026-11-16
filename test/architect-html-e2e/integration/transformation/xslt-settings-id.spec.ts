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
import { closeAiPanel } from '@support/aiPanel';
import { resetBackend } from '@support/resetBackend';

const TRANSFORMATION = 'SD_Empty';
const TRANSFORMATION_ID = '693db72d-a400-4df3-b4b7-e0487e7ce8d6';

test.describe('Id in the XSLT settings (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Settings show the transformation id read-only and copy it', async ({ page }) => {
    await page.goto('/');
    await closeAiPanel(page);
    await page.getByTestId('tree-toggle-Business Logic').click();
    await page.getByTestId('tree-toggle-Transformations').click();
    await page.getByTestId(`tree-node-${TRANSFORMATION}`).dblclick();
    await expect(page.getByTestId(`tab-${TRANSFORMATION}`)).toBeVisible();

    await page
      .getByText('Settings', { exact: true })
      .and(page.locator(':not([data-test-id="topbar-settings"] *)'))
      .click();

    const idInput = page.getByTestId('property-input-Id');
    await expect(idInput).toBeVisible();
    await expect(idInput).toHaveValue(TRANSFORMATION_ID);
    await expect(idInput).toBeDisabled();

    await page.context().grantPermissions(['clipboard-read', 'clipboard-write']);
    await idInput.locator('xpath=..').getByTitle('Copy to clipboard').click();
    await expect
      .poll(() => page.evaluate(() => navigator.clipboard.readText()))
      .toBe(TRANSFORMATION_ID);
  });
});
