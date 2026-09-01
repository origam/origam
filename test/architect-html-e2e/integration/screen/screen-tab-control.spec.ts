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

import { expect, test } from '@playwright/test';
import { resetBackend } from '@support/resetBackend';

test.describe('Screen editor widgets (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('drop a TabControl onto the screen design panel', async ({ page }) => {
    await page.goto('/');

    await page.getByTestId('tree-toggle-User Interface').click();
    await page.getByTestId('tree-toggle-Screens').click();
    await page.getByTestId('tree-node-Screens').click({ button: 'right' });
    await page.getByTestId('tree-menu-new').getByText('New').click();
    await page
      .getByTestId('tree-menu-new-Origam.Schema.GuiModel.FormControlSet')
      .getByText('Screen')
      .click();
    await expect(page.locator('#root')).toContainText('Screen Editor: NewForm');

    const toolbox = page.getByTestId('toolbox');
    const designSurface = page.getByTestId('design-surface');
    const components = designSurface.getByTestId('design-component');

    const initialCount = await components.count();

    await toolbox.getByText('Widgets', { exact: true }).click();
    await toolbox.getByText('TabControl', { exact: true }).dragTo(designSurface);

    await expect(components).toHaveCount(initialCount + 1, { timeout: 15_000 });
    await expect(designSurface.getByText(/TabPage/u)).toHaveCount(2, { timeout: 15_000 });
    await expect(page.locator('#root')).not.toContainText('prepareNewValue_');
  });
});
