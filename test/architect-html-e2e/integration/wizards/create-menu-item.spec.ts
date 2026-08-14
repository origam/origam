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

test.describe('Create Menu Item wizard (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Create Menu Item from a screen with a role', async ({ page }) => {
    await page.goto('/');

    await page.getByTestId('tree-toggle-User Interface').click();
    await page.getByTestId('tree-toggle-Screens').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId('tree-node-DimensionEntityRelation').click({ button: 'right' });
    await page.getByText('Actions', { exact: true }).click();
    await page.getByText('Create Menu Item').click();

    await page
      .getByRole('textbox', { name: 'e.g. DimensionEntityRelation' })
      .fill('Caption');
    await page.getByRole('textbox', { name: 'e.g. DimensionEntityRelation' }).press('Tab');

    await page.getByRole('button', { name: 'Next →' }).click();
    await expect(page.getByRole('dialog')).toContainText('MCaptionForm Reference Menu Item');
    await page
      .getByText('MCaptionForm Reference Menu ItemRoleDimensionEntityRelation')
      .click();

    await page.getByRole('button', { name: 'Create Menu Item' }).click();

    await page.getByRole('button', { name: 'Show result' }).click();
    await expect(page.locator('tbody')).toContainText('Menu\\DimensionEntityRelation');
    await expect(page.locator('#root')).toContainText('Search results for "Menu Item"');
  });
});
