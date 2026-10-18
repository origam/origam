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
import { readModelFile, reloadBackend, resetBackend } from '@support/resetBackend';

const ACTION_ID = 'ae37c20c-360e-42a8-ba9f-db3b90a99b28';
const TARGET_MENU_ID = '213e6abf-4123-42ce-bba0-ae63e5b4896c';
const ACTION_FILE = 'Widgets/DataEntity/Widgets/AllDataTypes.origam';
const ORIGINAL_PATH = /^Menu[\\/]_hidden[\\/]AllDataTypes_SingleRecord$/;
const TARGET_PATH = /^Menu[\\/]_hidden[\\/]AllDataTypes_Edit1$/;

test.describe('Menu reference properties (real backend)', () => {
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
  });

  test('renders the selected menu path of a menu action', async ({ page }) => {
    await openMenuAction(page);
    await expect(menuInput(page)).toHaveValue(ORIGINAL_PATH);
  });

  test('renders the selected menu path of a lookup menu binding', async ({ page }) => {
    await page.goto('/');
    for (const node of ['Data', 'Lookups', 'Widgets', 'UserName_Surname']) {
      await page.getByTestId(`tree-toggle-${node}`).click();
    }
    await page.getByTestId('tree-node-MenuBinding').dblclick();
    await expect(page.getByTestId('property-select-MenuItem').locator('input')).toHaveValue(
      /^Menu[\\/]TestMenuBinding[\\/]UserAccount$/,
    );
  });

  test('saves a selected menu and renders it after reloading the model', async ({ page, request }) => {
    await openMenuAction(page);
    await menuInput(page).fill('AllDataTypes_Edit1');
    const pendingUpdate = propertyUpdateResponse(page);
    await page.locator('body > ul > li').filter({ hasText: TARGET_PATH }).click();
    const response = await pendingUpdate;
    expect(response.ok(), await response.text()).toBeTruthy();
    await expect(menuInput(page)).toHaveValue(TARGET_PATH);

    await save(page);
    expect(savedMenuReference()).toContain(`/AllDataTypes_Edit1/${TARGET_MENU_ID}`);

    await reloadBackend(request, 'Widgets');
    await page.reload();
    await openMenuActionEditor(page);
    await expect(menuInput(page)).toHaveValue(TARGET_PATH);
  });

  test('saves a cleared menu and keeps it empty after reloading the model', async ({ page, request }) => {
    await openMenuAction(page);
    await expect(menuInput(page)).toHaveValue(ORIGINAL_PATH);
    await menuInput(page).fill('');
    const pendingUpdate = propertyUpdateResponse(page);
    await page.locator('body > ul > li').filter({ hasText: /^\(none\)$/ }).click();
    const response = await pendingUpdate;
    expect(response.ok(), await response.text()).toBeTruthy();
    await expect(menuInput(page)).toHaveValue('');

    await save(page);
    expect(savedMenuReference() ?? '').toBe('');

    await reloadBackend(request, 'Widgets');
    await page.reload();
    await openMenuActionEditor(page);
    await expect(menuInput(page)).toHaveValue('');
  });

  test('rejects an unknown menu path without clearing the current reference', async ({ page, request }) => {
    await openMenuAction(page);
    const originalReference = savedMenuReference();

    const response = await updateMenu(request, 'Menu/MissingMenuReference');
    expect(response.status(), await response.text()).toBe(420);
    expect(await response.text()).toContain('MissingMenuReference');

    await page.reload();
    await expect(menuInput(page)).toHaveValue(ORIGINAL_PATH);
    expect(savedMenuReference()).toBe(originalReference);
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();
  });

  test('continues to accept menu references submitted as GUIDs', async ({ page, request }) => {
    await openMenuAction(page);
    const response = await updateMenu(request, TARGET_MENU_ID);
    expect(response.ok(), await response.text()).toBeTruthy();

    await page.reload();
    await expect(menuInput(page)).toHaveValue(TARGET_PATH);
    await save(page);
    expect(savedMenuReference()).toContain(`/AllDataTypes_Edit1/${TARGET_MENU_ID}`);
  });

  for (const [format, value] of [
    ['path', 'Menu'],
    ['GUID', '147775f5-451d-4efd-8634-7f27a2cf50a6'],
  ]) {
    test(`rejects an incompatible reference submitted as a ${format}`, async ({ page, request }) => {
      await openMenuAction(page);
      const originalReference = savedMenuReference();

      const response = await updateMenu(request, value);
      expect(response.status(), await response.text()).toBe(420);
      expect(await response.text()).toContain('AbstractMenuItem');

      await page.reload();
      await expect(menuInput(page)).toHaveValue(ORIGINAL_PATH);
      expect(savedMenuReference()).toBe(originalReference);
      await expect(page.getByTestId('save-button-disabled')).toBeVisible();
    });
  }
});

async function openMenuAction(page: Page): Promise<void> {
  await page.goto('/');
  for (const node of ['Data', 'Entities', 'Widgets', 'AllDataTypes', 'UI Actions']) {
    await page.getByTestId(`tree-toggle-${node}`).click();
  }
  await openMenuActionEditor(page);
}

async function openMenuActionEditor(page: Page): Promise<void> {
  await page.getByTestId('tree-node-SingleRecord').dblclick();
  await expect(page.getByTestId('property-input-Name')).toHaveValue('SingleRecord');
}

function menuInput(page: Page) {
  return page.getByTestId('property-select-Menu').locator('input');
}

function propertyUpdateResponse(page: Page) {
  return page.waitForResponse(response => response.url().includes('/PropertyEditor/Update'));
}

function updateMenu(request: APIRequestContext, value: string) {
  return request.post('/PropertyEditor/Update', {
    data: { schemaItemId: ACTION_ID, changes: [{ name: 'Menu', value }] },
  });
}

async function save(page: Page): Promise<void> {
  await page.getByTestId('save-button').click();
  await expect(page.getByTestId('save-button-disabled')).toBeVisible();
}

function savedMenuReference(): string | undefined {
  const action = readModelFile(ACTION_FILE)
    .match(/<ema:EntityUIAction\b[^>]*>/g)
    ?.find(element => element.includes(`x:id="${ACTION_ID}"`));
  expect(action, 'the menu action must remain in the saved model').toBeDefined();
  return action?.match(/\bema:menu="([^"]*)"/)?.[1];
}
