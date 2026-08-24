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
import {
  expectDropTargets,
  expectModelFile,
  expectMoveRequest,
  expectMoveTargets,
  expectTreeSettled,
  menuItem,
  openConstants,
  openContextMenu,
} from '@support/modelTree';
import { resetBackend } from '@support/resetBackend';

const PACKAGE = 'AutomaticTests';
const CONSTANTS_DIR = 'AutomaticTests/DataConstant';
// A group of constants living in Root, which AutomaticTests references.
const FOREIGN_GROUP = 'Attachments';
const CONSTANT_GROUP = 'AutomaticTests';

test.describe('Model tree move to dialog (real backend)', () => {
  test.describe.configure({ mode: 'default', timeout: 45_000 });
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, PACKAGE);
  });

  test('Move to moves a constant into a group of another package', async ({ page }) => {
    await openConstants(page);
    await openMoveToDialog(page, 'UngroupedConstant');

    await selectOption(page, 'move-to-package', 'Root');
    await selectOption(page, 'move-to-target', FOREIGN_GROUP);
    await expectMoveRequest(page, () => page.getByTestId('move-to-button-move').click());

    await expect(page.getByTestId('move-to-dialog')).toHaveCount(0);
    await expectTreeSettled(page, 'UngroupedConstant');

    await expectModelFile('Root/DataConstant/Attachments/UngroupedConstant.origam', true);
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, false);
  });

  test('Move to copies a constant into a group of another package', async ({ page }) => {
    await openConstants(page);
    await openMoveToDialog(page, 'UngroupedConstant');

    await selectOption(page, 'move-to-package', 'Root');
    await selectOption(page, 'move-to-target', FOREIGN_GROUP);
    await expectMoveRequest(page, () => page.getByTestId('move-to-button-copy').click());

    await expectModelFile('Root/DataConstant/Attachments/Copy of UngroupedConstant.origam', true);
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, true);
  });

  test('Move to is disabled for a group', async ({ page }) => {
    await openConstants(page);

    await openContextMenu(page, 'AutomaticTestsGroup');
    await expect(menuItem(page, 'AutomaticTestsGroup', 'tree-menu-move-to')).toHaveAttribute(
      'aria-disabled',
      'true',
    );
  });

  test('An item of another package can only be copied', async ({ page }) => {
    await openConstants(page);
    await openMoveToDialog(page, 'InitialUserCreated');

    await selectOption(page, 'move-to-target', CONSTANT_GROUP);
    await expect(page.getByTestId('move-to-copy-only')).toBeVisible();
    await expect(page.getByTestId('move-to-button-move')).toBeDisabled();
    await expect(page.getByTestId('move-to-button-copy')).toBeEnabled();
  });

  test('The target list is filtered while typing', async ({ page }) => {
    await openConstants(page);
    await openMoveToDialog(page, 'UngroupedConstant');
    await selectOption(page, 'move-to-package', 'Root');

    const input = page.getByTestId('move-to-target').locator('input');
    await input.click();
    await input.fill(FOREIGN_GROUP);
    await expect(dropdownOptions(page)).toHaveCount(1);

    await input.fill('nothing matches this');
    await expect(page.locator('body > ul > li', { hasText: 'No matches' })).toBeVisible();
  });

  test('Move to leaves a pending cut alone', async ({ page }) => {
    await openConstants(page);

    const constant = page.getByTestId('tree-node-UngroupedConstant');
    await constant.click();
    await expectDropTargets(page, () => page.keyboard.press('Control+x'));
    await expect(constant).toHaveClass(/cutNode/);

    await openMoveToDialog(page, 'InitialUserCreated');
    await page.getByTestId('move-to-button-cancel').click();
    await expect(page.getByTestId('move-to-dialog')).toHaveCount(0);

    await expect(constant).toHaveClass(/cutNode/);
    await openContextMenu(page, 'AutomaticTests');
    await expect(menuItem(page, 'AutomaticTests', 'tree-menu-paste')).not.toHaveAttribute(
      'aria-disabled',
      'true',
    );
  });

  test('Escape closes the dialog', async ({ page }) => {
    await openConstants(page);
    await openMoveToDialog(page, 'UngroupedConstant');

    await page.keyboard.press('Escape');
    await expect(page.getByTestId('move-to-dialog')).toHaveCount(0);
  });
});

async function openMoveToDialog(page: Page, nodeText: string): Promise<void> {
  await openContextMenu(page, nodeText);
  await expectMoveTargets(page, () => menuItem(page, nodeText, 'tree-menu-move-to').click());
  await expect(page.getByTestId('move-to-dialog')).toBeVisible();
}

// The dropdown of a filterable select is rendered into a portal on the body.
function dropdownOptions(page: Page) {
  return page.locator('body > ul > li').filter({ hasText: /\S/ });
}

async function selectOption(page: Page, testId: string, optionText: string): Promise<void> {
  const input = page.getByTestId(testId).locator('input');
  await input.click();
  await input.fill(optionText);
  // Options of nested groups are indented, so the text is not an exact match.
  await dropdownOptions(page)
    .filter({ hasText: new RegExp(`^\\s*${optionText}\\s*$`) })
    .first()
    .click();
  await expect(input).toHaveValue(new RegExp(`^\\s*${optionText}\\s*$`));
}
