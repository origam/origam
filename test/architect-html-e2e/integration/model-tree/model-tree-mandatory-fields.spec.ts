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

const BOLD = '700';
const REGULAR = '400';

async function openMatrixFields(page: Page): Promise<void> {
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId('tree-toggle-Data').click();
  await page.getByTestId('tree-toggle-Entities').click();
  await page.getByTestId('tree-toggle-Dimensions').click();
  await page.getByTestId('tree-toggle-DimensionTransformationMatrix').click();
  await page.getByTestId('tree-toggle-Fields').click();
}

function propertyCheckbox(page: Page, propertyName: string): Locator {
  return page
    .getByText(propertyName, { exact: true })
    .locator('xpath=ancestor::div[2]')
    .getByRole('checkbox');
}

test.describe('Mandatory entity fields in the model tree (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('fields that do not allow nulls are bold', async ({ page }) => {
    await openMatrixFields(page);

    await expect(page.getByTestId('tree-node-refTargetDimensionEntityId')).toHaveCSS(
      'font-weight',
      BOLD,
    );
    await expect(page.getByTestId('tree-node-refSource1DimensionEntityId')).toHaveCSS(
      'font-weight',
      REGULAR,
    );
  });

  test('saving AllowNulls updates the node weight', async ({ page }) => {
    await openMatrixFields(page);
    const fieldNode = page.getByTestId('tree-node-refSource1DimensionEntityId');
    await expect(fieldNode).toHaveCSS('font-weight', REGULAR);

    await fieldNode.dblclick();
    await expect(page.getByTestId('tab-refSource1DimensionEntityId')).toBeVisible();
    const allowNulls = propertyCheckbox(page, 'AllowNulls');
    await expect(allowNulls).toBeChecked();
    await allowNulls.uncheck();

    await page.getByTestId('save-button').click();
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();

    await expect(fieldNode).toHaveCSS('font-weight', BOLD);
  });
});
