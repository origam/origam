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
import { readModelFile, resetBackend } from '@support/resetBackend';

const SCREEN_FILE = 'Widgets/FormControlSet/NewForm.origam';
const DATA_SOURCE = 'WidgetSectionTest';
const MASTER_SECTION = 'WidgetSectionTestMaster';
const DETAIL_SECTION = 'WidgetSectionTestDetail';
const DETAIL_DATA_MEMBER = 'WidgetSectionTestMaster.WidgetSectionTestDetail';

interface ApiProperty {
  name: string;
  value: unknown;
  dropDownValues: { name: string; value: unknown }[];
}

interface ApiControl {
  id: string;
  type: string;
  properties: ApiProperty[];
  children: ApiControl[];
}

function property(control: ApiControl, name: string): ApiProperty | undefined {
  return control.properties.find(candidate => candidate.name === name);
}

function findControl(control: ApiControl, schemaItemName: string): ApiControl | undefined {
  if (property(control, 'SchemaItemName')?.value === schemaItemName) {
    return control;
  }
  for (const child of control.children) {
    const found = findControl(child, schemaItemName);
    if (found) {
      return found;
    }
  }
  return undefined;
}

class ScreenEditor {
  readonly toolbox: Locator;
  readonly surface: Locator;
  private screenId = '';

  constructor(
    private readonly page: Page,
    private readonly request: APIRequestContext,
  ) {
    this.toolbox = page.getByTestId('toolbox');
    this.surface = page.getByTestId('design-surface');
  }

  async openNew(): Promise<void> {
    await this.page.goto('/');
    await closeAiPanel(this.page);
    await this.page.getByTestId('tree-toggle-User Interface').click();
    await this.page.getByTestId('tree-toggle-Screens').click();
    await this.page.getByTestId('tree-node-Screens').click({ button: 'right' });
    await this.page.getByTestId('tree-menu-new').getByText('New').click();
    await this.page
      .getByTestId('tree-menu-new-Origam.Schema.GuiModel.FormControlSet')
      .getByText('Screen')
      .click();
    await expect(this.page.locator('#root')).toContainText('Screen Editor: NewForm');

    await this.toolbox.getByRole('textbox').first().fill(DATA_SOURCE);
    await this.awaitUpdate(() => this.pickOption(DATA_SOURCE));
    this.screenId = await this.toolbox
      .getByText('Id', { exact: true })
      .locator('xpath=following-sibling::input')
      .inputValue();
  }

  async drop(
    toolboxTab: 'Screen Sections' | 'Widgets',
    item: string,
    target: Locator,
    position: { x: number; y: number },
  ): Promise<ApiControl> {
    await this.toolbox.getByText(toolboxTab, { exact: true }).click();
    const created = this.page.waitForResponse(response =>
      response.url().includes('/ScreenEditor/CreateItem'),
    );
    const updated = this.page.waitForResponse(response =>
      response.url().includes('/ScreenEditor/Update'),
    );
    await this.toolbox
      .getByText(item, { exact: true })
      .filter({ visible: true })
      .dragTo(target, { targetPosition: position });
    const response = await created;
    expect(response.ok(), await response.text()).toBeTruthy();
    expect((await updated).ok()).toBeTruthy();
    return (await response.json()).screenItem as ApiControl;
  }

  component(control: ApiControl): Locator {
    return this.surface.locator(`[class~="${control.id}"]`);
  }

  async select(control: ApiControl): Promise<void> {
    await this.component(control).click({ position: { x: 3, y: 3 } });
    await this.page.getByText('Properties', { exact: true }).click();
  }

  async setProperty(name: string, value: string): Promise<void> {
    await this.awaitUpdate(() => this.propertyInput(name).fill(value));
  }

  async chooseProperty(name: string, option: string): Promise<void> {
    await this.propertyInput(name).click();
    await this.awaitUpdate(() => this.pickOption(option));
  }

  async save(): Promise<void> {
    const saved = this.page.waitForResponse(response =>
      response.url().includes('/Tab/PersistChanges'),
    );
    await this.page.getByTestId('save-button').click();
    const response = await saved;
    expect(response.ok(), await response.text()).toBeTruthy();
  }

  async readScreen(): Promise<ApiControl> {
    const response = await this.request.post('/ScreenEditor/Update', {
      data: { schemaItemId: this.screenId, modelChanges: [] },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
    return (await response.json()).data.rootControl as ApiControl;
  }

  private propertyInput(name: string): Locator {
    return this.page
      .locator('span', { hasText: new RegExp(`^${name}$`) })
      .locator('xpath=../following-sibling::div[1]//input');
  }

  private async pickOption(option: string): Promise<void> {
    await this.page
      .getByRole('listitem')
      .filter({ hasText: new RegExp(`^${option.replaceAll('.', '\\.')}$`) })
      .click();
  }

  private async awaitUpdate(action: () => Promise<void>): Promise<void> {
    const updated = this.page.waitForResponse(response =>
      response.url().includes('/ScreenEditor/Update'),
    );
    await action();
    const response = await updated;
    expect(response.ok(), await response.text()).toBeTruthy();
  }
}

test.describe('Screen editor widgets (real backend)', () => {
  let editor: ScreenEditor;

  test.beforeEach(async ({ page, request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
    editor = new ScreenEditor(page, request);
  });

  test('toolbox offers the widgets the client can render', async () => {
    await editor.openNew();
    await editor.toolbox.getByText('Widgets', { exact: true }).click();

    for (const widget of ['AsTree', 'Label', 'Panel', 'SplitPanel', 'TabControl', 'TestPlugin']) {
      await expect(
        editor.toolbox.getByText(widget, { exact: true }).filter({ visible: true }),
      ).toBeVisible();
    }
  });

  test('TabControl gets two named tab pages', async ({ page }) => {
    await editor.openNew();
    const tabControl = await editor.drop('Widgets', 'TabControl', editor.surface, {
      x: 60,
      y: 60,
    });

    expect(property(tabControl, 'SchemaItemName')?.value).toBe('TabControl');
    expect(tabControl.children.map(tab => property(tab, 'SchemaItemName')?.value)).toEqual([
      'TabPage',
      'TabPage1',
    ]);
    await expect(editor.surface.getByText(/TabPage/u)).toHaveCount(2);
    await expect(page.locator('#root')).not.toContainText('prepareNewValue_');
  });

  test('SplitPanel docks its sections in tab index order', async () => {
    await editor.openNew();
    const splitPanel = await editor.drop('Widgets', 'SplitPanel', editor.surface, {
      x: 60,
      y: 60,
    });
    const lower = await editor.drop(
      'Screen Sections',
      DETAIL_SECTION,
      editor.component(splitPanel),
      { x: 40, y: 150 },
    );
    const upper = await editor.drop(
      'Screen Sections',
      MASTER_SECTION,
      editor.component(splitPanel),
      { x: 40, y: 5 },
    );

    expect(property(lower, 'SchemaItemName')?.value).toBe('AsPanel1');
    expect(property(upper, 'SchemaItemName')?.value).toBe('AsPanel2');
    expect(
      property(upper, 'DataMember')?.dropDownValues.map(dropDownValue => dropDownValue.value),
    ).toContain(DETAIL_DATA_MEMBER);

    const screen = await editor.readScreen();
    const savedUpper = findControl(screen, 'AsPanel2')!;
    const savedLower = findControl(screen, 'AsPanel1')!;
    expect(property(savedUpper, 'TabIndex')?.value).toBe(0);
    expect(property(savedLower, 'TabIndex')?.value).toBe(1);
    expect(property(savedUpper, 'Top')?.value).toBe(10);
    expect(Number(property(savedLower, 'Top')?.value)).toBeGreaterThan(
      Number(property(savedUpper, 'Height')?.value),
    );
  });

  test('Label, Panel, AsTree and a plugin are saved with their properties', async () => {
    await editor.openNew();
    const panel = await editor.drop('Widgets', 'Panel', editor.surface, { x: 40, y: 40 });
    const label = await editor.drop('Widgets', 'Label', editor.component(panel), {
      x: 20,
      y: 20,
    });
    const tree = await editor.drop('Widgets', 'AsTree', editor.surface, { x: 260, y: 40 });
    const plugin = await editor.drop('Widgets', 'TestPlugin', editor.surface, {
      x: 40,
      y: 260,
    });

    expect(property(label, 'Text')?.value).toBe('Label');
    expect(property(plugin, 'Text')?.value).toBe('TestPlugin');

    await editor.select(tree);
    await editor.chooseProperty('DataMember', DETAIL_DATA_MEMBER);
    await editor.setProperty('IDColumn', 'Id');
    await editor.select(plugin);
    await editor.setProperty('test1', 'hello');
    await editor.save();

    const screenXml = readModelFile(SCREEN_FILE);
    for (const widget of ['Panel', 'Label', 'AsTree', 'TestPlugin']) {
      expect(screenXml).toContain('#' + widget + '/');
    }
    const screen = await editor.readScreen();
    expect(findControl(findControl(screen, 'Panel')!, 'Label')).toBeTruthy();
    expect(property(findControl(screen, 'AsTree')!, 'DataMember')?.value).toBe(DETAIL_DATA_MEMBER);
    expect(property(findControl(screen, 'TestPlugin')!, 'test1')?.value).toBe('hello');
  });
});
