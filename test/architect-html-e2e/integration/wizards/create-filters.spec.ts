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

import { expect, test, type Page } from '@playwright/test';
import { resetBackend } from '@support/resetBackend';

async function createFilter(page: Page, menuItem: string, exact: boolean): Promise<void> {
  await page.getByTestId('tree-node-Name').click({ button: 'right' });
  await page.getByText('Actions', { exact: true }).click();
  await page.getByText(menuItem, { exact }).click();
  await page.getByRole('button', { name: 'Show result' }).click();
}

test.describe('Create Filter actions on entity field (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Create all filter types from a field context menu', async ({ page }) => {
    await page.goto('/');

    await page.getByText('Root Menu').dblclick();
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId('tree-toggle-DimensionEntity').click();
    await page.getByTestId('tree-toggle-Fields').click();

    await createFilter(page, 'Create (=) Filter', true);
    await expect(page.locator('tbody')).toContainText('DimensionEntity\\GetByName');
    await expect(page.locator('#root')).toContainText('Search results for "Filter (=)"');

    await createFilter(page, 'Create (=) Filter With', false);
    await expect(page.locator('tbody')).toContainText(
      'DimensionEntity\\GetByName\\Equal\\Right\\parName',
    );
    await expect(page.locator('tbody')).toContainText('DimensionEntity\\GetByName');
    await expect(page.locator('#root')).toContainText(
      'Search results for "Filter (=) with parameter"',
    );

    await createFilter(page, 'Create (Like) Filter', true);
    await expect(page.locator('tbody')).toContainText('DimensionEntity\\GetLikeName');
    await expect(page.locator('#root')).toContainText('Search results for "Filter (Like)"');

    await createFilter(page, 'Create (Like) Filter With', false);
    await expect(page.locator('tbody')).toContainText(
      'DimensionEntity\\GetLikeName\\Like\\Right\\parName',
    );
    await expect(page.locator('tbody')).toContainText('DimensionEntity\\GetLikeName');
    await expect(page.locator('#root')).toContainText(
      'Search results for "Filter (Like) with parameter"',
    );

    await createFilter(page, 'Create (List) Filter With', false);
    await expect(page.locator('tbody')).toContainText(
      'DimensionEntity\\GetByNameList\\In\\List\\parNameList',
    );
    await expect(page.locator('tbody')).toContainText('DimensionEntity\\GetByNameList');
    await expect(page.locator('#root')).toContainText(
      'Search results for "Filter (List) with parameter"',
    );

    await createFilter(page, 'Create (Between) Filter With', false);
    await expect(page.locator('tbody')).toContainText('DimensionEntity\\GetBetweenName');
    await page.getByRole('cell', { name: 'DimensionEntity\\parNameFrom' }).click();
    await expect(page.locator('tbody')).toContainText('DimensionEntity\\parNameFrom');
    await expect(page.locator('tbody')).toContainText('DimensionEntity\\parNameTo');
    await expect(page.locator('#root')).toContainText(
      'Search results for "Filter (Between) with parameters"',
    );
  });
});
