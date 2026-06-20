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
import { resetBackend } from '../support/resetBackend';

test.describe('Database Entity creation (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Database Entity creation', async ({ page }) => {
      await page.goto('/');

      await page.getByText('Root Menu').click();
      await page.getByTestId('tree-toggle-Data').click();
      await page.getByTestId('tree-toggle-Data Structures').click();
      await page.getByTestId('tree-toggle-Dimensions').click();
      await page.getByTestId('tree-node-Dimensions').click({
          button: 'right'
      });
      await page.getByTestId('tree-menu-new').getByText('New').click();
      await page.getByTestId('tree-menu-new-Origam.Schema.EntityModel.DataStructure').getByText('Data Structure').click();
      await page.getByRole('textbox', { name: 'NewDataStructure' }).click();
      await page.getByRole('textbox', { name: 'NewDataStructure' }).fill('NewDataStructure44');
      await page.getByTestId('save-button').click();
      await expect(page.getByTestId('save-button-disabled')).toBeVisible();
      await page.getByTestId('tab-close-NewDataStructure44').getByRole('img').click();
      await page.getByTestId('tree-node-NewDataStructure44').dblclick();
      await expect(page.locator('#root')).toContainText('Data Structure: NewDataStructure44');
  });
});
