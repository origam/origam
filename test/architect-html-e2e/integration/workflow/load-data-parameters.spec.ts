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
import { activatePackage } from '@support/activatePackage';
import { removeContextReference } from '@support/modelDefects';
import { openNodeMenu } from '@support/modelTree';
import { reloadBackend, resetBackend } from '@support/resetBackend';

const WIDGETS_PACKAGE = 'Widgets';
const WORKFLOW_FILE = 'Widgets/Workflow/Widgets/WorkQueueTest1.origam';
const PARAMETER_NAME = 'WorkQueueEntry_parId';
const CONTEXT_REFERENCE_TYPE = 'Origam.Schema.WorkflowModel.ContextReference';

async function expandLoadDataParameters(page: Page): Promise<Locator> {
  await page.goto('/');
  await page.getByTestId('tree-toggle-Business Logic').click();
  await page.getByTestId('tree-toggle-Sequential Workflows').click();
  await page.getByTestId('tree-toggle-Widgets').click();
  await page.getByTestId('tree-toggle-WorkQueueTest1').click();
  await page.getByTestId('tree-toggle-Tasks').click();
  await page.getByTestId('tree-toggle-0100_LoadData_WQ_TestQueue').click();
  await page.getByTestId('tree-toggle-Parameters').click();

  // The task folder comes first, the LoadData method parameter second.
  return page.getByTestId('tree-node-Parameters').nth(1);
}

test.describe('LoadData parameters in a work queue workflow (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('offers an unreferenced filter set parameter by name', async ({ page, request }) => {
    removeContextReference(WORKFLOW_FILE, PARAMETER_NAME);
    await reloadBackend(request, WIDGETS_PACKAGE);
    const parametersNode = await expandLoadDataParameters(page);

    const menu = await openNodeMenu(page, parametersNode);
    await menu.getByTestId('tree-menu-new').getByText('New').click();
    await menu
      .getByTestId(`tree-menu-new-param-${PARAMETER_NAME}`)
      .getByText(PARAMETER_NAME, { exact: true })
      .click();
    await menu
      .getByTestId(`tree-menu-new-param-${PARAMETER_NAME}-${CONTEXT_REFERENCE_TYPE}`)
      .click();

    await expect(page.getByTestId(`tab-${PARAMETER_NAME}`)).toBeVisible();
  });

  test('does not offer an already referenced parameter', async ({ page, request }) => {
    await activatePackage(request, WIDGETS_PACKAGE);
    const parametersNode = await expandLoadDataParameters(page);

    const menu = await openNodeMenu(page, parametersNode);

    await expect(menu.getByTestId(`tree-menu-new-${CONTEXT_REFERENCE_TYPE}`)).toHaveCount(1);
    await expect(menu.getByTestId(`tree-menu-new-param-${PARAMETER_NAME}`)).toHaveCount(0);
  });
});
