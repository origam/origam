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
import {
  missingId,
  plantBrokenGroupReference,
  plantMisnamedFile,
  plantMissingReference,
  plantOrphanFile,
  plantOverlongColumnName,
} from '@support/modelDefects';
import { resetBackend, restoreModelFiles } from '@support/resetBackend';

// Reading the whole model twice takes a while on a cold run.
const VALIDATION_TIMEOUT = 90_000;

async function runValidation(page: Page) {
  await page.goto('/');
  const validateButton = page.getByTestId('topbar-check-model');
  await expect(validateButton).toBeVisible();
  await validateButton.click();
  // The results tab only opens once the run has finished.
  await expect(page.getByTestId('model-check-results')).toBeVisible({
    timeout: VALIDATION_TIMEOUT,
  });
}

function fileSection(page: Page, caption: string) {
  return page.getByTestId('model-check-file-section').filter({ hasText: caption });
}

// Each test damages the model files on disk, so they must not overlap.
test.describe.configure({ mode: 'serial' });

test.describe('Model validation', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('reports a clean model as having no problems', async ({ page }) => {
    await runValidation(page);

    await expect(page.getByTestId('model-check-empty')).toBeVisible();
    await expect(page.getByTestId('model-check-summary')).toContainText('No problems found');
    await expect(page.getByTestId('model-check-file-section')).toHaveCount(0);
  });

  test('reports an identifier longer than the database allows', async ({ page }) => {
    const { itemPath } = plantOverlongColumnName();

    await runValidation(page);

    await expect(page.getByTestId('model-check-summary')).toContainText('1 rule violations');
    const ruleTable = page.getByTestId('schema-item-results-table');
    await expect(ruleTable).toContainText('Length limit exceeded');
    await expect(ruleTable).toContainText(itemPath);
    await expect(ruleTable).toContainText('Database Field');
  });

  test('reports a reference to an id that is not in the model', async ({ page }) => {
    const { instanceId } = plantMissingReference();

    await runValidation(page);

    const section = fileSection(page, 'Invalid References Between Origam Files');
    await expect(section).toContainText(instanceId);
    await expect(section).toContainText(missingId);
    await expect(section).toContainText('1.0.3.origam');
  });

  test('reports a file that no model element references', async ({ page }) => {
    const { fileName } = plantOrphanFile();

    await runValidation(page);

    const section = fileSection(page, 'not referenced by any model element');
    await expect(section).toContainText(fileName);
  });

  test('reports an object stored in a file with a different name', async ({ page }) => {
    const { expectedFileName, actualFileName } = plantMisnamedFile();

    await runValidation(page);

    const section = fileSection(page, 'Objects persisted in wrong files');
    await expect(section).toContainText(expectedFileName);
    await expect(section).toContainText(actualFileName);
  });

  test('reports a rule violation on an item whose group is missing', async ({ page }) => {
    const { itemPath } = plantOverlongColumnName();
    const { missingGroupId } = plantBrokenGroupReference();

    await runValidation(page);

    // Confirms the broken group actually reached the checked model.
    await expect(page.getByTestId('model-check-results')).toContainText(missingGroupId);

    await expect(page.getByTestId('model-check-summary')).toContainText('1 rule violations');
    const ruleTable = page.getByTestId('schema-item-results-table');
    await expect(ruleTable).toContainText('Length limit exceeded');
    // A missing group must not degrade the row to an orphaned one.
    await expect(ruleTable).toContainText(itemPath);
    await expect(ruleTable).toContainText('Database Field');
    await expect(ruleTable).toContainText('Database Entity');
    await expect(ruleTable).toContainText('Widgets');
    await expect(ruleTable.getByTitle('Orphaned reference')).toHaveCount(0);
  });

  test('reports several problems at once and counts them in the summary', async ({ page }) => {
    plantOverlongColumnName();
    plantMissingReference();
    plantOrphanFile();

    await runValidation(page);

    const summary = page.getByTestId('model-check-summary');
    await expect(summary).toContainText('1 rule violations');
    await expect(summary).toContainText('2 file problems');
    await expect(fileSection(page, 'Invalid References Between Origam Files')).toBeVisible();
    await expect(fileSection(page, 'not referenced by any model element')).toBeVisible();
  });

  test('replaces the previous results when it runs again', async ({ page }) => {
    plantOrphanFile();
    await runValidation(page);
    await expect(fileSection(page, 'not referenced by any model element')).toBeVisible();

    // A second run on a repaired model has to clear what the first one found.
    restoreModelFiles();
    await page.getByTestId('topbar-check-model').click();

    await expect(page.getByTestId('model-check-empty')).toBeVisible({
      timeout: VALIDATION_TIMEOUT,
    });
    await expect(page.getByTestId('model-check-file-section')).toHaveCount(0);
  });
});
