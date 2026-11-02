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

const newSubmenuItems = [
  'Database Field',
  'Virtual Field',
  'Function Call',
  'Aggregated Field',
  'Lookup Field',
  'Relationship',
  'Parameter',
  'Filter',
  'Index',
  'Row Level Security Filter',
  'Row Level Security Rule',
  'Conditional Formatting Rule',
  'Menu Action',
  'Report Action',
  'Sequential Workflow Action',
  'Dropdown Action',
];

test.describe('Entity context menu (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('all New submenu items are revealed and visible', async ({ page }) => {
    await page.goto('/');

    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-toggle-Dimensions').click();

    await page.getByTestId('tree-node-Dimension1').click({ button: 'right' });

    const newSubmenu = page.getByTestId('tree-menu-new');
    await newSubmenu.hover();
    await newSubmenu.evaluate(node => node.classList.add('contexify_submenu-isOpen'));
    await expect(newSubmenu).toHaveClass(/contexify_submenu-isOpen/);

    for (const caption of newSubmenuItems) {
      await expect(newSubmenu.getByRole('menuitem', { name: caption, exact: true })).toBeVisible();
    }
  });

  test('an ancestor entity shows a context menu with all items disabled', async ({ page }) => {
    await page.goto('/');

    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-toggle-Dimensions').click();
    await page.getByTestId('tree-toggle-Dimension1').click();
    await page.getByTestId('tree-toggle-_Ancestors').click();
    await page.getByTestId('tree-node-IDimension').first().click({ button: 'right' });

    const menu = page.getByRole('menu');
    await expect(menu).toBeVisible();
    const items = menu.getByRole('menuitem', { includeHidden: true });
    await expect(items).not.toHaveCount(0);
    for (const item of await items.all()) {
      await expect(item).toHaveAttribute('aria-disabled', 'true');
    }
    for (const action of ['edit', 'delete', 'documentation', 'references', 'dependencies']) {
      await expect(menu.getByTestId(`tree-menu-${action}`)).toHaveCount(0);
    }
  });
});
