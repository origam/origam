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

import { expect, test, type Locator, type Page } from '@playwright/test';
import { closeAiPanel } from '@support/aiPanel';
import { resetBackend } from '@support/resetBackend';

const ITEM = 'DimensionEntityRelation';
const ITEM_PACKAGE = 'Root';

function toolboxInput(page: Page, label: string): Locator {
  return page
    .getByTestId('toolbox')
    .getByText(label, { exact: true })
    .locator('xpath=following-sibling::input');
}

async function openDesigner(page: Page, groupName: string, header: string): Promise<void> {
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId('tree-toggle-User Interface').click();
  await page.getByTestId(`tree-toggle-${groupName}`).click();
  await page.getByTestId('tree-toggle-Dimensions').click();
  await page.getByTestId(`tree-node-${ITEM}`).dblclick();
  await expect(page.getByText(header, { exact: true })).toBeVisible();
}

test.describe('Package in the designer toolbox (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('the screen editor shows the package name', async ({ page }) => {
    await openDesigner(page, 'Screens', `Screen Editor: ${ITEM}`);

    await expect(toolboxInput(page, 'Package')).toHaveValue(ITEM_PACKAGE);
  });

  test('the screen section editor shows the package name', async ({ page }) => {
    await openDesigner(page, 'Screen Sections', `Screen Section Editor: ${ITEM}`);

    await expect(toolboxInput(page, 'Package')).toHaveValue(ITEM_PACKAGE);
  });
});
