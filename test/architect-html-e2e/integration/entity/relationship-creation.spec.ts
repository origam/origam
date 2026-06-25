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

test.describe('Entity Relationship creation (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Entity Relationship creation with incremental search', async ({ page }) => {
    await page.goto('/');

    await page.getByText('Root Menu').click();
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId('tree-node-IDimension').click({ button: 'right' });
    await page.getByText('New').click();
    await page.getByText('Relationship').click();

    await page.getByRole('textbox').nth(2).click();
    await page.getByRole('textbox').nth(2).fill('Test');
    await page.getByRole('textbox').nth(2).press('Enter');
    await page.getByRole('textbox').nth(4).click();
    await page.getByRole('textbox').nth(4).fill('Att');
    await page.getByRole('textbox').nth(4).press('Tab');

    await expect(page.getByRole('textbox').nth(4)).toHaveValue('Attachment');

    await page.getByTestId('save-button').click();
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();

    await page.getByTestId('tree-toggle-DimensionEntity').click();
    await page.getByTestId('tree-toggle-Relationships').click();
    await page.getByTestId('tree-toggle-Dimension4').click();
    await page.getByTestId('tree-toggle-_Ancestors').first().click();
    await page.getByTestId('tree-node-SourceDimensionEntityRelation').dblclick();
    await expect(page.getByTestId('tab-SourceDimensionEntityRelation')).toBeVisible();
  });
});
