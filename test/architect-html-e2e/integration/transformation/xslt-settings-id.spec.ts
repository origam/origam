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
import { closeAiPanel } from '@support/aiPanel';
import { resetBackend } from '@support/resetBackend';

const cases = [
  {
    caption: 'XSL Transformation',
    treePath: ['Transformations'],
    itemName: 'SD_Empty',
    itemId: '693db72d-a400-4df3-b4b7-e0487e7ce8d6',
  },
  {
    caption: 'XSL Rule',
    treePath: ['Rules', '_common'],
    itemName: 'ER_ExceptionTemplate',
    itemId: 'e1c65fcd-118d-4eb3-9c2f-aa27fec132ba',
  },
  {
    caption: 'End Rule',
    treePath: ['Rules', 'Work Queue'],
    itemName: 'ER_WQ_Unlock',
    itemId: 'cb164d4d-d971-4fa7-be5b-80f4348a2f3f',
  },
  {
    caption: 'Complex Data Rule',
    treePath: ['Rules', 'Work Queue'],
    itemName: 'WorkQueue_FillRetryValues',
    itemId: 'dcc3e0e1-a1bd-4525-a9cf-1372a3f9fb8e',
  },
];

test.describe('Id in the XSLT settings (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  for (const { caption, treePath, itemName, itemId } of cases) {
    test(`Settings of ${caption} show its id read-only and copy it`, async ({ page }) => {
      await page.goto('/');
      await closeAiPanel(page);
      await page.getByTestId('tree-toggle-Business Logic').click();
      for (const folder of treePath) {
        await page.getByTestId(`tree-toggle-${folder}`).click();
      }
      await page.getByTestId(`tree-node-${itemName}`).dblclick();
      await expect(page.getByTestId(`tab-${itemName}`)).toBeVisible();

      await page
        .getByText('Settings', { exact: true })
        .and(page.locator(':not([data-test-id="topbar-settings"] *)'))
        .click();

      const idInput = page.getByTestId('property-input-Id');
      await expect(idInput).toBeVisible();
      await expect(idInput).toHaveValue(itemId);
      await expect(idInput).toBeDisabled();

      await page.context().grantPermissions(['clipboard-read', 'clipboard-write']);
      await idInput.locator('xpath=..').getByTitle('Copy to clipboard').click();
      await expect.poll(() => page.evaluate(() => navigator.clipboard.readText())).toBe(itemId);
    });
  }
});
