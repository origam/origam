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
import { menuItem, openContextMenu } from '@support/modelTree';
import { resetBackend } from '@support/resetBackend';

async function expandAncestors(page: Page): Promise<void> {
  await page.goto('/');
  await page.getByTestId('tree-toggle-Data').click();
  await page.getByTestId('tree-toggle-Entities').click();
  await page.getByTestId('tree-toggle-Dimensions').click();
  await page.getByTestId('tree-toggle-Dimension1').click();
  await page.getByTestId('tree-toggle-_Ancestors').click();
}

async function expectNewDisabled(page: Page, nodeText: string): Promise<void> {
  await openContextMenu(page, nodeText);
  await expect(menuItem(page, nodeText, 'tree-menu-new')).toHaveAttribute('aria-disabled', 'true');
}

test.describe('Context menu of inherited nodes (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('the _Ancestors folder offers nothing new', async ({ page }) => {
    await expandAncestors(page);

    await expectNewDisabled(page, '_Ancestors');
  });

  test('an ancestor offers nothing new', async ({ page }) => {
    await expandAncestors(page);

    await expectNewDisabled(page, 'IOrigamEntity2');
  });

  test('a folder of an ancestor offers nothing new', async ({ page }) => {
    await expandAncestors(page);
    await page.getByTestId('tree-toggle-IOrigamEntity2').click();

    await expectNewDisabled(page, 'Fields');
  });
});
