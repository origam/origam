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

import type { Locator, Page } from '@playwright/test';

interface DragOptions {
  copy?: boolean;
  whileOverTarget?: () => Promise<void>;
}

// Both nodes are measured up front.
export async function dragTreeNode(
  page: Page,
  source: Locator,
  target: Locator,
  options: DragOptions = {},
): Promise<void> {
  await source.scrollIntoViewIfNeeded();
  await target.scrollIntoViewIfNeeded();
  const from = await centerOf(source);
  const to = await centerOf(target);

  // No wait for the verdicts here on purpose, the drop has to survive a fast drag.
  await page.mouse.move(from.x, from.y);
  await page.mouse.down();
  await page.mouse.move(from.x, from.y + 10, { steps: 4 });

  if (options.copy) {
    await page.keyboard.down('Control');
  }
  try {
    await page.mouse.move(to.x, to.y, { steps: 8 });
    // dragover fires on movement only, the second move confirms the modifier.
    await page.mouse.move(to.x + 1, to.y, { steps: 2 });
    await options.whileOverTarget?.();
    await page.mouse.up();
  } finally {
    if (options.copy) {
      await page.keyboard.up('Control');
    }
  }
}

async function centerOf(locator: Locator): Promise<{ x: number; y: number }> {
  const box = await locator.boundingBox();
  if (!box) {
    throw new Error('The tree node is not rendered, it has no bounding box.');
  }
  return { x: box.x + box.width / 2, y: box.y + box.height / 2 };
}
