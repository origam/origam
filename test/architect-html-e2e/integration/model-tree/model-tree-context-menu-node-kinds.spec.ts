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
import { menuItem, openContextMenu } from '@support/modelTree';
import { resetBackend } from '@support/resetBackend';

const SCHEMA_ITEM_ACTIONS = [
  'tree-menu-documentation',
  'tree-menu-references',
  'tree-menu-dependencies',
];
const DIMENSIONS = ['Data', 'Entities', 'Dimensions'];
const ANCESTOR = [...DIMENSIONS, 'Dimension1', '_Ancestors', 'IOrigamEntity2'];

const nodeKinds = [
  {
    kind: 'a provider',
    path: ['Data'],
    nodeText: 'Entities',
    isSchemaItem: false,
    canDelete: false,
  },
  {
    kind: 'a folder of another package',
    path: ['Data', 'Entities'],
    nodeText: 'Dimensions',
    isSchemaItem: false,
    canDelete: false,
  },
  {
    kind: 'an entity',
    path: DIMENSIONS,
    nodeText: 'Dimension1',
    isSchemaItem: true,
    canDelete: true,
  },
  {
    kind: 'the _Ancestors folder',
    path: [...DIMENSIONS, 'Dimension1'],
    nodeText: '_Ancestors',
    isSchemaItem: false,
    canDelete: false,
  },
  {
    kind: 'an ancestor',
    path: [...DIMENSIONS, 'Dimension1', '_Ancestors'],
    nodeText: 'IOrigamEntity2',
    isSchemaItem: false,
    canDelete: false,
  },
  {
    kind: 'a folder of an ancestor',
    path: ANCESTOR,
    nodeText: 'Fields',
    isSchemaItem: false,
    canDelete: false,
  },
  {
    kind: 'an inherited field',
    path: [...ANCESTOR, 'Fields'],
    nodeText: 'RecordCreated',
    isSchemaItem: true,
    canDelete: true,
  },
  {
    kind: 'a folder of an entity',
    path: [...DIMENSIONS, 'IDimension'],
    nodeText: 'Fields',
    isSchemaItem: false,
    canDelete: false,
  },
  {
    kind: 'a field',
    path: [...DIMENSIONS, 'IDimension', 'Fields'],
    nodeText: 'Name',
    isSchemaItem: true,
    canDelete: true,
  },
];

async function expand(page: Page, path: string[]): Promise<void> {
  await page.goto('/');
  for (const nodeText of path) {
    await page.getByTestId(`tree-toggle-${nodeText}`).click();
  }
}

async function clickMenuItem(
  page: Page,
  nodeText: string,
  testId: string,
  urlPart: string,
): Promise<void> {
  await openContextMenu(page, nodeText);
  const pendingResponse = page.waitForResponse(response => response.url().includes(urlPart), {
    timeout: 10_000,
  });
  await menuItem(page, nodeText, testId).click();
  const response = await pendingResponse;
  expect(response.ok(), await response.text()).toBeTruthy();
  await expect(page.locator('.dialogMessage')).toHaveCount(0);
}

test.describe('Model tree context menu by node kind (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  for (const { kind, path, nodeText, isSchemaItem, canDelete } of nodeKinds) {
    test(`${kind} offers only the actions it supports`, async ({ page }) => {
      await expand(page, path);

      await openContextMenu(page, nodeText);

      for (const testId of SCHEMA_ITEM_ACTIONS) {
        await expect(menuItem(page, nodeText, testId), testId).toHaveCount(isSchemaItem ? 1 : 0);
      }
      await expect(menuItem(page, nodeText, 'tree-menu-delete')).toHaveCount(canDelete ? 1 : 0);
    });
  }

  test('Delete of an inherited field refers to the field of the interface', async ({ page }) => {
    await expand(page, [...ANCESTOR, 'Fields']);

    await openContextMenu(page, 'RecordCreated');
    const pendingResponse = page.waitForResponse(response =>
      response.url().includes('/Model/DeleteSchemaItem'),
    );
    await menuItem(page, 'RecordCreated', 'tree-menu-delete').click();
    const response = await pendingResponse;

    expect(response.status()).toBe(400);
    await expect(page.locator('.dialogMessage')).toContainText(
      "Cannot delete item 'IOrigamEntity2\\RecordCreated'",
    );
  });

  test('a field of another package can be copied and moved', async ({ page }) => {
    await expand(page, [...DIMENSIONS, 'IDimension', 'Fields']);

    await openContextMenu(page, 'Name');

    for (const testId of ['tree-menu-copy', 'tree-menu-move-to']) {
      await expect(menuItem(page, 'Name', testId), testId).not.toHaveAttribute(
        'aria-disabled',
        'true',
      );
    }
  });

  test('Documentation, references and dependencies of an entity open', async ({ page }) => {
    await expand(page, DIMENSIONS);

    await clickMenuItem(page, 'Dimension1', 'tree-menu-references', '/Search/References');
    await expect(page.getByTestId('tab-References of: Dimension1')).toBeVisible();

    await clickMenuItem(page, 'Dimension1', 'tree-menu-dependencies', '/Search/Dependencies');
    await expect(page.getByTestId('tab-Dependencies of: Dimension1')).toBeVisible();

    await clickMenuItem(page, 'Dimension1', 'tree-menu-documentation', '/Documentation/OpenEditor');
  });

  test('Find dependencies lists each item once', async ({ page }) => {
    const duplicateKeyErrors: string[] = [];
    page.on('console', message => {
      if (message.type() === 'error' && message.text().includes('same key')) {
        duplicateKeyErrors.push(message.text());
      }
    });
    await expand(page, ['Data', 'Lookups', 'Dimensions']);

    await openContextMenu(page, 'Dimension1_Sales');
    const pendingResponse = page.waitForResponse(response =>
      response.url().includes('/Search/Dependencies'),
    );
    await menuItem(page, 'Dimension1_Sales', 'tree-menu-dependencies').click();
    const results = (await (await pendingResponse).json()) as { schemaId: string }[];

    await expect(page.getByTestId('tab-Dependencies of: Dimension1_Sales')).toBeVisible();
    expect(new Set(results.map(result => result.schemaId)).size).toBe(results.length);
    expect(duplicateKeyErrors).toEqual([]);
  });

  test('Edit of a work queue command shows each property once', async ({ page }) => {
    await expand(page, ['Business Logic', 'Work Queue Classes', 'TextFile', 'Commands']);

    await openContextMenu(page, 'Split');
    await menuItem(page, 'Split', 'tree-menu-edit').click();

    await expect(page.getByTestId('tab-Split')).toBeVisible();
    await expect(page.getByTestId('property-label-ParameterMappings')).toHaveCount(1);
  });

  test('Edit of an ancestor expands it', async ({ page }) => {
    await expand(page, [...DIMENSIONS, 'Dimension1', '_Ancestors']);

    await openContextMenu(page, 'IOrigamEntity2');
    await menuItem(page, 'IOrigamEntity2', 'tree-menu-edit').click();

    await expect(page.getByTestId('tree-node-Fields')).toBeVisible();
    await expect(page.locator('.dialogMessage')).toHaveCount(0);
  });
});
