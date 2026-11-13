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
import { fillProperty } from '@support/propertyEditor';
import { readModelFile, resetBackend } from '@support/resetBackend';

const ENTITY_FILE = 'Root/DataEntity/Dimensions/DimensionTransformationMatrix.origam';
const FIELD = 'refSource1DimensionEntityId';
const OTHER_FIELD = 'refTargetDimensionEntityId';

async function openFields(page: Page): Promise<void> {
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId('tree-toggle-Data').click();
  await page.getByTestId('tree-toggle-Entities').click();
  await page.getByTestId('tree-toggle-Dimensions').click();
  await page.getByTestId('tree-toggle-DimensionTransformationMatrix').click();
  await page.getByTestId('tree-toggle-Fields').click();
}

async function openField(page: Page, fieldName: string): Promise<void> {
  await page.getByTestId(`tree-node-${fieldName}`).dblclick();
  await expect(page.getByTestId('property-input-Name').filter({ visible: true })).toHaveValue(
    fieldName,
  );
}

async function clearMandatoryProperty(page: Page): Promise<void> {
  await fillProperty(page, 'MappedColumnName', '');
  await expect(page.getByText('Errors:')).toBeVisible();
  await expect(page.getByTestId('save-button-disabled')).toBeVisible();
}

function saveQuestion(page: Page, label: string): Locator {
  return page.getByText(`Do you want to save ${label}?`, { exact: true });
}

test.describe('Closing an editor with unsaved changes (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('asks to save an editor that has errors', async ({ page }) => {
    const savedEntity = readModelFile(ENTITY_FILE);
    await openFields(page);
    await openField(page, FIELD);
    await clearMandatoryProperty(page);
    await expect(page.getByTestId(`tab-dirty-${FIELD}`)).toBeVisible();

    await page.getByTestId(`tab-close-${FIELD}`).click();
    await expect(saveQuestion(page, FIELD)).toBeVisible();
    await page.getByRole('button', { name: 'Cancel' }).click();
    await expect(page.getByTestId(`tab-${FIELD}`)).toBeVisible();

    await page.getByTestId(`tab-close-${FIELD}`).click();
    await expect(saveQuestion(page, FIELD)).toBeVisible();
    await page.getByRole('button', { name: 'No' }).click();
    await expect(page.getByTestId(`tab-${FIELD}`)).toHaveCount(0);
    expect(readModelFile(ENTITY_FILE)).toBe(savedEntity);
  });

  test('keeps an editor that has errors open when saving it fails', async ({ page }) => {
    const savedEntity = readModelFile(ENTITY_FILE);
    await openFields(page);
    await openField(page, FIELD);
    await clearMandatoryProperty(page);

    await page.getByTestId(`tab-close-${FIELD}`).click();
    await page.getByRole('button', { name: 'Yes' }).click();
    await expect(page.getByText('Fix the errors to save')).toBeVisible();
    await page.getByRole('button', { name: 'Ok', exact: true }).click();

    await expect(page.getByTestId(`tab-dirty-${FIELD}`)).toBeVisible();
    await expect(page.getByTestId('property-input-MappedColumnName')).toHaveValue('');
    expect(readModelFile(ENTITY_FILE)).toBe(savedEntity);
  });

  test('asks about the editor being closed, not the active one', async ({ page }) => {
    await openFields(page);
    await openField(page, FIELD);
    await fillProperty(page, 'Caption', 'Changed caption');
    await expect(page.getByTestId(`tab-dirty-${FIELD}`)).toBeVisible();
    await openField(page, OTHER_FIELD);

    await page.getByTestId(`tab-close-${FIELD}`).click();
    await expect(saveQuestion(page, FIELD)).toBeVisible();
    await page.getByRole('button', { name: 'No' }).click();

    await expect(page.getByTestId(`tab-${FIELD}`)).toHaveCount(0);
    await expect(page.getByTestId(`tab-${OTHER_FIELD}`)).toBeVisible();
  });
});
