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
import fs from 'node:fs';
import { activatePackage } from '@support/activatePackage';
import { modelFilePath, resetBackend } from '@support/resetBackend';
import {
  type ApiControl,
  findControl,
  findFile,
  propertyValue,
  SECTION_FILE_NAME,
  SectionEditor,
} from '@support/sectionEditor';

const TAG_INPUT_SOURCE_LOOKUP_ID = '991a8bec-5169-4506-9457-7f91c27bb5fd';
const ARRAY_TEST_TEXT_LOOKUP_ID = 'ac42c31f-b978-4dd6-906a-3a736eeeb85f';
const REPORT_TEMPLATE_NAME_CONSTANT = 'ReportTemplateName';
const REPORT_TEMPLATE_NAME_CONSTANT_ID = 'fe570c7e-fe40-4712-84cc-807face163ce';
const LENGTH_TEST_CONSTANT = 'DataConstantLengthTest';
const LENGTH_TEST_CONSTANT_ID = '36bbe708-6fa8-445b-8fc2-1220f0bf7c4b';

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

  for (const commit of ['Enter', 'Tab', 'blur'] as const) {
    test(`manual field binding survives save and reopen after ${commit}`, async ({ page }) => {
      const fieldName = commit === 'Tab' ? 'Text' : 'CalculatedField';
      await editor.openNew('AllDataTypes');
      const textBox = await editor.dropWidget('AsTextBox', 'Text1');
      await editor.select(textBox);
      const input = editor.propertyInput('Value');
      await input.fill(fieldName);
      if (commit === 'Tab') {
        await expect(page.getByRole('listitem').filter({ hasText: /^Text2$/ })).toBeVisible();
      } else {
        await expect(
          page.getByRole('listitem').filter({ hasText: /^CalculatedField$/ }),
        ).toHaveCount(0);
      }

      const updated = page.waitForResponse(response =>
        response.url().includes('/SectionEditor/Update'),
      );
      if (commit === 'blur') {
        await editor.propertyInput('Caption').click();
      } else {
        await input.press(commit);
      }
      const response = await updated;
      expect(response.ok(), await response.text()).toBeTruthy();
      await expect(input).toHaveValue(fieldName);
      await editor.save();
      await editor.reopen();
      await editor.select(textBox);
      await expect(editor.propertyInput('Value')).toHaveValue(fieldName);
      const savedTextBox = await editor.readSavedWidget('AsTextBox');
      expect(propertyValue(savedTextBox, 'Value')).toBe(fieldName);
    });
  }

  test('dropdown selection replaces a manual field binding', async ({ page }) => {
    await editor.openNew('AllDataTypes');
    const textBox = await editor.dropWidget('AsTextBox', 'Text1');
    await editor.select(textBox);
    await editor.propertyInput('Value').fill('CalculatedField');
    const updated = page.waitForResponse(response =>
      response.url().includes('/SectionEditor/Update'),
    );
    await editor.propertyInput('Value').press('Enter');
    const response = await updated;
    expect(response.ok(), await response.text()).toBeTruthy();
    await expect(editor.propertyInput('Value')).toHaveValue('CalculatedField');

    await editor.chooseProperty('Value', 'Text2');
    await expect(editor.propertyInput('Value')).toHaveValue('Text2');
    await editor.save();
    await editor.reopen();
    await editor.select(textBox);
    await expect(editor.propertyInput('Value')).toHaveValue('Text2');
    const savedTextBox = await editor.readSavedWidget('AsTextBox');
    expect(propertyValue(savedTextBox, 'Value')).toBe('Text2');
  });

  test('Escape cancels a manual field binding edit', async () => {
    await editor.openNew('AllDataTypes');
    const textBox = await editor.dropWidget('AsTextBox', 'Text1');
    await editor.select(textBox);
    await editor.propertyInput('Value').fill('CalculatedField');
    await editor.propertyInput('Value').press('Escape');
    await editor.propertyInput('Caption').click();
    await expect(editor.propertyInput('Value')).toHaveValue('Text1');
    await editor.save();
    const savedTextBox = await editor.readSavedWidget('AsTextBox');
    expect(propertyValue(savedTextBox, 'Value')).toBe('Text1');
  });

  test('clearing a field binding removes it from the editor model', async ({ page }) => {
    await editor.openNew('AllDataTypes');
    const textBox = await editor.dropWidget('AsTextBox', 'Text1');
    await editor.select(textBox);
    await editor.propertyInput('Value').fill('');
    const updated = page.waitForResponse(response =>
      response.url().includes('/SectionEditor/Update'),
    );
    await editor.propertyInput('Value').press('Tab');
    const response = await updated;
    expect(response.ok(), await response.text()).toBeTruthy();
    await expect(editor.propertyInput('Value')).toHaveValue('');
    const rootControl = (await response.json()).data.rootControl as ApiControl;
    expect(propertyValue(findControl(rootControl, textBox.id)!, 'Value')).toBeNull();
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
    const editable = 'rgb(247, 246, 250)';
    const grey = 'rgb(233, 232, 240)';
    await editor.openNew('AllDataTypes');
    const tagInput = await editor.dropWidget('TagInput', 'TagInput');
    const tagInputBox = editor.component(tagInput).locator('[class*="editorBox"]');
    await expect(tagInputBox).toHaveCSS('background-color', editable);
    await editor.select(tagInput);
    await editor.checkProperty('ReadOnly');
    await expect(tagInputBox).toHaveCSS('background-color', grey);

    const comboBox = await editor.dropWidget('AsCombo', 'refTagInputSourceId');
    const comboBoxBox = editor.component(comboBox).locator('[class*="editorBox"]');
    await expect(comboBoxBox.locator('svg').filter({ visible: true })).toHaveCount(1);
    await editor.select(comboBox);
    await editor.checkProperty('ReadOnly');
    await expect(comboBoxBox).toHaveCSS('background-color', grey);
    await expect(comboBoxBox.locator('svg').filter({ visible: true })).toHaveCount(0);
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

  test('widgets keep the web client look', async ({ page }) => {
    await editor.openNew('AllDataTypes');
    const textBox = await editor.dropWidget('AsTextBox', 'Text1');
    await editor.select(textBox);
    await editor.setProperty('CaptionLength', '20');
    await editor.dropWidget('AsCombo', 'refTagInputSourceId');
    await editor.dropWidget('AsDateBox', 'Date1');
    await editor.dropWidget('AsCheckBox', 'Boolean1');
    const groupBox = await editor.dropWidget('GroupBox');
    await editor.select(groupBox);
    await editor.setProperty('Text', 'Group');
    await editor.dropWidget('AsTextBox', 'Text2', groupBox);

    // The surface fills the window, so its size differs between machines.
    const surface = (await editor.surface.boundingBox())!;
    await expect(page).toHaveScreenshot('section-widgets.png', {
      clip: { x: surface.x, y: surface.y, width: 520, height: 420 },
    });
  });

  test('navigation panel follows the panel settings', async () => {
    await editor.openNew('AllDataTypes');
    const navigationPanel = editor.navigationPanel;
    const addButton = navigationPanel.getByTestId('navigation-panel-add');
    const copyButton = navigationPanel.getByTestId('navigation-panel-copy');
    const deleteButton = navigationPanel.getByTestId('navigation-panel-delete');
    await expect(addButton).toHaveCount(0);
    await expect(deleteButton).toHaveCount(0);

    await editor.selectPanel();
    await editor.setProperty('PanelTitle', 'Orders');
    await expect(navigationPanel).toContainText('Orders');
    await editor.checkProperty('ShowNewButton');
    await expect(addButton).toBeVisible();
    await expect(copyButton).toBeVisible();
    await editor.checkProperty('HideCopyButton');
    await expect(copyButton).toHaveCount(0);
    await editor.checkProperty('ShowDeleteButton');
    await expect(deleteButton).toBeVisible();
    await editor.checkProperty('HideNavigationPanel');
    await expect(navigationPanel).toHaveCount(0);
  });

  test('tab order mode orders the fields in the order they are clicked', async ({ page }) => {
    await editor.openNew('AllDataTypes');
    const text1 = await editor.dropWidget('AsTextBox', 'Text1');
    const text2 = await editor.dropWidget('AsTextBox', 'Text2');
    const date1 = await editor.dropWidget('AsDateBox', 'Date1');
    const tabOrderButton = page.getByTestId('tab-order-button');
    await tabOrderButton.click();
    await expect(tabOrderButton).toHaveText('Finish Tab Order');

    for (const control of [date1, text2, text1]) {
      await editor.clickInTabOrder(control);
    }
    await expect(editor.tabOrderBadge(date1)).toHaveText('0');
    await expect(editor.tabOrderBadge(text2)).toHaveText('1');
    await expect(editor.tabOrderBadge(text1)).toHaveText('2');
    await expect(editor.tabOrderBadge(text1)).toHaveClass(/assigned/u);

    await tabOrderButton.click();
    await expect(tabOrderButton).toHaveText('Tab Order');
    await expect(editor.tabOrderBadge(text1)).toHaveCount(0);
    await editor.save();
    const section = await editor.readSection();
    expect(propertyValue(findControl(section, date1.id)!, 'TabIndex')).toBe(0);
    expect(propertyValue(findControl(section, text2.id)!, 'TabIndex')).toBe(1);
    expect(propertyValue(findControl(section, text1.id)!, 'TabIndex')).toBe(2);
  });

  test('Ctrl+click in tab order mode continues from the clicked field', async ({ page }) => {
    await editor.openNew('AllDataTypes');
    const text1 = await editor.dropWidget('AsTextBox', 'Text1');
    const text2 = await editor.dropWidget('AsTextBox', 'Text2');
    const date1 = await editor.dropWidget('AsDateBox', 'Date1');
    const tabOrderButton = page.getByTestId('tab-order-button');
    await tabOrderButton.click();
    for (const control of [text1, text2, date1]) {
      await editor.clickInTabOrder(control);
    }
    await expect(editor.tabOrderBadge(date1)).toHaveText('2');

    await tabOrderButton.click();
    await tabOrderButton.click();
    await expect(editor.tabOrderBadge(text1)).not.toHaveClass(/assigned/u);
    await editor.clickInTabOrder(text1, ['Control']);
    await editor.clickInTabOrder(date1);
    await expect(editor.tabOrderBadge(text1)).toHaveText('0');
    await expect(editor.tabOrderBadge(date1)).toHaveText('1');
    await expect(editor.tabOrderBadge(text2)).toHaveText('2');
    await expect(editor.tabOrderBadge(text2)).not.toHaveClass(/assigned/u);

    await editor.save();
    const section = await editor.readSection();
    expect(propertyValue(findControl(section, text1.id)!, 'TabIndex')).toBe(0);
    expect(propertyValue(findControl(section, date1.id)!, 'TabIndex')).toBe(1);
    expect(propertyValue(findControl(section, text2.id)!, 'TabIndex')).toBe(2);
  });
});
