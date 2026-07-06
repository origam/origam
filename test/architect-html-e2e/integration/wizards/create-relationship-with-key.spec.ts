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

test.describe('Create Relationship With Key wizard (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Create Relationship With Key', async ({ page }) => {
    await page.goto('/');

    await page.getByText('Root Menu').click();
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId('tree-node-Dimension1').click({ button: 'right' });
    await page.getByText('Actions', { exact: true }).click();
    await page.getByText('Create Relationship With Key').click();

    const dialog = page.getByRole('dialog');

    await dialog.getByRole('checkbox', { name: 'Is Parent-Child' }).check();

    const relatedEntity = dialog.getByRole('textbox').nth(1);
    await relatedEntity.fill('AsapModelVersion');
    await relatedEntity.press('Enter');

    const baseField = dialog.getByRole('textbox').nth(3);
    await baseField.click();
    await baseField.press('Enter');

    const relatedField = dialog.getByRole('textbox').nth(4);
    await relatedField.click();
    await relatedField.press('Enter');

    await page.getByRole('button', { name: 'Next →' }).click();
    await page.getByRole('button', { name: 'Create Relationship' }).click();

    await page.getByRole('button', { name: 'Show result' }).click();
    await expect(page.getByRole('cell', { name: 'Dimension1\\AsapModelVersion' })).toBeVisible();
    await expect(page.getByRole('cell', { name: 'Dimension1', exact: true }).first()).toBeVisible();
  });
});
