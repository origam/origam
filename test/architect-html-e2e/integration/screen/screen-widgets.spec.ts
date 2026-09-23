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
const MASTER_DATA_MEMBER = 'WidgetSectionTestMaster';
const DETAIL_DATA_MEMBER = 'WidgetSectionTestMaster.WidgetSectionTestDetail';
const STYLE = 'alignCenter';
const NO_STYLE = '00000000-0000-0000-0000-000000000000';
const NO_VALUE_OPTION = '(none)';
const ALL_DATA_TYPES = 'AllDataTypes';
const ALL_DATA_TYPES_WITH_DETAIL = 'AllDataTypes_WithDetail';
const ARRAY_SECTION = 'AllDataTypes_WithArray';
const ARRAY_FIELD = 'ArrayTestId';

interface ApiProperty {
  name: string;
  value: unknown;
  category: string | null;
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
  readonly warnings: Locator;
  private screenId = '';

  constructor(
    private readonly page: Page,
    private readonly request: APIRequestContext,
  ) {
    this.toolbox = page.getByTestId('toolbox');
    this.surface = page.getByTestId('design-surface');
    this.warnings = page.locator('[data-test-id="designer-warning"]');
  }

  async openNew(dataSource = DATA_SOURCE): Promise<void> {
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

    await this.chooseDataSource(dataSource);
    this.screenId = await this.toolbox
      .getByText('Id', { exact: true })
      .locator('xpath=following-sibling::input')
      .inputValue();
  }

  async chooseDataSource(dataSource: string): Promise<void> {
    await this.toolbox.getByRole('textbox').first().fill(dataSource);
    await this.awaitUpdate(() => this.pickOption(dataSource));
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

  async clearProperty(name: string): Promise<void> {
    await this.propertyInput(name).click();
    const noValueOption = this.page.getByRole('listitem').first();
    await expect(noValueOption).toHaveText(NO_VALUE_OPTION);
    await this.awaitUpdate(() => noValueOption.click());
  }

  propertyInput(name: string): Locator {
    return this.page
      .locator('span', { hasText: new RegExp(`^${name}$`) })
      .locator('xpath=../following-sibling::div[1]//input');
  }

  async remove(control: ApiControl): Promise<void> {
    await this.select(control);
    const deleted = this.page.waitForResponse(response =>
      response.url().includes('/ScreenEditor/Delete'),
    );
    await this.page.keyboard.press('Delete');
    const response = await deleted;
    expect(response.ok(), await response.text()).toBeTruthy();
  }

  async moveInto(control: ApiControl, container: ApiControl): Promise<void> {
    const moved = (await this.component(control).boundingBox())!;
    const target = (await this.component(container).boundingBox())!;
    const updated = this.page.waitForResponse(response =>
      response.url().includes('/ScreenEditor/Update'),
    );
    await this.page.mouse.move(moved.x + 5, moved.y + 5);
    await this.page.mouse.down();
    await this.page.mouse.move(target.x + 20, target.y + 30, { steps: 10 });
    // A drag shorter than 300 ms counts as a click and puts the widget back.
    await this.page.waitForTimeout(400);
    await this.page.mouse.up();
    const response = await updated;
    expect(response.ok(), await response.text()).toBeTruthy();
  }

  async useTabMenu(tabPageName: string, item: 'Add New' | 'Delete'): Promise<void> {
    await this.surface.getByText(tabPageName, { exact: true }).click({ button: 'right' });
    const responded = this.page.waitForResponse(response =>
      response.url().includes('/ScreenEditor/'),
    );
    await this.page.getByRole('menuitem').filter({ hasText: item }).click();
    const response = await responded;
    expect(response.ok(), await response.text()).toBeTruthy();
  }

  verb(id: 'add-tab' | 'remove-tab'): Locator {
    return this.page.getByTestId(`designer-verb-${id}`);
  }

  async useVerb(id: 'add-tab' | 'remove-tab'): Promise<void> {
    const responded = this.page.waitForResponse(response =>
      response.url().includes('/ScreenEditor/'),
    );
    await this.verb(id).click();
    const response = await responded;
    expect(response.ok(), await response.text()).toBeTruthy();
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

  test('each tab page keeps the widget dropped on it', async () => {
    await editor.openNew();
    const tabControl = await editor.drop('Widgets', 'TabControl', editor.surface, {
      x: 60,
      y: 60,
    });
    const insidePage = { x: 40, y: 60 };

    const section = await editor.drop(
      'Screen Sections',
      MASTER_SECTION,
      editor.component(tabControl),
      insidePage,
    );
    await editor.surface.getByText('TabPage1', { exact: true }).click();
    const label = await editor.drop('Widgets', 'Label', editor.component(tabControl), insidePage);

    const screen = await editor.readScreen();
    expect(findControl(screen, 'TabPage')!.children.map(child => child.id)).toEqual([section.id]);
    expect(findControl(screen, 'TabPage1')!.children.map(child => child.id)).toEqual([label.id]);
  });

  test('switching tabs highlights the clicked tab and shows only its widgets', async ({ page }) => {
    await editor.openNew();
    const tabControl = await editor.drop('Widgets', 'TabControl', editor.surface, {
      x: 60,
      y: 60,
    });
    const insidePage = { x: 40, y: 60 };
    const tabLabel = (name: string) => editor.surface.getByText(name, { exact: true });
    const pageLabels: Record<string, string> = {};

    for (const tabPageName of ['TabPage', 'TabPage1', 'TabPage2']) {
      if (tabPageName === 'TabPage2') {
        await editor.select(tabControl);
        await editor.useVerb('add-tab');
      }
      await tabLabel(tabPageName).click();
      const label = await editor.drop('Widgets', 'Label', editor.component(tabControl), insidePage);
      pageLabels[tabPageName] = property(label, 'Text')!.value as string;
    }

    for (const [tabPageName, holdMs] of [
      ['TabPage1', 0],
      ['TabPage', 400],
      ['TabPage2', 0],
      ['TabPage1', 400],
      ['TabPage', 0],
    ] as const) {
      const tab = tabLabel(tabPageName);
      const box = (await tab.boundingBox())!;
      await page.mouse.move(box.x + box.width / 2, box.y + box.height / 2);
      await page.mouse.down();
      await page.waitForTimeout(holdMs);
      await page.mouse.up();

      for (const [otherPageName, otherLabelText] of Object.entries(pageLabels)) {
        const isClicked = otherPageName === tabPageName;
        await expect(tabLabel(otherPageName)).toHaveClass(
          isClicked ? /activeTab/u : /^(?!.*activeTab)/u,
        );
        await expect(editor.surface.getByText(otherLabelText, { exact: true })).toHaveCount(
          isClicked ? 1 : 0,
        );
      }
    }
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

  test('a Panel alone is saved with the screen', async () => {
    await editor.openNew();
    const panel = await editor.drop('Widgets', 'Panel', editor.surface, {
      x: 60,
      y: 60,
    });

    expect(property(panel, 'SchemaItemName')?.value).toBe('Panel');
    await expect(editor.warnings).toHaveCount(0);
    await editor.save();

    expect(readModelFile(SCREEN_FILE)).toContain('#Panel/');
    expect(findControl(await editor.readScreen(), 'Panel')!.children).toHaveLength(0);
  });

  test('a Label alone keeps the text it is given', async () => {
    await editor.openNew();
    const label = await editor.drop('Widgets', 'Label', editor.surface, {
      x: 60,
      y: 60,
    });

    await editor.select(label);
    await editor.setProperty('Text', 'Audit info');
    await expect(editor.warnings).toHaveCount(0);
    await editor.save();

    expect(readModelFile(SCREEN_FILE)).toContain('#Label/');
    const savedLabel = findControl(await editor.readScreen(), 'Label')!;
    expect(property(savedLabel, 'Text')?.value).toBe('Audit info');
    expect(property(savedLabel, 'Text')?.category).toBe('Appearance');
    for (const layoutProperty of ['Top', 'Left', 'Width', 'Height']) {
      expect(property(savedLabel, layoutProperty)?.category).toBe('Layout');
    }
  });

  test('the Delete key removes the selected widget only', async () => {
    await editor.openNew();
    const panel = await editor.drop('Widgets', 'Panel', editor.surface, {
      x: 60,
      y: 60,
    });
    const label = await editor.drop('Widgets', 'Label', editor.component(panel), {
      x: 20,
      y: 20,
    });

    await editor.remove(label);

    const screen = await editor.readScreen();
    expect(findControl(screen, 'Label')).toBeUndefined();
    expect(findControl(screen, 'Panel')).toBeTruthy();
    await expect(editor.component(panel)).toBeVisible();
  });

  test('the tab label menu adds a third page and removes one again', async () => {
    await editor.openNew();
    const tabControl = await editor.drop('Widgets', 'TabControl', editor.surface, {
      x: 60,
      y: 60,
    });

    await editor.useTabMenu('TabPage1', 'Add New');
    const withThreePages = findControl(await editor.readScreen(), 'TabControl')!;
    expect(withThreePages.children.map(page => property(page, 'SchemaItemName')?.value)).toEqual([
      'TabPage',
      'TabPage1',
      'TabPage2',
    ]);

    await editor.useTabMenu('TabPage2', 'Delete');
    const withTwoPages = findControl(await editor.readScreen(), 'TabControl')!;
    expect(withTwoPages.children.map(page => property(page, 'SchemaItemName')?.value)).toEqual([
      'TabPage',
      'TabPage1',
    ]);
    expect(tabControl.children).toHaveLength(2);
  });

  test('Add Tab opens a new page and Remove Tab removes the open one', async () => {
    await editor.openNew();
    const tabControl = await editor.drop('Widgets', 'TabControl', editor.surface, {
      x: 60,
      y: 60,
    });
    await editor.select(tabControl);

    await editor.useVerb('add-tab');
    await expect(editor.surface.getByText(/^TabPage/u)).toHaveCount(3);

    await editor.useVerb('remove-tab');
    const tabPageNames = async () =>
      findControl(await editor.readScreen(), 'TabControl')!.children.map(
        tab => property(tab, 'SchemaItemName')?.value,
      );
    expect(await tabPageNames()).toEqual(['TabPage', 'TabPage1']);

    await editor.useVerb('remove-tab');
    expect(await tabPageNames()).toEqual(['TabPage1']);
    await expect(editor.verb('remove-tab')).toBeDisabled();
    await expect(editor.verb('add-tab')).toBeEnabled();
  });

  test('dragging a widget into a Panel makes it a child of that Panel', async () => {
    await editor.openNew();
    const panel = await editor.drop('Widgets', 'Panel', editor.surface, {
      x: 20,
      y: 20,
    });
    const label = await editor.drop('Widgets', 'Label', editor.surface, {
      x: 300,
      y: 320,
    });

    expect(findControl(await editor.readScreen(), 'Panel')!.children).toHaveLength(0);
    await editor.moveInto(label, panel);

    const movedInto = findControl(await editor.readScreen(), 'Panel')!;
    expect(movedInto.children.map(child => child.id)).toEqual([label.id]);
  });

  test('a SplitPanel nested in another SplitPanel keeps its own two halves', async () => {
    await editor.openNew();
    const outer = await editor.drop('Widgets', 'SplitPanel', editor.surface, {
      x: 60,
      y: 60,
    });
    const inner = await editor.drop('Widgets', 'SplitPanel', editor.component(outer), {
      x: 40,
      y: 150,
    });
    const outerUpper = await editor.drop(
      'Screen Sections',
      MASTER_SECTION,
      editor.component(outer),
      { x: 40, y: 5 },
    );
    const innerBox = (await editor.component(inner).boundingBox())!;
    const innerLower = await editor.drop(
      'Screen Sections',
      DETAIL_SECTION,
      editor.component(inner),
      {
        x: 20,
        y: Math.round(innerBox.height) - 10,
      },
    );
    const innerUpper = await editor.drop(
      'Screen Sections',
      MASTER_SECTION,
      editor.component(inner),
      {
        x: 20,
        y: 5,
      },
    );

    for (const section of [innerLower, innerUpper]) {
      await editor.select(section);
      await editor.chooseProperty(
        'DataMember',
        section === innerLower ? DETAIL_DATA_MEMBER : MASTER_DATA_MEMBER,
      );
    }
    await editor.select(outerUpper);
    await editor.chooseProperty('DataMember', MASTER_DATA_MEMBER);

    const screen = await editor.readScreen();
    const savedOuter = findControl(screen, 'SplitPanel')!;
    const savedInner = savedOuter.children.find(child => child.id === inner.id)!;
    expect(savedOuter.children).toHaveLength(2);
    expect(savedInner, 'The second SplitPanel is not inside the first one').toBeTruthy();
    expect(savedInner.children).toHaveLength(2);
    await expect(editor.warnings).toHaveCount(0);
  });

  test('the blank entry of a lookup takes the value back off', async () => {
    await editor.openNew();
    const section = await editor.drop('Screen Sections', MASTER_SECTION, editor.surface, {
      x: 60,
      y: 60,
    });
    const offeredStyles = property(section, 'StyleId')?.dropDownValues ?? [];
    expect(offeredStyles.map(offered => offered.name)).toContain(STYLE);

    await editor.select(section);
    await editor.chooseProperty('StyleId', STYLE);
    await expect(editor.propertyInput('StyleId')).toHaveValue(STYLE);

    await editor.clearProperty('StyleId');
    await expect(editor.propertyInput('StyleId')).toHaveValue('');

    const screen = await editor.readScreen();
    expect(property(findControl(screen, 'AsPanel1')!, 'StyleId')?.value).toBe(NO_STYLE);
  });

  test('picking a data source that holds every field of a section clears its warning', async ({
    page,
  }) => {
    await editor.openNew(ALL_DATA_TYPES);
    const section = await editor.drop('Screen Sections', ARRAY_SECTION, editor.surface, {
      x: 60,
      y: 60,
    });
    await editor.select(section);
    await editor.chooseProperty('DataMember', ALL_DATA_TYPES);
    await expect(editor.warnings.filter({ hasText: ARRAY_FIELD }).first()).toBeVisible();
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();

    await editor.chooseDataSource(ALL_DATA_TYPES_WITH_DETAIL);

    await expect(editor.warnings).toHaveCount(0);
    await editor.save();
  });

  test('Label, Panel, AsTree and a plugin are saved with their properties', async () => {
    await editor.openNew();
    const splitPanel = await editor.drop('Widgets', 'SplitPanel', editor.surface, {
      x: 60,
      y: 60,
    });
    const tree = await editor.drop('Widgets', 'AsTree', editor.component(splitPanel), {
      x: 40,
      y: 150,
    });
    const panel = await editor.drop('Widgets', 'Panel', editor.component(splitPanel), {
      x: 40,
      y: 5,
    });
    const label = await editor.drop('Widgets', 'Label', editor.component(panel), {
      x: 20,
      y: 10,
    });
    const plugin = await editor.drop('Widgets', 'TestPlugin', editor.component(panel), {
      x: 20,
      y: 50,
    });

    expect(property(label, 'Text')?.value).toBe('Label');
    expect(property(plugin, 'Text')?.value).toBe('TestPlugin');
    await expect(editor.warnings.filter({ hasText: 'AsTree' }).first()).toBeVisible();

    await editor.select(tree);
    await editor.chooseProperty('DataMember', DETAIL_DATA_MEMBER);
    await editor.setProperty('IDColumn', 'Id');
    await editor.setProperty('ParentIDColumn', 'refWidgetSectionTestMasterId');
    await editor.setProperty('NameColumn', 'Text1');
    await editor.select(plugin);
    await editor.setProperty('test1', 'hello');
    await editor.chooseProperty('DataMember', DETAIL_DATA_MEMBER);
    await expect(editor.warnings).toHaveCount(0);
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
