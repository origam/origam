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

interface DesignerTab {
  label: string;
  schemaItemId: string;
  widgetId: string;
}

const ARRAY_TEST_SECTION: DesignerTab = {
  label: 'ArrayTest',
  schemaItemId: '3ce8ac9f-3ecd-428b-9931-9fb63ff4f7dd',
  widgetId: '374e714d-1934-4a31-aea0-7445ce1ca37a',
};
const DETAIL_SECTION: DesignerTab = {
  label: 'WidgetSectionTestDetail',
  schemaItemId: 'a9ab4be7-d8f6-4a02-af9c-60633e4d91b4',
  widgetId: '89be97a4-86e8-4036-b57a-36155e3f2322',
};
const WIDGET_SECTION_TEST_SCREEN: DesignerTab = {
  label: 'WidgetSectionTest',
  schemaItemId: '87ac3988-10a4-47e0-a37a-6ef0826d1bcf',
  widgetId: '775fa5ea-fa75-40a7-8c39-7828f7cdf508',
};
const ARRAY_TEST_SCREEN: DesignerTab = {
  label: 'ArrayTest',
  schemaItemId: '64266781-83b6-44e2-919e-49fb63be0691',
  widgetId: '7bce881e-dcef-4eaf-8e53-f8d03f3c46e2',
};

class DesignerTabs {
  readonly properties: Locator;
  readonly componentSelect: Locator;

  constructor(private readonly page: Page) {
    this.properties = page.getByTestId('properties');
    this.componentSelect = page.getByTestId('properties-component-select');
  }

  async open(request: APIRequestContext, tabs: DesignerTab[]): Promise<void> {
    for (const tab of tabs) {
      const response = await request.post('/Tab/Open', {
        data: { schemaItemId: tab.schemaItemId },
      });
      expect(response.ok(), await response.text()).toBeTruthy();
    }
    await this.page.goto('/');
    await closeAiPanel(this.page);
    for (const tab of tabs) {
      await this.activate(tab);
      await expect(this.widget(tab)).toBeVisible();
    }
  }

  async activate(tab: DesignerTab): Promise<void> {
    await this.page.getByTestId(`tab-${tab.label}`).click();
  }

  async selectWidget(tab: DesignerTab): Promise<void> {
    await this.widget(tab).click({ position: { x: 3, y: 3 } });
    await this.page.getByText('Properties', { exact: true }).click();
    await expect(this.componentSelect).toHaveValue(tab.widgetId);
  }

  // Inactive tabs stay in the DOM, only hidden.
  private widget(tab: DesignerTab): Locator {
    return this.page
      .getByTestId('design-surface')
      .filter({ visible: true })
      .locator(`[class~="${tab.widgetId}"]`);
  }
}

test.describe('Properties panel follows the active section tab (real backend)', () => {
  let tabs: DesignerTabs;

  test.beforeEach(async ({ page, request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
    tabs = new DesignerTabs(page);
    await tabs.open(request, [ARRAY_TEST_SECTION, DETAIL_SECTION]);
  });

  test('switching tabs shows the selection of the active section', async () => {
    await tabs.activate(ARRAY_TEST_SECTION);
    await tabs.selectWidget(ARRAY_TEST_SECTION);
    await tabs.activate(DETAIL_SECTION);
    await tabs.selectWidget(DETAIL_SECTION);

    await tabs.activate(ARRAY_TEST_SECTION);

    await expect(tabs.componentSelect).toHaveValue(ARRAY_TEST_SECTION.widgetId);
  });

  test('switching to a section without a selection empties the panel', async () => {
    await tabs.activate(DETAIL_SECTION);
    await tabs.selectWidget(DETAIL_SECTION);

    await tabs.activate(ARRAY_TEST_SECTION);

    await expect(tabs.properties.locator('[data-test-id^="property-"]')).toHaveCount(0);
  });

  test('a property edit goes to the section of the active tab', async ({ page }) => {
    for (const section of [ARRAY_TEST_SECTION, DETAIL_SECTION]) {
      await tabs.activate(section);
      await tabs.selectWidget(section);
      const updated = page.waitForResponse(response =>
        response.url().includes('/SectionEditor/Update'),
      );
      await page.getByTestId('property-input-Caption').fill(`Caption of ${section.label}`);
      const request = (await updated).request();
      expect(request.postDataJSON().schemaItemId, section.label).toBe(section.schemaItemId);
    }
  });
});

test.describe('Properties panel follows the active screen tab (real backend)', () => {
  let tabs: DesignerTabs;

  test.beforeEach(async ({ page, request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
    tabs = new DesignerTabs(page);
    await tabs.open(request, [WIDGET_SECTION_TEST_SCREEN, ARRAY_TEST_SCREEN]);
  });

  test('switching tabs shows the selection of the active screen', async () => {
    await tabs.activate(WIDGET_SECTION_TEST_SCREEN);
    await tabs.selectWidget(WIDGET_SECTION_TEST_SCREEN);
    await tabs.activate(ARRAY_TEST_SCREEN);
    await tabs.selectWidget(ARRAY_TEST_SCREEN);

    await tabs.activate(WIDGET_SECTION_TEST_SCREEN);

    await expect(tabs.componentSelect).toHaveValue(WIDGET_SECTION_TEST_SCREEN.widgetId);
  });

  test('switching to a screen without a selection empties the panel', async () => {
    await tabs.activate(ARRAY_TEST_SCREEN);
    await tabs.selectWidget(ARRAY_TEST_SCREEN);

    await tabs.activate(WIDGET_SECTION_TEST_SCREEN);

    await expect(tabs.properties.locator('[data-test-id^="property-"]')).toHaveCount(0);
  });
});
