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
import { menuItem, openContextMenu } from '@support/modelTree';
import { resetBackend } from '@support/resetBackend';

const WORKFLOW_PACKAGE = 'Workflow';
const ACTION_NAME = 'MergeTestThirdPhase';
const UNMAPPED_PARAMETER = 'MergeThroughDifferentEntity';
const MAPPED_PARAMETER = 'WorkflowMergeTestEntity';
const PARAMETER_MAPPING_TYPE = 'Origam.Schema.GuiModel.EntityUIActionParameterMapping';
const SCREEN_CONDITION_TYPE = 'Origam.Schema.GuiModel.ScreenCondition';

function actionMenuItem(page: Page, testId: string): Locator {
  return menuItem(page, ACTION_NAME, testId);
}

test.describe('Workflow action parameters in an entity (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, WORKFLOW_PACKAGE);
  });

  test('offers only parameter mappings under unmapped names', async ({ page }) => {
    await page.goto('/');
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();
    await page.getByTestId('tree-toggle-Workflow').click();
    await page.getByTestId('tree-toggle-WorkflowMergeTestEntity').click();
    await page.getByTestId('tree-toggle-UI Actions').click();
    await openContextMenu(page, ACTION_NAME);

    await expect(actionMenuItem(page, `tree-menu-new-${SCREEN_CONDITION_TYPE}`)).toHaveCount(1);
    await expect(actionMenuItem(page, `tree-menu-new-${PARAMETER_MAPPING_TYPE}`)).toHaveCount(0);
    const newSubmenu = actionMenuItem(page, 'tree-menu-new');
    await expect(newSubmenu.locator('.contexify_separator')).toHaveCount(1);
    await expect(
      actionMenuItem(page, `tree-menu-new-param-${UNMAPPED_PARAMETER}-${PARAMETER_MAPPING_TYPE}`),
    ).toHaveCount(1);
    await expect(
      actionMenuItem(page, `tree-menu-new-param-${UNMAPPED_PARAMETER}-${SCREEN_CONDITION_TYPE}`),
    ).toHaveCount(0);
    await expect(actionMenuItem(page, `tree-menu-new-param-${MAPPED_PARAMETER}`)).toHaveCount(0);
  });
});
