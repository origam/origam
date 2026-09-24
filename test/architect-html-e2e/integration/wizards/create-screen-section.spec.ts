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

test.describe('Create Screen Section wizard (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Create Screen Section from Dimension1 with a custom name', async ({ page }) => {
    await page.goto('/');

    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId('tree-node-Dimension1').click({ button: 'right' });
    await page.getByText('Actions', { exact: true }).click();
    await page.getByText('Create Screen Section').click();

    const dialog = page.getByRole('dialog');
    await expect(dialog).toContainText('Create Screen Section');
    await expect(dialog).toContainText('from Dimension1');

    const nameInput = dialog.getByRole('textbox').first();
    await expect(nameInput).toHaveValue(/.+/);
    await nameInput.fill('Dimension1SectionE2E');

    await page.getByRole('button', { name: 'Next →' }).click();

    await page.getByRole('button', { name: 'Select all' }).click();
    await page.getByRole('button', { name: 'Next →' }).click();

    await expect(dialog).toContainText('Dimension1SectionE2E');
    await expect(dialog).toContainText('Screen Section');

    await page.getByRole('button', { name: 'Create Screen Section' }).click();

    await page.getByRole('button', { name: 'Show result' }).click();
    await expect(page.locator('tbody')).toContainText('Dimension1SectionE2E');
    await expect(page.locator('tbody')).toContainText('Screen Section');
  });
});
