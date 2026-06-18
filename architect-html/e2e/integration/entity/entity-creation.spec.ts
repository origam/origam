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

test.describe('Database Entity creation (real backend)', () => {
  test('Database Entity creation', async ({ page }) => {
    await page.goto('/');

    await expect(page.getByRole('img', { name: 'ORIGAM' })).toBeVisible();

    await page.getByText('Root Menu').click();
    await expect(page.getByRole('textbox', { name: 'Search' })).toBeVisible();

    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-node-Work Queue').click({
      button: 'right',
    });
    await expect(page.getByRole('menuitem', { name: 'New Database Entity Virtual' })).toBeVisible();

    await page.getByText('New').click();
    await page.getByText('Database Entity').click();
    await expect(page.getByRole('button', { name: 'Info' })).toBeVisible();

    await page
      .locator('div')
      .filter({ hasText: /^Save$/ })
      .nth(2)
      .click();
    await page.getByRole('img').nth(5).click();
    await page.getByTestId('tree-toggle-Work Queue').click();
    await expect(page.getByTestId('tree-node-NewTable').first()).toBeVisible();
  });
});
