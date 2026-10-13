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

import { expect, test, type Page, type Request } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { readModelFile, resetBackend } from '@support/resetBackend';

const TYPED_TEXT = 'thequickbrownfoxjumpsoverthelazydog';
const PROPERTY_UPDATE_PATH = '/PropertyEditor/Update';

const cases = [
  {
    editor: 'transformation',
    treePath: ['Business Logic', 'Transformations'],
    node: 'SD_Empty',
    modelFile:
      'Root/Transformation/SD_Empty.origam___text___693db72d-a400-4df3-b4b7-e0487e7ce8d6.xslt',
    loadedText: '<?xml version="1.0" encoding="UTF-8"?>',
    caretLineIndex: 6,
    caretLineEnd: '<ROOT>',
  },
  {
    editor: 'deployment script',
    treePath: ['Common', 'Deployment', '_origam_root', '4.1'],
    node: '00020_GrantPermissions (MsSql)',
    modelFile:
      'Root/DeploymentVersion/_origam_root/4.1.origam___commandText___617ae0bc-c9ea-4dc5-8981-4785fd20424e.txt',
    loadedText: 'GRANT EXEC ON TYPE::[dbo].[AsapListValue]',
    caretLineIndex: 0,
    caretLineEnd: '[public]',
  },
];

function editorCodeArea(page: Page) {
  return page.getByTestId('code-editor').first().locator('.view-lines');
}

function isPropertyUpdate(request: Request) {
  return new URL(request.url()).pathname.endsWith(PROPERTY_UPDATE_PATH);
}

function trackPropertyUpdates(page: Page) {
  const tracker = { sent: 0, pending: 0 };
  page.on('request', request => {
    if (isPropertyUpdate(request)) {
      tracker.sent++;
      tracker.pending++;
    }
  });
  const onDone = (request: Request) => {
    if (isPropertyUpdate(request)) {
      tracker.pending--;
    }
  };
  page.on('requestfinished', onDone);
  page.on('requestfailed', onDone);
  return tracker;
}

async function save(page: Page) {
  await page.getByTestId('save-button').first().click();
  await expect(page.getByTestId('save-button-disabled').first()).toBeVisible();
}

for (const {
  editor,
  treePath,
  node,
  modelFile,
  loadedText,
  caretLineIndex,
  caretLineEnd,
} of cases) {
  test.describe(`Typing in the ${editor} editor (real backend)`, () => {
    test.beforeEach(async ({ request, page }) => {
      await resetBackend(request);
      await activatePackage(request, 'Root');
      await page.goto('/');

      for (const folder of treePath) {
        await page.getByTestId(`tree-toggle-${folder}`).click();
      }
      await page.getByTestId(`tree-node-${node}`).dblclick();
      await expect(editorCodeArea(page)).toContainText(loadedText, { timeout: 30_000 });

      await editorCodeArea(page).click();
      await page.keyboard.press('Control+Home');
      for (let i = 0; i < caretLineIndex; i++) {
        await page.keyboard.press('ArrowDown');
      }
      await page.keyboard.press('End');
    });

    test('caret stays in place while property updates are slow', async ({ page }) => {
      await page.route(
        url => url.pathname.endsWith(PROPERTY_UPDATE_PATH),
        async route => {
          await new Promise(resolve => setTimeout(resolve, 300));
          await route.continue();
        },
      );
      const updates = trackPropertyUpdates(page);

      await page.keyboard.type(TYPED_TEXT, { delay: 50 });
      await expect.poll(() => updates.pending).toBe(0);

      await expect(editorCodeArea(page)).toContainText(`${caretLineEnd}${TYPED_TEXT}`);
    });

    test('typing does not send a property update per keystroke', async ({ page }) => {
      const updates = trackPropertyUpdates(page);

      await page.keyboard.type(TYPED_TEXT);
      await save(page);

      expect(updates.sent).toBeLessThanOrEqual(2);
    });

    test('text typed right before saving is persisted', async ({ page }) => {
      await page.keyboard.type(TYPED_TEXT);
      await save(page);

      expect(readModelFile(modelFile)).toContain(`${caretLineEnd}${TYPED_TEXT}`);
    });
  });
}
