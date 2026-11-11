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
import { closeAiPanel } from '@support/aiPanel';
import { resetBackend } from '@support/resetBackend';

const ICON_SIZE = 16;

async function expectNodeIcon(page: Page, nodeText: string, iconUrl: string): Promise<void> {
  const icon = page.getByTestId(`tree-node-${nodeText}`).locator('img');
  await expect(icon).toHaveAttribute('src', iconUrl);
  await expect
    .poll(() => icon.evaluate((image: HTMLImageElement) => image.naturalWidth))
    .toBe(ICON_SIZE);
  await expect(icon).toHaveCSS('width', `${ICON_SIZE}px`);
  await expect(icon).toHaveCSS('height', `${ICON_SIZE}px`);
}

test.describe('Model tree node icons (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('nodes show the icon of their type', async ({ page }) => {
    await page.goto('/');
    await closeAiPanel(page);
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Constants').click();

    await expectNodeIcon(page, 'Data', '/Icons/icon_05_data.svg');
    await expectNodeIcon(page, 'Constants', '/Icons/icon_06_constants.svg');
    await expectNodeIcon(page, '_boolean', '/Icons/37_folder-3.svg');
    await expectNodeIcon(page, 'InitialUserCreated', '/Icons/icon_data-constant.svg');
  });
});
