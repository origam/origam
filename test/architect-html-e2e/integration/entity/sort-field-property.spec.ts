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

import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { closeAiPanel } from '@support/aiPanel';
import { readModelFile, reloadBackend, resetBackend } from '@support/resetBackend';

const SORT_SET_ID = 'a6af8408-6d99-45f8-9084-0599c1cd0518';
const SORT_FIELD_ID = '0b84d6c2-adc5-4f69-9990-e374d22b2db0';
const DATA_STRUCTURE_FILE = 'Widgets/DataStructure/Widgets/AllDataTypes.origam';

test.describe('Sort field properties (real backend)', () => {
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
  });

  for (const [source, fieldName] of [
    ['entity', 'Id'],
    ['data structure', 'TagInputSourceLabel_DataStructure'],
  ]) {
    test(`selects a column from the ${source} and preserves its name after saving and reloading`, async ({
      page,
      request,
    }) => {
      await openSortField(page, request, SORT_FIELD_ID);
      await expect(fieldInput(page)).toHaveValue('Date1');
      await expect(page.getByTestId('property-input-FieldName')).toHaveCount(0);
      await expect(page.getByTestId('property-open-FieldName')).toHaveCount(0);

      await pickOption(page, 'FieldName', fieldName);
      await save(page);
      expect(savedFieldName(SORT_FIELD_ID)).toBe(fieldName);

      await reloadBackend(request, 'Widgets');
      await openSortField(page, request, SORT_FIELD_ID);
      await expect(fieldInput(page)).toHaveValue(fieldName);
    });
  }

  test('offers column names after selecting the entity of a new sort field', async ({
    page,
    request,
  }) => {
    const response = await request.post('/Tab/CreateNode', {
      data: {
        nodeId: SORT_SET_ID,
        newTypeName: 'Origam.Schema.EntityModel.DataStructureSortSetItem',
        changes: [],
        persist: false,
      },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
    const created = await response.json();
    expect(created.discarded).not.toBe(true);
    const sortFieldId: string = created.node.origamId;

    await page.goto('/');
    await closeAiPanel(page);
    await expect(page.getByTestId('property-input-FieldName')).toBeVisible();
    await expect(fieldInput(page)).toHaveCount(0);

    await pickOption(page, 'Entity', 'AllDataTypes');
    await expect(page.getByTestId('property-input-FieldName')).toHaveCount(0);
    await expect(fieldInput(page)).toBeVisible();
    await pickOption(page, 'FieldName', 'Id');
    await save(page);
    expect(savedFieldName(sortFieldId)).toBe('Id');

    await reloadBackend(request, 'Widgets');
    await openSortField(page, request, sortFieldId);
    await expect(fieldInput(page)).toHaveValue('Id');
    await expect(page.getByTestId('property-select-Entity').locator('input')).toHaveValue(
      'AllDataTypes',
    );
  });
});

async function openSortField(page: Page, request: APIRequestContext, id: string): Promise<void> {
  const response = await request.post('/Tab/Open', { data: { schemaItemId: id } });
  expect(response.ok(), await response.text()).toBeTruthy();
  await page.goto('/');
  await closeAiPanel(page);
  await expect(fieldInput(page)).toBeVisible();
}

function fieldInput(page: Page) {
  return page.getByTestId('property-select-FieldName').locator('input');
}

async function pickOption(page: Page, property: string, name: string): Promise<void> {
  const input = page.getByTestId(`property-select-${property}`).locator('input');
  await input.fill(name);
  const pendingUpdate = page.waitForResponse(response =>
    response.url().includes('/PropertyEditor/Update'),
  );
  await page
    .locator('body > ul > li')
    .filter({ hasText: new RegExp(`^${name}$`) })
    .click();
  const response = await pendingUpdate;
  expect(response.ok(), await response.text()).toBeTruthy();
  await expect(input).toHaveValue(name);
}

async function save(page: Page): Promise<void> {
  await page.getByTestId('save-button').click();
  await expect(page.getByTestId('save-button-disabled')).toBeVisible();
}

function savedFieldName(id: string): string | undefined {
  const sortField = readModelFile(DATA_STRUCTURE_FILE)
    .match(/<dsssi:DataStructureSortSetItem\b[^>]*>/g)
    ?.find(element => element.includes(`x:id="${id}"`));
  expect(sortField, 'the sort field must exist in the saved model').toBeDefined();
  return sortField?.match(/\bdsssi:fieldName="([^"]*)"/)?.[1];
}
