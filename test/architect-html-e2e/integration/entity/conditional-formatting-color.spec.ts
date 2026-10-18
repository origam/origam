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
import { activatePackage } from '@support/activatePackage';
import { closeAiPanel } from '@support/aiPanel';
import {
  awaitPropertyUpdate,
  collectServerErrors,
  expectModelFileContent,
  expectNoServerErrors,
  fillProperty,
  fillRejectedProperty,
  save,
} from '@support/propertyEditor';
import { readModelFile, resetBackend } from '@support/resetBackend';

const PACKAGE = 'Root';
const ENTITY_FILE = 'Root/DataEntity/Work Queue/WorkQueueEntry.origam';
const RULE = '90_Blue_SR_WorkQueueEntry_InRetryIsTrue_*';
const STORED_BACKGROUND = '-10185235';
const STORED_BACKGROUND_HEX = '#6495ed';
const RED_ARGB = '-65536';

test.describe('Color property of a conditional formatting rule (real backend)', () => {
  test.use({ actionTimeout: 10_000, navigationTimeout: 20_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, PACKAGE);
  });

  test('shows the stored color in a color picker', async ({ page }) => {
    const serverErrors = collectServerErrors(page);

    await openRuleEditor(page);

    await expect(page.getByTestId('property-color-BackgroundColor')).toHaveValue(
      STORED_BACKGROUND_HEX,
    );
    await expect(page.getByTestId('property-input-BackgroundColor')).toHaveValue(
      STORED_BACKGROUND_HEX,
    );
    await expectNoServerErrors(page, serverErrors);
  });

  test('shows an unset color as empty', async ({ page }) => {
    await openRuleEditor(page);

    await expect(page.getByTestId('property-input-ForegroundColor')).toHaveValue('');
    await expect(page.getByTestId('property-color-ForegroundColor')).toHaveClass(
      /colorPickerEmpty/,
    );
  });

  test('stores a color picked in the color picker', async ({ page }) => {
    await openRuleEditor(page);

    await awaitPropertyUpdate(page, () =>
      page.getByTestId('property-color-BackgroundColor').fill('#ff0000'),
    );
    await save(page);

    await expectModelFileContent(ENTITY_FILE, content =>
      content.includes(`ecf:backgroundColor="${RED_ARGB}"`),
    );
  });

  test('stores a typed color', async ({ page }) => {
    await openRuleEditor(page);

    await fillProperty(page, 'BackgroundColor', '#FF0000');

    await expect(page.getByTestId('property-input-BackgroundColor')).toHaveValue('#ff0000');
    await save(page);
    await expectModelFileContent(ENTITY_FILE, content =>
      content.includes(`ecf:backgroundColor="${RED_ARGB}"`),
    );
  });

  test('removes a cleared color', async ({ page }) => {
    await openRuleEditor(page);

    await fillProperty(page, 'BackgroundColor', '');
    await save(page);

    await expectModelFileContent(ENTITY_FILE, content => !content.includes('ecf:backgroundColor='));
  });

  test('keeps the rule untouched when the same color is typed', async ({ page }) => {
    await openRuleEditor(page);

    await fillProperty(page, 'BackgroundColor', STORED_BACKGROUND_HEX.toUpperCase());

    // The typed text is shown until the server answer is applied.
    await expect(page.getByTestId('property-input-BackgroundColor')).toHaveValue(
      STORED_BACKGROUND_HEX,
    );
    await expect(page.getByTestId('property-input-Name')).toHaveValue(RULE);
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();
  });

  for (const rejectedColor of ['Red', '#f00', '#ff00001']) {
    test(`rejects ${rejectedColor} as a color`, async ({ page }) => {
      await openRuleEditor(page);

      await fillRejectedProperty(page, 'BackgroundColor', rejectedColor);

      await expect(page.getByTestId('property-color-BackgroundColor')).toHaveValue(
        STORED_BACKGROUND_HEX,
      );
    });
  }
});

async function openRuleEditor(page: Page): Promise<void> {
  expect(readModelFile(ENTITY_FILE)).toContain(`ecf:backgroundColor="${STORED_BACKGROUND}"`);
  await page.goto('/');
  await closeAiPanel(page);
  for (const group of [
    'Data',
    'Entities',
    'Work Queue',
    'WorkQueueEntry',
    'Conditional Formatting',
  ]) {
    await page.getByTestId(`tree-toggle-${group}`).click();
  }
  await page.getByTestId(`tree-node-${RULE}`).dblclick();
  await expect(page.getByTestId(`tab-${RULE}`)).toBeVisible();
}
