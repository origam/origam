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

import fs from 'node:fs';
import { expect, test, type Locator, type Page } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { expectBoxSettled } from '@support/modelTree';
import { modelFilePath, reloadBackend, resetBackend } from '@support/resetBackend';

const WORKFLOW_PACKAGE = 'Workflow';
const WORKFLOW_FILE = 'Workflow/Workflow/Workflow/BooleanContextStoreTest.origam';
const PARAMETER_NAME = 'boolean';
const CONTEXT_REFERENCE_TYPE = 'Origam.Schema.WorkflowModel.ContextReference';
const CONTEXT_STORE_NAME = 'BooleanContextStore';

function removeParameterReference(): void {
  const filePath = modelFilePath(WORKFLOW_FILE);
  const content = fs.readFileSync(filePath, 'utf8');
  const reference = new RegExp(
    `<cr:WorkflowContextReference[^>]*asi:name="${PARAMETER_NAME}"[^>]*>\\s*`,
  );
  if (!reference.test(content)) {
    throw new Error(`"${PARAMETER_NAME}" reference was not found in ${filePath}.`);
  }
  fs.writeFileSync(filePath, content.replace(reference, ''), 'utf8');
}

async function expandTransformParameters(page: Page): Promise<Locator> {
  await page.goto('/');
  await page.getByTestId('tree-toggle-Business Logic').click();
  await page.getByTestId('tree-toggle-Sequential Workflows').click();
  await page.getByTestId('tree-toggle-Workflow').click();
  await page.getByTestId('tree-toggle-BooleanContextStoreTest').click();
  await page.getByTestId('tree-toggle-Tasks').click();
  await page.getByTestId('tree-toggle-0200_Transform_FillStringContextStore').click();
  await page.getByTestId('tree-toggle-Parameters').click();

  // The task folder comes first, the Transform method parameter second.
  return page.getByTestId('tree-node-Parameters').nth(1);
}

async function openNodeMenu(page: Page, node: Locator): Promise<Locator> {
  // A menu opened while the previous one is still closing would close with it.
  await expect(page.getByRole('menu')).toHaveCount(0);
  const pendingResponse = page.waitForResponse(
    response => response.url().includes('/Model/GetMenuItems'),
    { timeout: 10_000 },
  );
  await node.click({ button: 'right' });
  await pendingResponse;

  const menu = node.locator('xpath=..');
  await expectBoxSettled(menu.getByTestId('tree-menu-paste'));
  return menu;
}

async function selectContextStore(page: Page): Promise<void> {
  const input = page.getByTestId('property-select-ContextStore').locator('input');
  await input.fill(CONTEXT_STORE_NAME);
  const pendingUpdate = page.waitForResponse(
    response => response.url().includes('/PropertyEditor/Update'),
    { timeout: 10_000 },
  );
  await input.press('Tab');
  await pendingUpdate;
}

test.describe('Transformation parameters in a workflow (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('offers an unreferenced parameter by name until it is saved', async ({ page, request }) => {
    removeParameterReference();
    await reloadBackend(request, WORKFLOW_PACKAGE);
    const parametersNode = await expandTransformParameters(page);

    const menu = await openNodeMenu(page, parametersNode);
    await menu.getByTestId('tree-menu-new').getByText('New').click();
    await menu
      .getByTestId(`tree-menu-new-param-${PARAMETER_NAME}`)
      .getByText(PARAMETER_NAME, { exact: true })
      .click();
    await expect(menu.getByTestId(`tree-menu-new-${CONTEXT_REFERENCE_TYPE}`)).toHaveCount(0);
    await expect(menu.getByTestId('tree-menu-new').locator('.contexify_separator')).toHaveCount(0);
    await menu
      .getByTestId(`tree-menu-new-param-${PARAMETER_NAME}-${CONTEXT_REFERENCE_TYPE}`)
      .click();
    await expect(page.getByTestId(`tab-${PARAMETER_NAME}`)).toBeVisible();

    await selectContextStore(page);
    await page.getByTestId('save-button').click();
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();

    await openNodeMenu(page, parametersNode);
    await expect(menu.getByTestId(`tree-menu-new-${CONTEXT_REFERENCE_TYPE}`)).toHaveCount(1);
    await expect(menu.getByTestId(`tree-menu-new-param-${PARAMETER_NAME}`)).toHaveCount(0);
  });

  test('does not offer an already referenced parameter', async ({ page, request }) => {
    await activatePackage(request, WORKFLOW_PACKAGE);
    const parametersNode = await expandTransformParameters(page);

    const menu = await openNodeMenu(page, parametersNode);

    await expect(menu.getByTestId(`tree-menu-new-${CONTEXT_REFERENCE_TYPE}`)).toHaveCount(1);
    await expect(menu.getByTestId(`tree-menu-new-param-${PARAMETER_NAME}`)).toHaveCount(0);
  });
});
