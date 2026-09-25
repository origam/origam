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
import fs from 'node:fs';
import path from 'node:path';
import { activatePackage } from '@support/activatePackage';
import { closeAiPanel } from '@support/aiPanel';
import { modelFilePath, resetBackend } from '@support/resetBackend';

const SECTION_FILE_NAME = 'NewPanel.origam';
const WIDGET_TYPES: Record<string, string> = {
  AsCombo: 'AsDropDown',
  GroupBox: 'GroupBoxWithChamfer',
  RadioButton: 'AsRadioButton',
};
const TAG_INPUT_SOURCE_LOOKUP_ID = '991a8bec-5169-4506-9457-7f91c27bb5fd';
const ARRAY_TEST_TEXT_LOOKUP_ID = 'ac42c31f-b978-4dd6-906a-3a736eeeb85f';
const REPORT_TEMPLATE_NAME_CONSTANT = 'ReportTemplateName';
const REPORT_TEMPLATE_NAME_CONSTANT_ID = 'fe570c7e-fe40-4712-84cc-807face163ce';
const LENGTH_TEST_CONSTANT = 'DataConstantLengthTest';
const LENGTH_TEST_CONSTANT_ID = '36bbe708-6fa8-445b-8fc2-1220f0bf7c4b';

interface ApiControl {
  id: string;
  name: string;
  type: string;
  properties: { name: string; value: unknown }[];
  children: ApiControl[];
}

function propertyValue(control: ApiControl, name: string): unknown {
  return control.properties.find(property => property.name === name)?.value;
}

function findWidget(control: ApiControl, shortType: string): ApiControl | undefined {
  for (const child of control.children) {
    if (child.type.endsWith('.' + shortType)) {
      return child;
    }
    const found = findWidget(child, shortType);
    if (found) {
      return found;
    }
  }
  return undefined;
}

function findControl(control: ApiControl, id: string): ApiControl | undefined {
  if (control.id === id) {
    return control;
  }
  for (const child of control.children) {
    const found = findControl(child, id);
    if (found) {
      return found;
    }
  }
  return undefined;
}

function findFile(directory: string, fileName: string): string | undefined {
  for (const entry of fs.readdirSync(directory, { withFileTypes: true })) {
    const entryPath = path.join(directory, entry.name);
    if (entry.isDirectory()) {
      const found = findFile(entryPath, fileName);
      if (found) {
        return found;
      }
    } else if (entry.name === fileName) {
      return entryPath;
    }
  }
  return undefined;
}

class SectionEditor {
  readonly toolbox: Locator;
  readonly surface: Locator;
  private sectionId = '';
  private dropCount = 0;

  constructor(
    private readonly page: Page,
    private readonly request: APIRequestContext,
  ) {
    this.toolbox = page.getByTestId('toolbox');
    this.surface = page.getByTestId('design-surface');
  }

  async openNew(dataSource: string): Promise<void> {
    await this.page.goto('/');
    await closeAiPanel(this.page);
    await this.page.getByTestId('tree-toggle-User Interface').click();
    await this.page.getByTestId('tree-toggle-Screen Sections').click();
    await this.page.getByTestId('tree-node-Screen Sections').click({ button: 'right' });
    await this.page.getByTestId('tree-menu-new').getByText('New').click();
    await this.page
      .getByTestId('tree-menu-new-Origam.Schema.GuiModel.PanelControlSet')
      .getByText('Screen Section')
      .click();
    await expect(this.page.locator('#root')).toContainText('Screen Section Editor: NewPanel');

    await this.toolbox.getByRole('textbox').first().fill(dataSource);
    await this.awaitUpdate(() => this.pickOption(dataSource));
    this.sectionId = await this.toolbox
      .getByText('Id', { exact: true })
      .locator('xpath=following-sibling::input')
      .inputValue();
  }

  async dropWidget(widget: string, field?: string, container?: ApiControl): Promise<ApiControl> {
    if (field) {
      await this.toolboxItem(field).click();
    }
    await this.toolbox.getByText('Widgets', { exact: true }).click();
    const target = container ? this.component(container) : this.surface;
    const position = container
      ? { x: 60, y: 30 + this.dropCount * 30 }
      : { x: 150, y: 40 + this.dropCount * 40 };
    this.dropCount += 1;
    const created = this.page.waitForResponse(response =>
      response.url().includes('/SectionEditor/CreateItem'),
    );
    await this.toolboxItem(widget).dragTo(target, {
      targetPosition: position,
    });
    const response = await created;
    expect(response.ok(), await response.text()).toBeTruthy();
    await this.toolbox.getByText('Fields', { exact: true }).click();
    return (await response.json()) as ApiControl;
  }

  async select(control: ApiControl): Promise<void> {
    await this.component(control).click({ position: { x: 3, y: 3 } });
    await this.page.getByText('Properties', { exact: true }).click();
  }

  async setProperty(name: string, value: string): Promise<void> {
    await this.awaitUpdate(() => this.propertyInput(name).fill(value));
  }

  async chooseProperty(name: string, option: string): Promise<void> {
    await this.propertyInput(name).fill(option);
    await this.awaitUpdate(() => this.pickOption(option));
  }

  async save(): Promise<void> {
    const saved = this.page.waitForResponse(response =>
      response.url().includes('/SectionEditor/Save'),
    );
    await this.page.getByTestId('save-button').click();
    const response = await saved;
    expect(response.ok(), await response.text()).toBeTruthy();
    expect((await response.json()).warnings).toEqual([]);
  }

  async readSavedWidget(widget: string): Promise<ApiControl> {
    const sectionFile = findFile(modelFilePath('Widgets/PanelControlSet'), SECTION_FILE_NAME);
    expect(sectionFile, SECTION_FILE_NAME + ' was not written').toBeTruthy();
    expect(fs.readFileSync(sectionFile!, 'utf8')).toContain('#' + widget + '/');

    const response = await this.request.post('/SectionEditor/Update', {
      data: { schemaItemId: this.sectionId, modelChanges: [] },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
    const rootControl = (await response.json()).data.rootControl as ApiControl;
    const shortType = WIDGET_TYPES[widget] ?? widget;
    const savedWidget = findWidget(rootControl, shortType);
    expect(savedWidget, widget + ' is not in the saved section').toBeTruthy();
    return savedWidget!;
  }

  private toolboxItem(name: string): Locator {
    return this.toolbox.getByText(name, { exact: true }).filter({ visible: true });
  }

  component(control: ApiControl): Locator {
    return this.surface.locator(`[class~="${control.id}"]`);
  }

  async checkProperty(name: string): Promise<void> {
    await this.awaitUpdate(() => this.page.getByTestId(`property-checkbox-${name}`).check());
  }

  propertyInput(name: string): Locator {
    return this.page
      .locator('span', { hasText: new RegExp(`^${name}$`) })
      .locator('xpath=../following-sibling::div[1]//input');
  }

  private async pickOption(option: string): Promise<void> {
    await this.page
      .getByRole('listitem')
      .filter({ hasText: new RegExp(`^${option}$`) })
      .click();
  }

  private async awaitUpdate(action: () => Promise<void>): Promise<void> {
    const updated = this.page.waitForResponse(response =>
      response.url().includes('/SectionEditor/Update'),
    );
    await action();
    const response = await updated;
    expect(response.ok(), await response.text()).toBeTruthy();
  }
}

test.describe('Screen section widgets (real backend)', () => {
  let editor: SectionEditor;

  test.beforeEach(async ({ page, request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
    editor = new SectionEditor(page, request);
  });

  test('AsTextBox bound to a string field', async () => {
    await editor.openNew('AllDataTypes');
    await editor.dropWidget('AsTextBox', 'Text1');
    await editor.save();

    const textBox = await editor.readSavedWidget('AsTextBox');
    expect(propertyValue(textBox, 'Value')).toBe('Text1');
  });

  test('AsDateBox bound to a date field', async () => {
    await editor.openNew('AllDataTypes');
    await editor.dropWidget('AsDateBox', 'Date1');
    await editor.save();

    const dateBox = await editor.readSavedWidget('AsDateBox');
    expect(propertyValue(dateBox, 'DateValue')).toBe('Date1');
  });

  test('AsCheckBox bound to a boolean field', async () => {
    await editor.openNew('AllDataTypes');
    await editor.dropWidget('AsCheckBox', 'Boolean1');
    await editor.save();

    const checkBox = await editor.readSavedWidget('AsCheckBox');
    expect(propertyValue(checkBox, 'Value')).toBe('Boolean1');
  });

  test('AsCombo takes the lookup of the bound field', async () => {
    await editor.openNew('AllDataTypes');
    await editor.dropWidget('AsCombo', 'refTagInputSourceId');
    await editor.save();

    const dropDown = await editor.readSavedWidget('AsCombo');
    expect(propertyValue(dropDown, 'LookupValue')).toBe('refTagInputSourceId');
    expect(propertyValue(dropDown, 'DataLookup')).toBe(TAG_INPUT_SOURCE_LOOKUP_ID);
  });

  test('ColorPicker bound to an integer field', async () => {
    await editor.openNew('AllDataTypes');
    await editor.dropWidget('ColorPicker', 'Integer1');
    await editor.save();

    const colorPicker = await editor.readSavedWidget('ColorPicker');
    expect(propertyValue(colorPicker, 'SelectedColor')).toBe('Integer1');
  });

  test('Checklist takes the lookup of the bound array field', async () => {
    await editor.openNew('AllDataTypes');
    await editor.dropWidget('Checklist', 'ArrayTestId');
    await editor.save();

    const checklist = await editor.readSavedWidget('Checklist');
    expect(propertyValue(checklist, 'Value')).toBe('ArrayTestId');
    expect(propertyValue(checklist, 'DataLookup')).toBe(ARRAY_TEST_TEXT_LOOKUP_ID);
  });

  test('TagInput takes the lookup of the bound array field', async () => {
    await editor.openNew('AllDataTypes');
    await editor.dropWidget('TagInput', 'TagInput');
    await editor.save();

    const tagInput = await editor.readSavedWidget('TagInput');
    expect(propertyValue(tagInput, 'Value')).toBe('TagInput');
    expect(propertyValue(tagInput, 'DataLookup')).toBe(TAG_INPUT_SOURCE_LOOKUP_ID);
  });

  test('Label with a text', async () => {
    await editor.openNew('AllDataTypes');
    const label = await editor.dropWidget('Label');
    await editor.select(label);
    await editor.setProperty('Text', 'Hello');
    await editor.save();

    const savedLabel = await editor.readSavedWidget('Label');
    expect(propertyValue(savedLabel, 'Text')).toBe('Hello');
  });

  test('GroupBox with a text box inside', async () => {
    await editor.openNew('AllDataTypes');
    const groupBox = await editor.dropWidget('GroupBox');
    await editor.select(groupBox);
    await editor.setProperty('Text', 'Group');
    await editor.dropWidget('AsTextBox', 'Text1', groupBox);
    await editor.save();

    const savedGroupBox = await editor.readSavedWidget('GroupBox');
    expect(propertyValue(savedGroupBox, 'Text')).toBe('Group');
    expect(savedGroupBox.children.map(child => child.type)).toEqual(['Origam.Gui.Win.AsTextBox']);
    expect(propertyValue(savedGroupBox.children[0], 'Value')).toBe('Text1');
  });

  test('BlobControl bound to the file name with the blob member set', async () => {
    await editor.openNew('Attachment');
    const blobControl = await editor.dropWidget('BlobControl', 'FileName');
    await editor.select(blobControl);
    await editor.setProperty('BlobMember', 'Data');
    await editor.save();

    const savedBlobControl = await editor.readSavedWidget('BlobControl');
    expect(propertyValue(savedBlobControl, 'FileName')).toBe('FileName');
    expect(propertyValue(savedBlobControl, 'BlobMember')).toBe('Data');
  });

  test('ImageBox bound to a blob field', async () => {
    await editor.openNew('Attachment');
    await editor.dropWidget('ImageBox', 'Data');
    await editor.save();

    const imageBox = await editor.readSavedWidget('ImageBox');
    expect(propertyValue(imageBox, 'ImageData')).toBe('Data');
  });

  test('RadioButton with a text and a value constant', async () => {
    await editor.openNew('AllDataTypes');
    const radioButton = await editor.dropWidget('RadioButton', 'Text1');
    await editor.select(radioButton);
    await editor.setProperty('Text', 'Yes');
    await editor.chooseProperty('ValueConstant', REPORT_TEMPLATE_NAME_CONSTANT);
    await editor.save();

    const savedRadioButton = await editor.readSavedWidget('RadioButton');
    expect(propertyValue(savedRadioButton, 'Value')).toBe('Text1');
    expect(propertyValue(savedRadioButton, 'Text')).toBe('Yes');
    expect(propertyValue(savedRadioButton, 'ValueConstant')).toBe(REPORT_TEMPLATE_NAME_CONSTANT_ID);
  });

  test('GroupBox text follows the Text property before the server answers', async ({ page }) => {
    await editor.openNew('AllDataTypes');
    const groupBox = await editor.dropWidget('GroupBox');
    await editor.select(groupBox);

    let releaseUpdate = () => {};
    const updateHeld = new Promise<void>(resolve => (releaseUpdate = resolve));
    await page.route('**/SectionEditor/Update', async route => {
      await updateHeld;
      await route.continue();
    });
    const updated = page.waitForResponse(response =>
      response.url().includes('/SectionEditor/Update'),
    );
    await editor.propertyInput('Text').fill('Group');
    await expect(editor.component(groupBox)).toContainText('Group');
    releaseUpdate();
    await updated;
    await page.unroute('**/SectionEditor/Update');
    await expect(editor.component(groupBox)).toContainText('Group');
  });

  test('Ctrl+S saves with a non-English keyboard layout', async ({ page }) => {
    await editor.openNew('AllDataTypes');
    await editor.dropWidget('AsTextBox', 'Text1');

    const saved = page.waitForResponse(response =>
      response.url().includes('/SectionEditor/Save'),
    );
    await page.evaluate(() =>
      window.dispatchEvent(
        new KeyboardEvent('keydown', { key: 'ы', code: 'KeyS', ctrlKey: true, bubbles: true }),
      ),
    );
    const response = await saved;
    expect(response.ok(), await response.text()).toBeTruthy();
    await expect(page.getByTestId('save-button-disabled')).toBeVisible();
  });

  test('Ctrl+S saves the value still being typed in a property', async ({ page }) => {
    await editor.openNew('AllDataTypes');
    const textBox = await editor.dropWidget('AsTextBox', 'Text1');
    await editor.select(textBox);
    await editor.propertyInput('Top').fill('137');

    const saved = page.waitForResponse(response =>
      response.url().includes('/SectionEditor/Save'),
    );
    await page.keyboard.press('Control+s');
    const response = await saved;
    expect(response.ok(), await response.text()).toBeTruthy();
    const sectionFile = findFile(modelFilePath('Widgets/PanelControlSet'), SECTION_FILE_NAME);
    expect(fs.readFileSync(sectionFile!, 'utf8')).toContain('value="137"');
  });

  test('ReadOnly paints a widget grey and takes the buttons off a combo', async () => {
    const white = 'rgb(255, 255, 255)';
    const grey = 'rgb(233, 232, 240)';
    await editor.openNew('AllDataTypes');
    const tagInput = await editor.dropWidget('TagInput', 'TagInput');
    const tagInputBox = editor.component(tagInput).locator('div').first();
    await expect(tagInputBox).toHaveCSS('background-color', white);
    await editor.select(tagInput);
    await editor.checkProperty('ReadOnly');
    await expect(tagInputBox).toHaveCSS('background-color', grey);

    const comboBox = await editor.dropWidget('AsCombo', 'refTagInputSourceId');
    const comboBoxBox = editor.component(comboBox).locator('div').first();
    await expect(comboBoxBox.locator('svg')).toHaveCount(2);
    await editor.select(comboBox);
    await editor.checkProperty('ReadOnly');
    await expect(comboBoxBox).toHaveCSS('background-color', grey);
    await expect(comboBoxBox.locator('svg')).toHaveCount(0);
  });

  test('a quick drag that leaves the surface still moves the widget', async ({ page }) => {
    await editor.openNew('AllDataTypes');
    const textBox = await editor.dropWidget('AsTextBox', 'Text1');
    const box = (await editor.component(textBox).boundingBox())!;
    const toolbox = (await editor.toolbox.boundingBox())!;
    const startX = box.x + 20;
    const startY = box.y + 10;

    await page.mouse.move(startX, startY);
    await page.mouse.down();
    await page.mouse.move(startX + 30, startY + 10);
    await page.mouse.move(toolbox.x + 20, startY + 10);
    await page.mouse.move(startX + 60, startY + 30);
    const updated = page.waitForResponse(
      response =>
        response.url().includes('/SectionEditor/Update') &&
        response.request().postDataJSON().modelChanges.length > 0,
    );
    await page.mouse.up();
    const response = await updated;
    expect(response.ok(), await response.text()).toBeTruthy();

    const rootControl = (await response.json()).data.rootControl as ApiControl;
    const movedTextBox = findControl(rootControl, textBox.id)!;
    expect(propertyValue(movedTextBox, 'Left')).toBe(
      (propertyValue(textBox, 'Left') as number) + 60,
    );
    expect(propertyValue(movedTextBox, 'Top')).toBe((propertyValue(textBox, 'Top') as number) + 30);
  });

  test('MultiColumnAdapterFieldWrapper with two children mapped to constants', async () => {
    await editor.openNew('AllDataTypes');
    const wrapper = await editor.dropWidget('MultiColumnAdapterFieldWrapper', 'Text1');
    const textBox = await editor.dropWidget('AsTextBox', 'Text2', wrapper);
    await editor.select(textBox);
    await editor.chooseProperty('MappingCondition', REPORT_TEMPLATE_NAME_CONSTANT);
    const dateBox = await editor.dropWidget('AsDateBox', 'Date1', wrapper);
    await editor.select(dateBox);
    await editor.chooseProperty('MappingCondition', LENGTH_TEST_CONSTANT);
    await editor.save();

    const savedWrapper = await editor.readSavedWidget('MultiColumnAdapterFieldWrapper');
    expect(propertyValue(savedWrapper, 'ControlMember')).toBe('Text1');
    expect(savedWrapper.children.map(child => child.type)).toEqual([
      'Origam.Gui.Win.AsTextBox',
      'Origam.Gui.Win.AsDateBox',
    ]);
    expect(propertyValue(savedWrapper.children[0], 'Value')).toBe('Text2');
    expect(propertyValue(savedWrapper.children[0], 'MappingCondition')).toBe(
      REPORT_TEMPLATE_NAME_CONSTANT_ID,
    );
    expect(propertyValue(savedWrapper.children[1], 'DateValue')).toBe('Date1');
    expect(propertyValue(savedWrapper.children[1], 'MappingCondition')).toBe(
      LENGTH_TEST_CONSTANT_ID,
    );
  });
});
