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
import { closeAiPanel } from '@support/aiPanel';
import { resetBackend } from '@support/resetBackend';

async function openMatrixFields(page: Page): Promise<void> {
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId('tree-toggle-Data').click();
  await page.getByTestId('tree-toggle-Entities').click();
  await page.getByTestId('tree-toggle-Dimensions').click();
  await page.getByTestId('tree-toggle-DimensionTransformationMatrix').click();
  await page.getByTestId('tree-toggle-Fields').click();
}

test.describe('Open referenced item from the property editor (real backend)', () => {
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('Open button reveals the referenced item in a tab and in the tree', async ({ page }) => {
    await openMatrixFields(page);
    await page.getByTestId('tree-node-refSource4DimensionEntityId').dblclick();
    await expect(page.getByTestId('tab-refSource4DimensionEntityId')).toBeVisible();

    await page.getByTestId('property-open-DefaultLookup').click();

    await expect(page.getByTestId('tab-SourceDimensionEntity_ByTarget')).toHaveClass(/activeTab/);
    await expect(page.getByTestId('tree-toggle-Lookups')).toHaveText('▼');
    const targetNode = page.getByTestId('tree-node-SourceDimensionEntity_ByTarget');
    await expect(targetNode.locator('xpath=../..')).toHaveClass(/highlighted/);
    await expect(targetNode).toBeInViewport();
  });
});
