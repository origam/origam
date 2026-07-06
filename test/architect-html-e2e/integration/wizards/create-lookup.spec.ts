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
import { resetBackend } from '@support/resetBackend';

test.describe('Create Lookup wizard (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Create Lookup with keyboard navigation', async ({ page }) => {
    await page.goto('/');

    await page.getByText('Root Menu').click();
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId('tree-node-Dimension1').click({ button: 'right' });
    await page.getByText('Actions', { exact: true }).click();
    await page.getByText('Create Lookup').click();

    const dialog = page.getByRole('dialog');

    const displayField = dialog.getByRole('textbox').nth(0);
    await displayField.fill('Na');
    await displayField.press('Tab');

    const listFilter = dialog.getByRole('textbox').nth(1);
    await listFilter.fill('Get');
    await listFilter.press('Tab');

    const idFilter = dialog.getByRole('textbox').nth(2);
    await idFilter.fill('geti');
    await idFilter.press('Tab');
    await expect(idFilter).toHaveValue('GetId');

    await page.getByRole('button', { name: 'Next →' }).click();
    await expect(page.getByRole('textbox', { name: 'e.g.' })).toHaveValue('Dimension1_Name_GetId');

    await page.getByRole('button', { name: 'Next →' }).click();
    await page.getByRole('button', { name: 'Create Lookup' }).click();

    await page.getByRole('button', { name: 'Show result' }).click();
    await expect(page.locator('tbody')).toContainText('Dimension1_Name_GetId');
    await expect(page.locator('tbody')).toContainText('LookupDimension1_Name_GetId');
    await expect(page.getByRole('cell', { name: 'Data Service Lookup' }).first()).toBeVisible();
    await expect(page.getByRole('cell', { name: 'Data Structure' }).first()).toBeVisible();
  });
});
