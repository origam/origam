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
import { closeAiPanel } from '@support/aiPanel';
import { resetBackend } from '@support/resetBackend';

const ITEM = 'DimensionEntityRelation';
const LONG_ENTITY = 'WorkQueueNotificationContactType';
const LONG_FIELD = 'refTargetDimensionEntityId';
const SCREEN_SECTION = 'DimensionTransformationMatrix';
const DEFAULT_WIDTH = 292;
const MIN_WIDTH = 200;
const MIN_DESIGN_SURFACE_WIDTH = 200;

async function openDesigner(page: Page, groupName: string): Promise<Locator> {
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId('tree-toggle-User Interface').click();
  await page.getByTestId(`tree-toggle-${groupName}`).click();
  await page.getByTestId('tree-toggle-Dimensions').click();
  await page.getByTestId(`tree-node-${ITEM}`).dblclick();
  const toolbox = page.getByTestId('toolbox').filter({ visible: true });
  await expect(toolbox).toBeVisible();
  return toolbox;
}

async function widthOf(locator: Locator): Promise<number> {
  return locator.evaluate(element => Math.round(element.getBoundingClientRect().width));
}

async function dragSplitter(page: Page, toolbox: Locator, targetX: (startX: number) => number) {
  const splitter = (await toolbox.getByRole('separator').boundingBox())!;
  const startX = splitter.x + splitter.width / 2;
  const middleY = splitter.y + splitter.height / 2;
  await page.mouse.move(startX, middleY);
  await page.mouse.down();
  await page.mouse.move(targetX(startX), middleY, { steps: 5 });
  await page.mouse.up();
}

async function isCutOff(item: Locator): Promise<boolean> {
  return item.evaluate(element => element.scrollWidth > element.clientWidth);
}

async function horizontalOverflowAround(toolbox: Locator): Promise<number> {
  return toolbox.evaluate(element => {
    let overflow = 0;
    for (let ancestor = element.parentElement; ancestor; ancestor = ancestor.parentElement) {
      overflow = Math.max(overflow, ancestor.scrollWidth - ancestor.clientWidth);
    }
    return overflow;
  });
}

async function editorWidthOf(toolbox: Locator): Promise<number> {
  return toolbox.evaluate(element => element.parentElement!.clientWidth);
}

test.describe('Designer toolbox width (real backend)', () => {
  test.use({ viewport: { width: 1600, height: 900 } });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
  });

  test('the screen section toolbox can be resized and keeps its width', async ({ page }) => {
    const toolbox = await openDesigner(page, 'Screen Sections');
    expect(await widthOf(toolbox)).toBe(DEFAULT_WIDTH);
    const dataSource = toolbox.getByRole('textbox').first();
    const defaultInputWidth = await widthOf(dataSource);
    const fieldRow = toolbox.getByTitle(LONG_FIELD, { exact: true });
    const defaultRowWidth = await widthOf(fieldRow);

    await dragSplitter(page, toolbox, startX => startX + 200);

    expect(await widthOf(toolbox)).toBe(DEFAULT_WIDTH + 200);
    expect(await widthOf(dataSource)).toBe(defaultInputWidth + 200);
    expect(await widthOf(fieldRow)).toBeGreaterThan(defaultRowWidth + 150);

    await page.reload();
    await page.getByTestId(`tab-${ITEM}`).click();
    await expect(toolbox).toBeVisible();
    expect(await widthOf(toolbox)).toBe(DEFAULT_WIDTH + 200);
  });

  test('the screen toolbox uses the same width and can be resized', async ({ page }) => {
    await page.addInitScript(() => localStorage.setItem('designerToolboxWidth', '400'));
    const toolbox = await openDesigner(page, 'Screens');
    expect(await widthOf(toolbox)).toBe(400);
    await expect(toolbox.getByTitle(SCREEN_SECTION, { exact: true })).toContainText(SCREEN_SECTION);

    await dragSplitter(page, toolbox, startX => startX - 100);

    expect(await widthOf(toolbox)).toBe(300);
  });

  test('the toolbox leaves room for the design surface', async ({ page }) => {
    await page.setViewportSize({ width: 1200, height: 900 });
    const toolbox = await openDesigner(page, 'Screen Sections');

    await dragSplitter(page, toolbox, () => page.viewportSize()!.width - 1);

    const widestWidth = (await editorWidthOf(toolbox)) - MIN_DESIGN_SURFACE_WIDTH;
    expect(await widthOf(toolbox)).toBe(widestWidth);
    expect(await horizontalOverflowAround(toolbox)).toBe(0);
    expect(await page.evaluate(() => localStorage.getItem('designerToolboxWidth'))).toBe(
      String(widestWidth),
    );

    await page.evaluate(() => localStorage.setItem('designerToolboxWidth', '800'));
    await page.reload();
    await page.getByTestId(`tab-${ITEM}`).click();
    await expect(toolbox).toBeVisible();
    expect(await widthOf(toolbox)).toBe(widestWidth);
    expect(await horizontalOverflowAround(toolbox)).toBe(0);
  });

  test('a cut off name shows a tooltip until the toolbox is wide enough', async ({ page }) => {
    const toolbox = await openDesigner(page, 'Screen Sections');
    await dragSplitter(page, toolbox, startX => startX - 400);
    expect(await widthOf(toolbox)).toBe(MIN_WIDTH);

    const field = toolbox.getByTitle(LONG_FIELD, { exact: true });
    await expect(field).toContainText(LONG_FIELD);
    const fieldBox = (await field.boundingBox())!;
    const toolboxBox = (await toolbox.boundingBox())!;
    expect(fieldBox.x + fieldBox.width).toBeGreaterThan(toolboxBox.x + toolboxBox.width);
    const dataSource = toolbox.getByRole('textbox').first();
    await expect(dataSource).toHaveAttribute('title', ITEM);

    await dataSource.fill(LONG_ENTITY);
    const longOption = page.getByText(LONG_ENTITY, { exact: true });
    await expect(longOption).toBeVisible();
    expect(await isCutOff(longOption)).toBe(true);
    await expect(longOption).toHaveAttribute('title', LONG_ENTITY);

    await dragSplitter(page, toolbox, startX => startX + 300);
    await expect(longOption).toBeHidden();
    await expect(dataSource).toHaveValue(ITEM);

    await dataSource.fill(LONG_ENTITY);
    await expect(longOption).toBeVisible();
    expect(await isCutOff(longOption)).toBe(false);
  });
});
