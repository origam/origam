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

const SCREEN = {
  label: 'WidgetSectionTest',
  schemaItemId: '87ac3988-10a4-47e0-a37a-6ef0826d1bcf',
  widgetId: '775fa5ea-fa75-40a7-8c39-7828f7cdf508',
};
const COMPONENT_LABEL = 'WidgetSectionTest [Origam.Schema.GuiModel.PanelControlSet]';
const SELECTED_MEMBER = 'WidgetSectionTestMaster';
const LONG_MEMBER = 'WidgetSectionTestMaster.WidgetSectionTestDetail.WidgetSectionTestDetailDetail';

async function openWidgetProperties(page: Page, request: APIRequestContext): Promise<Locator> {
  const response = await request.post('/Tab/Open', { data: { schemaItemId: SCREEN.schemaItemId } });
  expect(response.ok(), await response.text()).toBeTruthy();
  await page.goto('/');
  await closeAiPanel(page);
  await page.getByTestId(`tab-${SCREEN.label}`).click();
  await page
    .getByTestId('design-surface')
    .filter({ visible: true })
    .locator(`[class~="${SCREEN.widgetId}"]`)
    .click({ position: { x: 3, y: 3 } });
  await page.getByText('Properties', { exact: true }).click();
  await expect(page.getByTestId('properties-component-select')).toHaveValue(SCREEN.widgetId);
  return page.getByTestId('properties');
}

function propertyInput(properties: Locator, name: string): Locator {
  return properties
    .getByTestId(`property-label-${name}`)
    .locator('xpath=following-sibling::div[1]//input');
}

async function widthOf(locator: Locator): Promise<number> {
  return locator.evaluate(element => Math.round(element.getBoundingClientRect().width));
}

async function panelWidthOf(properties: Locator): Promise<number> {
  return widthOf(properties.locator('xpath=..'));
}

async function horizontalOverflowOf(properties: Locator): Promise<number> {
  return properties
    .locator('xpath=../..')
    .evaluate(scrollArea => scrollArea.scrollWidth - scrollArea.clientWidth);
}

async function widenSideBar(page: Page, distance: number): Promise<void> {
  const splitter = await page.getByRole('separator').last().boundingBox();
  const startX = splitter!.x + splitter!.width / 2;
  const middleY = splitter!.y + splitter!.height / 2;
  await page.mouse.move(startX, middleY);
  await page.mouse.down();
  await page.mouse.move(startX - distance, middleY, { steps: 5 });
  await page.mouse.up();
}

test.describe('Properties panel width (real backend)', () => {
  test.use({ viewport: { width: 1600, height: 900 } });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
  });

  test('the properties fit the side bar and grow with it', async ({ page, request }) => {
    const properties = await openWidgetProperties(page, request);
    expect(await horizontalOverflowOf(properties)).toBe(0);
    const narrowWidth = await widthOf(properties);
    const narrowInputWidth = await widthOf(propertyInput(properties, 'PanelTitle'));

    await widenSideBar(page, 400);

    await expect.poll(() => panelWidthOf(properties)).toBe(narrowWidth + 400);
    expect(await widthOf(properties)).toBe(narrowWidth + 400);
    expect(await horizontalOverflowOf(properties)).toBe(0);
    expect(await widthOf(propertyInput(properties, 'PanelTitle'))).toBeGreaterThan(
      narrowInputWidth + 300,
    );
  });

  test('the values stay usable in the narrowest side bar', async ({ page, request }) => {
    await page.addInitScript(() => localStorage.setItem('sidebarWidth', '250'));
    const properties = await openWidgetProperties(page, request);

    expect(await horizontalOverflowOf(properties)).toBe(0);
    expect(await widthOf(propertyInput(properties, 'PanelTitle'))).toBeGreaterThan(60);
    await expect(page.getByTestId('properties-component-select')).toHaveAttribute(
      'title',
      COMPONENT_LABEL,
    );
  });

  test('a cropped drop-down value shows its full text in a tooltip', async ({ page, request }) => {
    const properties = await openWidgetProperties(page, request);
    const dataMember = propertyInput(properties, 'DataMember');
    await dataMember.scrollIntoViewIfNeeded();
    await expect(dataMember).toHaveAttribute('title', SELECTED_MEMBER);

    await dataMember.click();
    const longOption = page.getByText(LONG_MEMBER, { exact: true });
    await expect(longOption).toBeVisible();
    expect(await longOption.evaluate(item => item.scrollWidth > item.clientWidth)).toBe(true);
    await expect(longOption).toHaveAttribute('title', LONG_MEMBER);
  });
});
