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

test.describe('Create Localization Child Entity wizard (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Create Localization Child Entity from Dimension2', async ({ page }) => {
    await page.goto('/');

    await page.getByText('Root Menu').click();
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId('tree-node-Dimension2').click({ button: 'right' });
    await page.getByText('Actions', { exact: true }).click();
    await page.getByText('Create Localization Child Entity').click();

    const dialog = page.getByRole('dialog');
    await expect(dialog).toContainText('Create Localization Child Entity');
    await expect(dialog).toContainText('Dimension2_l10n');

    await page.getByRole('button', { name: 'Next →' }).click();
    await expect(dialog).toContainText('Language Translation Entity');

    await page.getByRole('button', { name: 'Create Entity' }).click();

    await page.getByRole('button', { name: 'Show result' }).click();
    await expect(page.locator('tbody')).toContainText('Dimension2_l10n');
  });
});
