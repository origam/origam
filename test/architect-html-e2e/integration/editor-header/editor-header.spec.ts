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
import { activatePackage } from '@support/activatePackage';
import { closeAiPanel } from '@support/aiPanel';
import { menuItem, openConstants, openContextMenu } from '@support/modelTree';
import { resetBackend } from '@support/resetBackend';

const DATA_CONSTANT_TYPE = 'Origam.Schema.EntityModel.DataConstant';
const NEW_CONSTANT = 'NewDataConstant';
const EXISTING_CONSTANT = 'DefaultMailWorkQueueName';
const SCREEN = 'DimensionEntityRelation';
const SQL_SCRIPT = '00040_Create_Test_User';

function header(page: Page, text: string): Locator {
  return page.getByText(text, { exact: true });
}

async function save(page: Page): Promise<void> {
  await page.getByTestId('save-button').filter({ visible: true }).click();
  await expect(page.getByTestId('save-button-disabled').filter({ visible: true })).toBeVisible();
}

async function renameInPropertyEditorAndSave(page: Page, newName: string): Promise<void> {
  await page.getByTestId('property-input-Name').fill(newName);
  await save(page);
}

async function expectHeaderChanged(page: Page, oldText: string, newText: string): Promise<void> {
  await expect(header(page, newText)).toBeVisible();
  await expect(header(page, oldText)).toHaveCount(0);
}

test.describe('Grid editor header (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Root');
  });

  test('saving a new item updates the editor header', async ({ page }) => {
    await openConstants(page);
    await openContextMenu(page, 'Constants');
    await menuItem(page, 'Constants', 'tree-menu-new').getByText('New').click();
    await menuItem(page, 'Constants', `tree-menu-new-${DATA_CONSTANT_TYPE}`).click();
    await expect(header(page, `Data Constant: ${NEW_CONSTANT}`)).toBeVisible();

    await renameInPropertyEditorAndSave(page, 'SavedConstant');

    await expectHeaderChanged(
      page,
      `Data Constant: ${NEW_CONSTANT}`,
      'Data Constant: SavedConstant',
    );
  });

  test('saving a renamed item updates the editor header', async ({ page }) => {
    await openConstants(page);
    await page.getByTestId(`tree-node-${EXISTING_CONSTANT}`).dblclick();
    await expect(header(page, `Data Constant: ${EXISTING_CONSTANT}`)).toBeVisible();

    await renameInPropertyEditorAndSave(page, 'RenamedConstant');

    await expectHeaderChanged(
      page,
      `Data Constant: ${EXISTING_CONSTANT}`,
      'Data Constant: RenamedConstant',
    );
  });

  test('saving a second rename in the same tab updates the editor header again', async ({
    page,
  }) => {
    await openConstants(page);
    await page.getByTestId(`tree-node-${EXISTING_CONSTANT}`).dblclick();

    await renameInPropertyEditorAndSave(page, 'RenamedConstant');
    await renameInPropertyEditorAndSave(page, 'RenamedConstantAgain');

    await expectHeaderChanged(
      page,
      'Data Constant: RenamedConstant',
      'Data Constant: RenamedConstantAgain',
    );
  });
});

test.describe('Screen editor header (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Root');
  });

  test('saving a renamed screen updates the editor header', async ({ page }) => {
    await page.goto('/');
    await closeAiPanel(page);
    await page.getByTestId('tree-toggle-User Interface').click();
    await page.getByTestId('tree-toggle-Screens').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId(`tree-node-${SCREEN}`).dblclick();
    await expect(header(page, `Screen Editor: ${SCREEN}`)).toBeVisible();

    const nameInput = page
      .getByTestId('toolbox')
      .getByText('Name', { exact: true })
      .locator('xpath=following-sibling::input');
    const pendingUpdate = page.waitForResponse(response =>
      response.url().includes('/ScreenEditor/Update'),
    );
    await nameInput.fill('RenamedScreen');
    await pendingUpdate;
    await save(page);

    await expectHeaderChanged(page, `Screen Editor: ${SCREEN}`, 'Screen Editor: RenamedScreen');
  });
});

test.describe('Deployment scripts editor header (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'AutomaticTests');
  });

  test('saving a renamed script updates the editor header', async ({ page }) => {
    await page.goto('/');
    await closeAiPanel(page);
    await page.getByTestId('tree-toggle-Common').click();
    await page.getByTestId('tree-toggle-Deployment').click();
    await page.getByTestId('tree-toggle-AutomaticTests').click();
    await page.getByTestId('tree-toggle-1.0.0').click();
    await page.getByTestId(`tree-node-${SQL_SCRIPT} (MsSql)`).dblclick();
    const settingsTab = page
      .getByText('SQL', { exact: true })
      .locator('xpath=following-sibling::div[1]');
    await settingsTab.click();
    await expect(header(page, `Settings: ${SQL_SCRIPT}`)).toBeVisible();

    await renameInPropertyEditorAndSave(page, 'RenamedScript');

    await expectHeaderChanged(page, `Settings: ${SQL_SCRIPT}`, 'Settings: RenamedScript');
  });
});
