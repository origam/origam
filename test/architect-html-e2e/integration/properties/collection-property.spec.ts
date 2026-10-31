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

import { expect, test, type APIRequestContext, type Locator, type Page } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { closeAiPanel } from '@support/aiPanel';
import { collectServerErrors, expectNoServerErrors } from '@support/propertyEditor';
import { resetBackend } from '@support/resetBackend';

const MENU_ACTION = { id: 'ae37c20c-360e-42a8-ba9f-db3b90a99b28', name: 'SingleRecord' };
const WORKFLOW_ACTION = { id: '9e3f4ff0-ec2b-4847-8ce1-99b6bec05190', name: 'Identity_DeleteUser' };
const ACTION_WITHOUT_MAPPINGS = {
  id: 'a901c648-de34-4a81-8bef-016bbe155ec5',
  name: 'LoopTest_True',
};
const DATA_STRUCTURE_RULE = {
  id: '3b7170e2-fae7-4fb1-8f47-8573e1582de1',
  name: 'LabelToModifiedString',
};
const WORK_QUEUE_COMMAND = {
  id: '780a1b21-3043-4fe6-a9a7-8c0648a56ebc',
  name: 'Execute_WorkQueueTest',
};

test.describe('Collection properties (real backend)', () => {
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  let serverErrors: string[];

  test.beforeEach(async ({ page, request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
    serverErrors = collectServerErrors(page);
  });

  test.afterEach(async ({ page }) => {
    await expectNoServerErrors(page, serverErrors);
  });

  test('shows a read-only collection placeholder instead of the items', async ({
    page,
    request,
  }) => {
    await openEditor(page, request, MENU_ACTION);

    const input = page.getByTestId('property-input-ParameterMappings');
    await expect(input).toHaveValue('(Collection)');
    await expect(input).toBeDisabled();
    await expect(page.getByTestId('property-collection-ParameterMappings')).toBeEnabled();
  });

  test('selects the first member and shows its key and value', async ({ page, request }) => {
    await openEditor(page, request, MENU_ACTION);
    const dialog = await openCollection(page, 'ParameterMappings');

    await expect(dialog.locator('[data-test-id^="collection-member-"]')).toHaveCount(1);
    await expect(member(dialog, 'AllDataTypes_parId')).toHaveAttribute('aria-selected', 'true');
    await expectReadOnlyValue(dialog, 'Key', 'AllDataTypes_parId');
    await expectReadOnlyValue(dialog, 'Value', 'Id');
  });

  test('shows the properties of the selected member', async ({ page, request }) => {
    await openEditor(page, request, WORKFLOW_ACTION);
    const dialog = await openCollection(page, 'ParameterMappings');
    await expect(dialog.locator('[data-test-id^="collection-member-"]')).toHaveCount(2);

    await member(dialog, 'BusinessPartnerId').click();
    await expect(member(dialog, 'BusinessPartnerId')).toHaveAttribute('aria-selected', 'true');
    await expect(member(dialog, 'UserName')).toHaveAttribute('aria-selected', 'false');
    await expectReadOnlyValue(dialog, 'Key', 'BusinessPartnerId');
    await expectReadOnlyValue(dialog, 'Value', 'Id');

    await member(dialog, 'UserName').click();
    await expect(member(dialog, 'UserName')).toHaveAttribute('aria-selected', 'true');
    await expectReadOnlyValue(dialog, 'Key', 'UserName');
    await expectReadOnlyValue(dialog, 'Value', 'UserName');
  });

  test('lists schema items by name with their read-only properties', async ({ page, request }) => {
    await openEditor(page, request, DATA_STRUCTURE_RULE);
    const dialog = await openCollection(page, 'RuleDependencies');

    await expect(dialog.locator('[data-test-id^="collection-member-"]')).toHaveCount(1);
    await expect(member(dialog, 'TagInputSource_Number')).toHaveAttribute('aria-selected', 'true');
    await expectReadOnlyValue(dialog, 'Name', 'TagInputSource_Number');
  });

  test('shows an empty collection without members', async ({ page, request }) => {
    await openEditor(page, request, ACTION_WITHOUT_MAPPINGS);
    await expect(page.getByTestId('property-input-ParameterMappings')).toHaveValue('(Collection)');

    const dialog = await openCollection(page, 'ParameterMappings');
    await expect(dialog.locator('[data-test-id^="collection-member-"]')).toHaveCount(0);
    await expect(dialog.locator('[data-test-id^="property-input-"]')).toHaveCount(0);
  });

  test('shows only the collection redeclared by a derived type', async ({ page, request }) => {
    await openEditor(page, request, WORK_QUEUE_COMMAND);
    await expect(page.getByTestId('property-input-ParameterMappings')).toHaveCount(1);

    const dialog = await openCollection(page, 'ParameterMappings');
    await expect(member(dialog, 'WQ_TestQueue1')).toHaveAttribute('aria-selected', 'true');
    await expectReadOnlyValue(dialog, 'Name', 'WQ_TestQueue1');
  });

  test('shows the members added after the editor was opened', async ({ page, request }) => {
    await openEditor(page, request, MENU_ACTION);
    await createParameterMapping(request, MENU_ACTION.id, 'NewMapping', 'Name');

    const dialog = await openCollection(page, 'ParameterMappings');
    await expect(dialog.locator('[data-test-id^="collection-member-"]')).toHaveCount(2);
    await member(dialog, 'NewMapping').click();
    await expectReadOnlyValue(dialog, 'Key', 'NewMapping');
    await expectReadOnlyValue(dialog, 'Value', 'Name');
  });

  test('closes without changing the model', async ({ page, request }) => {
    await openEditor(page, request, MENU_ACTION);
    const dialog = await openCollection(page, 'ParameterMappings');

    await page.getByTestId('collection-editor-close').click();
    await expect(dialog).toHaveCount(0);
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();
    await expect(page.getByTestId(`tab-dirty-${MENU_ACTION.name}`)).toHaveCount(0);
  });
});

async function openEditor(
  page: Page,
  request: APIRequestContext,
  item: { id: string; name: string },
): Promise<void> {
  const response = await request.post('/Tab/Open', { data: { schemaItemId: item.id } });
  expect(response.ok(), await response.text()).toBeTruthy();
  await page.goto('/');
  await closeAiPanel(page);
  await expect(page.getByTestId('property-input-Name')).toHaveValue(item.name);
}

async function createParameterMapping(
  request: APIRequestContext,
  actionId: string,
  name: string,
  field: string,
): Promise<void> {
  const response = await request.post('/Tab/CreateNode', {
    data: {
      nodeId: actionId,
      newTypeName: 'Parameter Mapping',
      changes: [
        { name: 'Name', value: name },
        { name: 'Field', value: field },
      ],
      persist: true,
    },
  });
  expect(response.ok(), await response.text()).toBe(true);
  expect((await response.json()).discarded).toBeFalsy();
}

async function openCollection(page: Page, propertyName: string): Promise<Locator> {
  await page.getByTestId(`property-collection-${propertyName}`).click();
  const dialog = page.getByTestId('collection-editor');
  await expect(dialog).toBeVisible();
  return dialog;
}

function member(dialog: Locator, name: string): Locator {
  return dialog.getByTestId(`collection-member-${name}`);
}

async function expectReadOnlyValue(dialog: Locator, propertyName: string, value: string) {
  const input = dialog.getByTestId(`property-input-${propertyName}`);
  await expect(input).toHaveValue(value);
  await expect(input).toBeDisabled();
}
