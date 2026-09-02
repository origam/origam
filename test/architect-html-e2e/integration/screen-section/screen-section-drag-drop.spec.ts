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

import { expect, Page, test } from '@playwright/test';
import { scanPropertyBindings } from '@support/modelAssertions';
import { resetBackend } from '@support/resetBackend';

test.describe('Screen Section drag and drop (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  async function openNewScreenSection(page: Page) {
    await page.goto('/');

    await page.getByTestId('tree-toggle-User Interface').click();
    await page.getByTestId('tree-toggle-Screen Sections').click();
    await page.getByTestId('tree-node-Screen Sections').click({ button: 'right' });
    await page.getByTestId('tree-menu-new').getByText('New').click();
    await page
      .getByTestId('tree-menu-new-Origam.Schema.GuiModel.PanelControlSet')
      .getByText('Screen Section')
      .click();
    await expect(page.locator('#root')).toContainText('Screen Section Editor: NewPanel');

    await page.getByRole('textbox').nth(1).click();
    await page.getByText('Attachment', { exact: true }).click();
  }

  test('drag a field onto the design panel', async ({ page }) => {
    await openNewScreenSection(page);

    const field = page.getByText('FileName', { exact: true });
    const designSurface = page.getByTestId('design-surface');

    await field.dragTo(designSurface);

    await expect(designSurface.getByText('File Name')).toBeVisible({ timeout: 15_000 });
  });

  test('drag a widget onto the design panel without selecting a field first', async ({ page }) => {
    await openNewScreenSection(page);

    const toolbox = page.getByTestId('toolbox');
    const designSurface = page.getByTestId('design-surface');
    const components = designSurface.getByTestId('design-component');

    const initialCount = await components.count();

    await toolbox.getByText('Widgets', { exact: true }).click();

    await toolbox.getByText('AsTextBox', { exact: true }).dragTo(designSurface);
    await expect(components).toHaveCount(initialCount + 1, { timeout: 15_000 });
    await expect(designSurface.getByText('AsTextBox', { exact: true })).toHaveCount(0);

    await toolbox.getByText('AsCheckBox', { exact: true }).dragTo(designSurface);
    await expect(components).toHaveCount(initialCount + 2, { timeout: 15_000 });
    await expect(designSurface.getByText('AsCheckBox', { exact: true })).toHaveCount(0);

    await toolbox.getByText('GroupBox', { exact: true }).dragTo(designSurface);
    await expect(components).toHaveCount(initialCount + 3, { timeout: 15_000 });
    await expect(designSurface.getByText('Group Box', { exact: true })).toHaveCount(0);

    const saveResponse = page.waitForResponse(
      response =>
        response.request().method() === 'POST' && response.url().includes('/SectionEditor/Save'),
      { timeout: 15_000 },
    );
    await page.getByTestId('save-button').click();
    expect((await saveResponse).status()).toBe(200);

    const bindingScan = scanPropertyBindings();
    expect(bindingScan.bindingsScanned).toBeGreaterThan(300);
    expect(bindingScan.offenders).toEqual([]);
  });

  test('drag a widget onto the design panel with a field selected', async ({ page }) => {
    await openNewScreenSection(page);

    const toolbox = page.getByTestId('toolbox');
    const designSurface = page.getByTestId('design-surface');

    await toolbox.getByText('FileName', { exact: true }).click();
    await toolbox.getByText('Widgets', { exact: true }).click();

    await toolbox.getByText('AsTextBox', { exact: true }).dragTo(designSurface);

    await expect(designSurface.getByText('File Name')).toBeVisible({ timeout: 15_000 });
  });
});
