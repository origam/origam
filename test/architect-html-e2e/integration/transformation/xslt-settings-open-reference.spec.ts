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
import { activatePackage } from '@support/activatePackage';
import { closeAiPanel } from '@support/aiPanel';
import { resetBackend } from '@support/resetBackend';

const TRANSFORMATION = 'SD_Empty';
const STRUCTURE = 'Dimension1';

test.describe('Open referenced item from the XSLT settings (real backend)', () => {
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Root');
  });

  test('Open button reveals the source structure in a tab and in the tree', async ({ page }) => {
    await page.goto('/');
    await closeAiPanel(page);
    await page.getByTestId('tree-toggle-Business Logic').click();
    await page.getByTestId('tree-toggle-Transformations').click();
    await page.getByTestId(`tree-node-${TRANSFORMATION}`).dblclick();
    await expect(page.getByTestId(`tab-${TRANSFORMATION}`)).toBeVisible();

    const settingsLoaded = page.waitForResponse(
      response => response.url().includes('/Xslt/Settings') && response.ok(),
    );
    await page.getByText('Settings', { exact: true }).click();
    await settingsLoaded;
    const openButton = page.getByTestId('xslt-settings-open-source-structure');
    await expect(openButton).toHaveCount(0);

    await page.getByTestId('xslt-settings-source-structure').selectOption({ label: STRUCTURE });
    await openButton.click();

    await expect(page.getByTestId(`tab-${STRUCTURE}`)).toHaveClass(/activeTab/);
    const targetNode = page.getByTestId(`tree-node-${STRUCTURE}`);
    await expect(targetNode.locator('xpath=../..')).toHaveClass(/highlighted/);
    await expect(targetNode).toBeInViewport();
  });
});
