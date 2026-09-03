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
import { resetBackend } from '@support/resetBackend';

test.describe.configure({ mode: 'serial' });

const TRANSFORMATION = 'SD_Empty';
const FIRST_LINE = '<?xml version="1.0" encoding="UTF-8"?>';

async function enableVimMode(page: Page) {
  await page.getByTestId('topbar-settings').click();
  const vimCheckbox = page.getByRole('checkbox', { name: 'Vim mode' });
  if (!(await vimCheckbox.isChecked())) {
    await vimCheckbox.check();
  }
  await page.getByRole('button', { name: 'Close' }).click();
}

function editorCodeArea(page: Page) {
  return page.getByTestId('code-editor').first().locator('.view-lines');
}

async function openTransformation(page: Page) {
  await page.getByTestId('tree-toggle-Business Logic').click();
  await page.getByTestId('tree-toggle-Transformations').click();
  await page.getByTestId(`tree-node-${TRANSFORMATION}`).dblclick();
  await expect(editorCodeArea(page)).toContainText(FIRST_LINE, { timeout: 30_000 });
}

async function closeWithoutSaving(page: Page) {
  await page.getByTestId(`tab-close-${TRANSFORMATION}`).click();
  const saveDialog = page.getByRole('button', { name: 'No' });
  if (await saveDialog.isVisible()) {
    await saveDialog.click();
  }
}

test.describe('Vim mode in the transformation editor (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Root');
  });

  test('vim key bindings drive the XSLT editor', async ({ page }) => {
    await page.goto('/');
    await enableVimMode(page);
    await openTransformation(page);

    const statusBar = page.getByTestId('vim-status-bar').first();
    const codeArea = editorCodeArea(page);

    await expect(statusBar).toContainText('--NORMAL--');

    await codeArea.click();
    await page.keyboard.press('Escape');
    await page.keyboard.press('g');
    await page.keyboard.press('g');

    await page.keyboard.press('i');
    await expect(statusBar).toContainText('--INSERT--');

    await page.keyboard.type('vimtest', { delay: 100 });
    await expect(codeArea).toContainText('vimtest');

    await page.keyboard.press('Escape');
    await expect(statusBar).toContainText('--NORMAL--');

    await page.keyboard.press('u');
    await expect(codeArea).not.toContainText('vimtest');
    await expect(codeArea).toContainText(FIRST_LINE);

    await closeWithoutSaving(page);
  });

  test('vim status bar stays visible when vim is enabled on an open editor', async ({ page }) => {
    await page.goto('/');
    await openTransformation(page);
    await enableVimMode(page);

    const statusBar = page.getByTestId('vim-status-bar').first();
    await editorCodeArea(page).click();
    await expect(statusBar).toContainText('--NORMAL--');

    const statusBox = await statusBar.boundingBox();
    const tabStripBox = await page.getByText('Source XML', { exact: true }).boundingBox();
    expect(statusBox!.y + statusBox!.height).toBeLessThanOrEqual(tabStripBox!.y);

    await closeWithoutSaving(page);
  });
});
