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
import { resetBackend } from '@support/resetBackend';

// A schema item provider that offers "New Folder" (nodeLevelType === 'Provider').
const PROVIDER = 'Data Structures';

async function activateProvider(page: Page): Promise<void> {
  await page.goto('/');
  await page.getByTestId('tree-toggle-Data').click();
  await expect(page.getByTestId('tree-node-' + PROVIDER)).toBeVisible();
}

async function createFolder(page: Page, parentNode: string, name: string): Promise<void> {
  await page.getByTestId('tree-node-' + parentNode).click({ button: 'right' });
  await page.getByTestId('tree-menu-new-folder').click();
  await page.getByLabel('Folder name').fill(name);
  await page.getByRole('button', { name: 'OK' }).click();
  await expect(page.getByTestId('tree-node-' + name)).toBeVisible();
}

async function openRenameDialog(page: Page, folder: string): Promise<void> {
  await page.getByTestId('tree-node-' + folder).click({ button: 'right' });
  await page.getByTestId('tree-menu-rename-folder').click();
}

test.describe('Folder management (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('renames a folder', async ({ page }) => {
    await activateProvider(page);
    await createFolder(page, PROVIDER, 'E2EFolder');

    await openRenameDialog(page, 'E2EFolder');
    await page.getByLabel('Folder name').fill('E2EFolderRenamed');
    await page.getByRole('button', { name: 'OK' }).click();

    await expect(page.getByTestId('tree-node-E2EFolderRenamed')).toBeVisible();
    await expect(page.getByTestId('tree-node-E2EFolder')).toHaveCount(0);
  });

  test('deletes an empty folder after confirmation', async ({ page }) => {
    await activateProvider(page);
    await createFolder(page, PROVIDER, 'E2EEmpty');

    await page.getByTestId('tree-node-E2EEmpty').click({ button: 'right' });
    await page.getByTestId('tree-menu-delete').click();
    await page.getByRole('button', { name: 'Yes' }).click();

    await expect(page.getByTestId('tree-node-E2EEmpty')).toHaveCount(0);
  });

  test('keeps the folder when the confirmation is declined', async ({ page }) => {
    await activateProvider(page);
    await createFolder(page, PROVIDER, 'E2EKeep');

    await page.getByTestId('tree-node-E2EKeep').click({ button: 'right' });
    await page.getByTestId('tree-menu-delete').click();
    await page.getByRole('button', { name: 'No' }).click();

    await expect(page.getByTestId('tree-node-E2EKeep')).toBeVisible();
  });

  test('deletes a folder together with its contents and closes their tabs', async ({ page }) => {
    await activateProvider(page);
    await createFolder(page, PROVIDER, 'E2EWithItem');

    // Create a Data Structure inside the folder (mirrors entity-creation.spec).
    await page.getByTestId('tree-node-E2EWithItem').click({ button: 'right' });
    await page.getByTestId('tree-menu-new').getByText('New').click();
    await page
      .getByTestId('tree-menu-new-Origam.Schema.EntityModel.DataStructure')
      .getByText('Data Structure')
      .click();
    await page.getByRole('textbox', { name: 'NewDataStructure' }).fill('E2EInnerDs');
    await page.getByTestId('save-button').click();
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();
    await expect(page.getByTestId('tab-E2EInnerDs')).toBeVisible();
    await expect(page.getByTestId('tree-node-E2EInnerDs')).toBeVisible();

    // Deleting the folder must cascade to the item and close its open tab.
    await page.getByTestId('tree-node-E2EWithItem').click({ button: 'right' });
    await page.getByTestId('tree-menu-delete').click();
    await page.getByRole('button', { name: 'Yes' }).click();

    await expect(page.getByTestId('tree-node-E2EWithItem')).toHaveCount(0);
    await expect(page.getByTestId('tree-node-E2EInnerDs')).toHaveCount(0);
    await expect(page.getByTestId('tab-E2EInnerDs')).toHaveCount(0);
  });

  test('rejects a duplicate name on rename', async ({ page }) => {
    await activateProvider(page);
    await createFolder(page, PROVIDER, 'E2EAlpha');
    await createFolder(page, PROVIDER, 'E2EBeta');

    await openRenameDialog(page, 'E2EBeta');
    await page.getByLabel('Folder name').fill('E2EAlpha');

    await expect(page.getByText('A folder with this name already exists.')).toBeVisible();
    await expect(page.getByRole('button', { name: 'OK' })).toBeDisabled();
  });

  test('rejects invalid characters on rename', async ({ page }) => {
    await activateProvider(page);
    await createFolder(page, PROVIDER, 'E2EGamma');

    await openRenameDialog(page, 'E2EGamma');
    await page.getByLabel('Folder name').fill('bad/name');

    await expect(page.getByText('Folder name contains invalid characters.')).toBeVisible();
    await expect(page.getByRole('button', { name: 'OK' })).toBeDisabled();
  });

  // Regression: a group resolved by id is not wired to its provider, so touching ChildItems
  // (which TreeNode creation and the delete cascade both do) threw a NullReferenceException as
  // soon as the folder contained a subfolder.
  test('renames a folder that contains a subfolder', async ({ page }) => {
    await activateProvider(page);
    await createFolder(page, PROVIDER, 'E2EOuter');
    await createFolder(page, 'E2EOuter', 'E2EInner');

    await openRenameDialog(page, 'E2EOuter');
    await page.getByLabel('Folder name').fill('E2EOuterRenamed');
    await page.getByRole('button', { name: 'OK' }).click();

    await expect(page.getByTestId('tree-node-E2EOuterRenamed')).toBeVisible();
    await expect(page.getByTestId('tree-node-E2EOuter')).toHaveCount(0);
    await expect(page.getByTestId('tree-node-E2EInner')).toBeVisible();
  });

  test('deletes a folder that contains a subfolder', async ({ page }) => {
    await activateProvider(page);
    await createFolder(page, PROVIDER, 'E2EParent');
    await createFolder(page, 'E2EParent', 'E2EChild');

    await page.getByTestId('tree-node-E2EParent').click({ button: 'right' });
    await page.getByTestId('tree-menu-delete').click();
    await page.getByRole('button', { name: 'Yes' }).click();

    await expect(page.getByTestId('tree-node-E2EParent')).toHaveCount(0);
    await expect(page.getByTestId('tree-node-E2EChild')).toHaveCount(0);
  });
});
