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

test.describe('Database Entity creation (real backend)', () => {
  test('Database Entity creation', async ({ page }) => {
    await page.goto('/');

    await page.getByText('Root Menu').click();
    await page.getByText('▶').nth(1).click();
    await page.getByText('▶').nth(3).click();
    await page.getByText('Dimensions').click({
      button: 'right',
    });
    await page.getByText('Database Entity').click();
    await page.getByRole('textbox', { name: 'NewTable' }).first().click();
    await page.getByRole('textbox', { name: 'NewTable' }).first().press('ControlOrMeta+a');
    await page.getByRole('textbox', { name: 'NewTable' }).first().fill('TestEntity');
    await page.getByText('Save').click();
    await page
      .locator(
        'div:nth-child(11) > .ModelTree-module__treeNodeTitle__-a > .ModelTree-module__symbol__IA',
      )
      .click();
    await expect(page.getByText('TestEntity').nth(1)).toBeVisible();
    await page
      .locator(
        'div:nth-child(22) > .ModelTree-module__treeNodeTitle__-a > .ModelTree-module__symbol__IA',
      )
      .click();
    await page.getByText('TestEntity').nth(1).click();
    await page.getByText('TestEntity').nth(1).dblclick();
    await page.getByRole('img').nth(5).click();
    await page.getByText('TestEntity').click();
    await page.getByText('TestEntity').dblclick();
    await expect(page.getByText('Database Entity: TestEntity')).toBeVisible();
  });
});
