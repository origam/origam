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
import { activatePackage } from '@support/activatePackage';
import { openConstants } from '@support/modelTree';
import { resetBackend } from '@support/resetBackend';

const PACKAGE = 'Root';
const CONSTANT_GROUP = 'Compression Algorithm Constants';
const UNGROUPED_CONSTANT = 'DefaultMailWorkQueueName';
const GROUPED_CONSTANT = 'CompressionAlgorithm_zip';

async function renameInEditorAndSave(page: Page, oldName: string, newName: string): Promise<void> {
  await page.getByRole('textbox', { name: oldName, exact: true }).fill(newName);
  await page.getByTestId('save-button').click();
  await expect(page.getByTestId('save-button-disabled')).toBeVisible();
}

async function expectTreeRenamed(page: Page, oldName: string, newName: string): Promise<void> {
  await expect(page.getByTestId(`tree-node-${newName}`)).toBeVisible();
  await expect(page.getByTestId(`tree-node-${oldName}`)).toHaveCount(0);
}

test.describe('Model tree rename from the property editor (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, PACKAGE);
  });

  test('saving a renamed item updates its tree node', async ({ page }) => {
    await openConstants(page);
    await page.getByTestId(`tree-node-${UNGROUPED_CONSTANT}`).dblclick();

    await renameInEditorAndSave(page, UNGROUPED_CONSTANT, 'RenamedConstant');

    await expectTreeRenamed(page, UNGROUPED_CONSTANT, 'RenamedConstant');
  });

  test('saving a renamed item inside a folder updates its tree node', async ({ page }) => {
    await openConstants(page);
    await page.getByTestId(`tree-toggle-${CONSTANT_GROUP}`).click();
    await page.getByTestId(`tree-node-${GROUPED_CONSTANT}`).dblclick();

    await renameInEditorAndSave(page, GROUPED_CONSTANT, 'RenamedGroupedConstant');

    await expectTreeRenamed(page, GROUPED_CONSTANT, 'RenamedGroupedConstant');
  });

  test('saving a second rename in the same tab updates its tree node again', async ({ page }) => {
    await openConstants(page);
    await page.getByTestId(`tree-node-${UNGROUPED_CONSTANT}`).dblclick();

    await renameInEditorAndSave(page, UNGROUPED_CONSTANT, 'RenamedConstant');
    await renameInEditorAndSave(page, 'RenamedConstant', 'RenamedConstantAgain');

    await expectTreeRenamed(page, 'RenamedConstant', 'RenamedConstantAgain');
  });

  test('saving a renamed item in a tab restored after reload updates its tree node', async ({
    page,
  }) => {
    await openConstants(page);
    await page.getByTestId(`tree-node-${UNGROUPED_CONSTANT}`).dblclick();
    await expect(
      page.getByRole('textbox', { name: UNGROUPED_CONSTANT, exact: true }),
    ).toBeVisible();

    await page.reload();
    await expect(page.getByTestId(`tree-node-${UNGROUPED_CONSTANT}`)).toBeVisible();

    await renameInEditorAndSave(page, UNGROUPED_CONSTANT, 'RenamedConstant');

    await expectTreeRenamed(page, UNGROUPED_CONSTANT, 'RenamedConstant');
  });
});
