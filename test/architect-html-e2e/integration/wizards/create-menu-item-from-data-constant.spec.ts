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
import { openConstants } from '@support/modelTree';
import { readModelFile, resetBackend } from '@support/resetBackend';

const CONSTANT = 'ReportTemplateName';
const CONSTANT_ID = 'fe570c7e-fe40-4712-84cc-807face163ce';

async function openWizard(page: Page) {
  await openConstants(page);
  await page.getByTestId('tree-toggle-Widgets').click();
  await page.getByTestId(`tree-node-${CONSTANT}`).click({ button: 'right' });
  await page.getByText('Actions', { exact: true }).click();
  await page.getByText('Create Menu Item').click();

  const dialog = page.getByRole('dialog');
  await expect(dialog).toContainText('Create Menu from Data Constant');
  return dialog;
}

test.describe('Create Menu Item from Data Constant wizard (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
  });

  test('creates a Data Constant Reference menu item', async ({ page }) => {
    const dialog = await openWizard(page);

    const nextButton = page.getByRole('button', { name: 'Next →' });
    await expect(nextButton).toBeDisabled();
    await page.getByRole('textbox', { name: `e.g. ${CONSTANT}` }).fill('ConstantMenuCaption');
    await nextButton.click();

    await expect(dialog).toContainText('Data Constant Reference');
    await expect(dialog).toContainText(`Role${CONSTANT}`);

    const createResponse = page.waitForResponse(response =>
      response.url().includes('/wizards/data-constant-menu-items'),
    );
    await page.getByRole('button', { name: 'Create Menu Item' }).click();
    const response = await createResponse;
    expect(response.ok(), await response.text()).toBeTruthy();

    await page.getByRole('button', { name: 'Show result' }).click();
    await expect(page.locator('tbody')).toContainText(`Menu\\${CONSTANT}`);
    await expect(page.locator('#root')).toContainText('Search results for "Menu Item"');

    const menuFile = readModelFile('Widgets/Menu/Menu.origam');
    expect(menuFile).toContain(`dcrmi:constant="${CONSTANT}#${CONSTANT}/${CONSTANT_ID}"`);
    expect(menuFile).toContain('ami:displayName="ConstantMenuCaption"');
    expect(menuFile).toContain(`ami:roles="${CONSTANT}"`);
  });
});
