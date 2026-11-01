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

import { expect, type APIRequestContext, type Locator, type Page } from '@playwright/test';
import fs from 'node:fs';
import path from 'node:path';
import { closeAiPanel } from '@support/aiPanel';
import { modelFilePath, reloadBackend } from '@support/resetBackend';

export const SECTION_FILE_NAME = 'NewPanel.origam';
const WIDGET_TYPES: Record<string, string> = {
  AsCombo: 'AsDropDown',
  GroupBox: 'GroupBoxWithChamfer',
  RadioButton: 'AsRadioButton',
};

export interface ApiControl {
  id: string;
  name: string;
  type: string;
  properties: { name: string; value: unknown }[];
  children: ApiControl[];
}

export function propertyValue(control: ApiControl, name: string): unknown {
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

export function findControl(control: ApiControl, id: string): ApiControl | undefined {
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

export function findFile(directory: string, fileName: string): string | undefined {
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

export class SectionEditor {
  readonly toolbox: Locator;
  readonly surface: Locator;
  readonly navigationPanel: Locator;
  private sectionId = '';
  private dropCount = 0;

  constructor(
    private readonly page: Page,
    private readonly request: APIRequestContext,
  ) {
    this.toolbox = page.getByTestId('toolbox');
    this.surface = page.getByTestId('design-surface');
    this.navigationPanel = this.surface.getByTestId('navigation-panel');
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

  async dropWidget(
    widget: string,
    field?: string,
    container?: ApiControl,
    position?: { x: number; y: number },
  ): Promise<ApiControl> {
    if (field) {
      await this.toolboxItem(field).click();
    }
    await this.toolbox.getByText('Widgets', { exact: true }).click();
    const target = container ? this.component(container) : this.surface;
    const defaultPosition = container
      ? { x: 60, y: 30 + this.dropCount * 30 }
      : { x: 150, y: 40 + this.dropCount * 40 };
    this.dropCount += 1;
    const created = this.page.waitForResponse(response =>
      response.url().includes('/SectionEditor/CreateItem'),
    );
    await this.toolboxItem(widget).dragTo(target, {
      targetPosition: position ?? defaultPosition,
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

  async selectPanel(): Promise<void> {
    await this.navigationPanel.click();
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

  async reopen(): Promise<void> {
    await reloadBackend(this.request, 'Widgets');
    const response = await this.request.post('/Tab/Open', {
      data: { schemaItemId: this.sectionId },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
    await this.page.reload();
    await closeAiPanel(this.page);
    await expect(this.page.locator('#root')).toContainText('Screen Section Editor: NewPanel');
  }

  async readSavedWidget(widget: string): Promise<ApiControl> {
    const sectionFile = findFile(modelFilePath('Widgets/PanelControlSet'), SECTION_FILE_NAME);
    expect(sectionFile, SECTION_FILE_NAME + ' was not written').toBeTruthy();
    expect(fs.readFileSync(sectionFile!, 'utf8')).toContain('#' + widget + '/');

    const rootControl = await this.readSection();
    const shortType = WIDGET_TYPES[widget] ?? widget;
    const savedWidget = findWidget(rootControl, shortType);
    expect(savedWidget, widget + ' is not in the saved section').toBeTruthy();
    return savedWidget!;
  }

  async readSection(): Promise<ApiControl> {
    const response = await this.request.post('/SectionEditor/Update', {
      data: { schemaItemId: this.sectionId, modelChanges: [] },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
    return (await response.json()).data.rootControl as ApiControl;
  }

  private toolboxItem(name: string): Locator {
    return this.toolbox.getByText(name, { exact: true }).filter({ visible: true });
  }

  component(control: ApiControl): Locator {
    return this.surface.locator(`[class~="${control.id}"]`);
  }

  tabOrderBadge(control: ApiControl): Locator {
    return this.surface.getByTestId(`tab-order-badge-${control.id}`);
  }

  async clickInTabOrder(control: ApiControl, modifiers: 'Control'[] = []): Promise<void> {
    await this.component(control).click({ position: { x: 3, y: 3 }, modifiers });
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
