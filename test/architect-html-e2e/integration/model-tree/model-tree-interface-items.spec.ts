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

import { expect, test } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { menuItem, openContextMenu } from '@support/modelTree';
import { resetBackend } from '@support/resetBackend';

const SECURITY_RULE_TYPE = 'Origam.Schema.EntityModel.EntityFieldSecurityRule';
const REFERENCED_FIELD_ID = '6a92781b-ac57-4b24-a2a3-7ff772747431';
const DIMENSION2_ID = 'c4b132a4-306a-44a5-b051-a44c31b61322';

test.describe('Items of an interface entity (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('a new item under a field of an interface saves', async ({ page }) => {
    await page.goto('/');
    for (const nodeText of [
      'Data',
      'Entities',
      '__Origam General Interfaces',
      'IActive',
      'Fields',
    ]) {
      await page.getByTestId(`tree-toggle-${nodeText}`).click();
    }

    await openContextMenu(page, 'IsActive');
    await menuItem(page, 'IsActive', 'tree-menu-new').getByText('New').click();
    await menuItem(page, 'IsActive', `tree-menu-new-${SECURITY_RULE_TYPE}`).click();
    await expect(page.getByTestId('tab-Row Level Security Rule')).toBeVisible();

    const roles = page.getByTestId('property-input-Roles');
    const pendingUpdate = page.waitForResponse(response =>
      response.url().includes('/PropertyEditor/Update'),
    );
    await roles.fill('*');
    await roles.press('Tab');
    await pendingUpdate;
    const pendingSave = page.waitForResponse(response =>
      response.url().includes('/Tab/PersistChanges'),
    );
    await page.getByTestId('save-button').click();
    const saveResponse = await pendingSave;

    expect(saveResponse.ok(), await saveResponse.text()).toBeTruthy();
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();
    await expect(page.locator('.dialogMessage')).toHaveCount(0);
  });

  test('a folder menu still opens after a refused delete', async ({ page }) => {
    await page.goto('/');
    await page.getByTestId('tree-toggle-Data').click();
    await page.getByTestId('tree-toggle-Entities').click();

    const refusedDelete = await page.request.post('/Model/DeleteSchemaItem', {
      data: { schemaItemId: REFERENCED_FIELD_ID },
    });
    expect(refusedDelete.status()).toBe(400);

    await openContextMenu(page, 'Dimensions');
    await expect(menuItem(page, 'Dimensions', 'tree-menu-new')).not.toHaveAttribute(
      'aria-disabled',
      'true',
    );
  });

  test('a field of an interface cannot be moved into an entity that inherits it', async ({
    request,
  }) => {
    await activatePackage(request, 'Root');

    const response = await request.post('/Model/GetMoveVerdicts', {
      data: {
        source: { id: REFERENCED_FIELD_ID, nodeText: 'RecordCreated', isNonPersistentItem: false },
        targets: [{ id: DIMENSION2_ID, nodeText: 'Dimension2', isNonPersistentItem: false }],
      },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    expect(await response.json()).toEqual([
      expect.objectContaining({ canMove: false, canCopy: true }),
    ]);
  });
});
