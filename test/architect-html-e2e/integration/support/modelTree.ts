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

import { expect, type Locator, type Page } from '@playwright/test';
import fs from 'node:fs';
import { modelFilePath } from '@support/resetBackend';

export async function openConstants(page: Page): Promise<void> {
  await page.goto('/');
  await page.getByTestId('tree-toggle-Data').click();
  await page.getByTestId('tree-toggle-Constants').click();
}

// Both parents reload after the move, so the node exists twice for a moment.
export async function expectTreeSettled(page: Page, nodeText: string): Promise<void> {
  await expect(page.getByTestId(`tree-node-${nodeText}`)).toHaveCount(1);
}

// Every node renders its own menu and earlier ones stay around.
export function menuItem(page: Page, nodeText: string, testId: string): Locator {
  return page.getByTestId(`tree-node-${nodeText}`).locator('xpath=..').getByTestId(testId);
}

// The menu opens before its items arrive from the server.
export async function openContextMenu(page: Page, nodeText: string): Promise<void> {
  const pendingResponse = page.waitForResponse(
    response => response.url().includes('/Model/GetMenuItems'),
    { timeout: 10_000 },
  );
  await page.getByTestId(`tree-node-${nodeText}`).click({ button: 'right' });
  await pendingResponse;
  await expectBoxSettled(menuItem(page, nodeText, 'tree-menu-paste'));
}

// The arriving items change the height of the menu, which moves it.
export async function expectBoxSettled(locator: Locator): Promise<void> {
  let previous: string | null = null;
  await expect
    .poll(
      async () => {
        const box = await locator.boundingBox();
        const current = box && `${box.x} ${box.y} ${box.width} ${box.height}`;
        const hasSettled = !!current && current === previous;
        previous = current;
        return hasSettled;
      },
      { message: 'the context menu never stopped moving', timeout: 10_000 },
    )
    .toBe(true);
}

// The verdicts arrive for the whole tree and re-render every node.
export async function awaitMoveVerdicts(page: Page, action: () => Promise<void>): Promise<void> {
  await awaitResponse(page, '/Model/GetMoveVerdicts', action);
}

export async function awaitMoveTargets(page: Page, action: () => Promise<void>): Promise<void> {
  await awaitResponse(page, '/Model/GetMoveTargets', action);
}

// Reports a server side rejection instead of a later missing node.
export async function expectMoveSucceeds(page: Page, action: () => Promise<void>): Promise<void> {
  const response = await awaitResponse(page, '/Model/MoveNode', action);
  expect(response.ok(), await response.text()).toBeTruthy();
}

export async function expectModelFile(relativePath: string, exists: boolean): Promise<void> {
  await expect
    .poll(() => fs.existsSync(modelFilePath(relativePath)), {
      message: relativePath,
      timeout: 5_000,
    })
    .toBe(exists);
}

async function awaitResponse(page: Page, urlPart: string, action: () => Promise<void>) {
  const pendingResponse = page.waitForResponse(response => response.url().includes(urlPart), {
    timeout: 10_000,
  });
  await action();
  return pendingResponse;
}
