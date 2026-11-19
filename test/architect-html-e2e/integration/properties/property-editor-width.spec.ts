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

import { expect, test, type APIRequestContext, type Locator, type Page } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { closeAiPanel } from '@support/aiPanel';
import { resetBackend } from '@support/resetBackend';

type EditedItem = { label: string; schemaItemId: string };

const ENTITY: EditedItem = {
  label: 'OrigamChatMessage',
  schemaItemId: '478da4a8-dc69-42bb-b10a-7fab3eaa8d6d',
};
const SERVICE_METHOD_CALL_TASK: EditedItem = {
  label: '0100_LoadData_WQ_TestQueue',
  schemaItemId: '0f23be5d-078c-4304-ae94-a75e1f6219a7',
};
const LONG_PROPERTY = 'AuditingSecondReferenceKeyColumn';

async function openEditor(page: Page, request: APIRequestContext, item: EditedItem): Promise<void> {
  const response = await request.post('/Tab/Open', { data: { schemaItemId: item.schemaItemId } });
  expect(response.ok(), await response.text()).toBeTruthy();
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId(`tab-${item.label}`).click();
  await expect(propertyLabel(page, 'Name')).toBeVisible();
}

function propertyLabel(page: Page, name: string): Locator {
  return page.getByTestId(`property-label-${name}`).filter({ visible: true });
}

function propertyValue(page: Page, name: string): Locator {
  return propertyLabel(page, name).locator('xpath=following-sibling::div[1]');
}

function propertyEditor(page: Page): Locator {
  return propertyLabel(page, 'Name').locator('xpath=../../..');
}

async function boxOf(locator: Locator): Promise<{ x: number; width: number; height: number }> {
  const box = await locator.boundingBox();
  expect(box).not.toBeNull();
  return box!;
}

async function valueLeftEdgesOf(editor: Locator): Promise<number[]> {
  return editor.evaluate(element =>
    [...element.querySelectorAll('[data-test-id^="property-label-"]')].map(label =>
      Math.round(label.nextElementSibling!.getBoundingClientRect().x),
    ),
  );
}

async function gapToRightEdgeOf(value: Locator, editor: Locator): Promise<number> {
  const valueBox = await boxOf(value);
  const editorBox = await boxOf(editor);
  return Math.round(editorBox.x + editorBox.width - (valueBox.x + valueBox.width));
}

async function horizontalOverflowOf(editor: Locator): Promise<number> {
  return editor.evaluate(element => element.scrollWidth - element.clientWidth);
}

test.describe('Property editor width in an editor (real backend)', () => {
  test.use({ viewport: { width: 1600, height: 900 } });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
  });

  test('a long property name stays on one line', async ({ page, request }) => {
    await openEditor(page, request, ENTITY);

    const longLabel = await boxOf(propertyLabel(page, LONG_PROPERTY));
    const shortLabel = await boxOf(propertyLabel(page, 'Name'));
    expect(longLabel.height).toBe(shortLabel.height);
  });

  for (const item of [ENTITY, SERVICE_METHOD_CALL_TASK]) {
    test(`the values of ${item.label} line up and fill the editor`, async ({ page, request }) => {
      await openEditor(page, request, item);

      const valueLeftEdges = await valueLeftEdgesOf(propertyEditor(page));
      expect(valueLeftEdges.length).toBeGreaterThan(1);
      expect(new Set(valueLeftEdges).size).toBe(1);
      expect(
        await gapToRightEdgeOf(propertyValue(page, 'Name'), propertyEditor(page)),
      ).toBeLessThan(20);
    });
  }

  test('a long property name gives way in a narrow editor', async ({ page, request }) => {
    await page.setViewportSize({ width: 900, height: 900 });
    await openEditor(page, request, ENTITY);

    const editorWidth = (await boxOf(propertyEditor(page))).width;
    expect((await boxOf(propertyLabel(page, LONG_PROPERTY))).width).toBeLessThanOrEqual(
      editorWidth / 2,
    );
    expect((await boxOf(propertyValue(page, LONG_PROPERTY))).width).toBeGreaterThan(60);
    expect(await horizontalOverflowOf(propertyEditor(page))).toBe(0);
  });
});
