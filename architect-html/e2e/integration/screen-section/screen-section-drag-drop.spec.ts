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
import { resetBackend } from '../support/resetBackend';

test.describe('Screen Section drag and drop (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('drag a field onto the design panel', async ({ page }) => {
    await page.goto('/');

    await page.getByText('Root Menu').click();
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

    const field = page.getByText('FileName', { exact: true });
    const designSurface = page.getByTestId('design-surface');

    await field.dragTo(designSurface);

    await expect(designSurface.getByText('File Name')).toBeVisible({ timeout: 15_000 });
  });
});
