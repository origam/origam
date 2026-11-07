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

import { expect, test, type Locator, type Page } from '@playwright/test';
import { resetBackend } from '@support/resetBackend';

const NARROW_SIDEBAR_WIDTH = '250';
const SUBPIXEL_TOLERANCE = 1;

async function openLongAndWideTree(page: Page): Promise<Locator> {
  await page.addInitScript(
    width => localStorage.setItem('sidebarWidth', width),
    NARROW_SIDEBAR_WIDTH,
  );
  await page.goto('/');
  await page.getByTestId('tree-toggle-Data').click();
  await page.getByTestId('tree-toggle-Entities').click();
  await page.getByTestId('tree-toggle-Security').click();
  await expect(page.getByTestId('tree-node-OrigamRoleOrigamApplicationRole')).toBeVisible();
  return page.getByTestId('model-tree');
}

async function isInsideVisibleAreaOfScrollParents(tree: Locator): Promise<boolean> {
  return tree.evaluate((element, tolerance) => {
    const frame = element.getBoundingClientRect();
    for (let parent = element.parentElement; parent; parent = parent.parentElement) {
      if (getComputedStyle(parent).overflow === 'visible') {
        continue;
      }
      const parentBox = parent.getBoundingClientRect();
      const visibleLeft = parentBox.left + parent.clientLeft;
      const visibleTop = parentBox.top + parent.clientTop;
      const isInside =
        frame.left >= visibleLeft - tolerance &&
        frame.top >= visibleTop - tolerance &&
        frame.right <= visibleLeft + parent.clientWidth + tolerance &&
        frame.bottom <= visibleTop + parent.clientHeight + tolerance;
      if (!isInside) {
        return false;
      }
    }
    return true;
  }, SUBPIXEL_TOLERANCE);
}

test.describe('Model tree focus frame (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('stays on the edges of the panel while the tree scrolls', async ({ page }) => {
    const tree = await openLongAndWideTree(page);
    const topNode = page.getByTestId('tree-node-Data');

    await page.getByTestId('tree-node-Security').click();
    await expect(tree).toBeFocused();
    await expect(tree).toHaveCSS('outline-style', 'solid');
    expect(await isInsideVisibleAreaOfScrollParents(tree)).toBe(true);

    const frameBeforeScroll = await tree.boundingBox();
    const topNodeBeforeScroll = (await topNode.boundingBox())!;
    await page.mouse.wheel(0, 1000);
    await page.mouse.wheel(1000, 0);
    await expect
      .poll(async () => (await topNode.boundingBox())!.y)
      .toBeLessThan(topNodeBeforeScroll.y - 100);
    await expect
      .poll(async () => (await topNode.boundingBox())!.x)
      .toBeLessThan(topNodeBeforeScroll.x - 50);

    expect(await tree.boundingBox()).toEqual(frameBeforeScroll);
    expect(await isInsideVisibleAreaOfScrollParents(tree)).toBe(true);
  });

  test('highlights the selected row across the whole scrollable width', async ({ page }) => {
    const tree = await openLongAndWideTree(page);
    const selectedNode = page.getByTestId('tree-node-Security');

    await selectedNode.click();
    const selectedRow = selectedNode.locator('xpath=../..');
    await expect(selectedRow).toHaveClass(/selected/);

    const contentWidth = await tree.evaluate(element => element.scrollWidth);
    const rowWidth = (await selectedRow.boundingBox())!.width;
    expect(Math.abs(rowWidth - contentWidth)).toBeLessThan(SUBPIXEL_TOLERANCE);
  });
});
