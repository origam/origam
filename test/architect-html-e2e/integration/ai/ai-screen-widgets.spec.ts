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

import { expect, test, type APIRequestContext, type Page } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { clearAiScript, deleteChatThreads, setAiScript } from '@support/aiScript';
import { readModelFile, resetBackend } from '@support/resetBackend';

test.describe.configure({ mode: 'serial' });

const AGENT_HEADERS = { 'X-Origam-Agent': 'true' };
const DATA_STRUCTURE_ID = '2c3b7bab-0f4d-4719-8846-a83cb2770ce9';
const MASTER_SECTION = 'WidgetSectionTestMaster';
const DETAIL_SECTION = 'WidgetSectionTestDetail';
const MASTER_DATA_MEMBER = 'WidgetSectionTestMaster';
const DETAIL_DATA_MEMBER = 'WidgetSectionTestMaster.WidgetSectionTestDetail';
const EXISTING_SCREEN_ID = '87ac3988-10a4-47e0-a37a-6ef0826d1bcf';
const EXISTING_SCREEN_FILE = 'Widgets/FormControlSet/Widgets/WidgetSectionTest.origam';
const EXISTING_TAB_PAGE_ID = 'aee7ae9d-6440-4752-b033-ae651ed91532';
const SCRIPTED_ANSWER = 'The scripted model added the label.';

interface ApiProperty {
  name: string;
  value: unknown;
}

interface ApiControl {
  id: string;
  properties: ApiProperty[];
  children: ApiControl[];
}

interface ScreenData {
  rootControl: ApiControl;
  dataMembers: string[];
  warnings: string[];
}

function value(control: ApiControl, name: string): unknown {
  return control.properties.find(candidate => candidate.name === name)?.value;
}

function bounds(control: ApiControl) {
  return {
    top: value(control, 'Top'),
    left: value(control, 'Left'),
    width: value(control, 'Width'),
    height: value(control, 'Height'),
  };
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

class AgentScreenApi {
  screenId = '';

  constructor(private readonly request: APIRequestContext) {}

  async createScreen(name: string): Promise<void> {
    const response = await this.request.post('/Tab/CreateNode', {
      headers: AGENT_HEADERS,
      data: {
        nodeId: 'Origam.Schema.GuiModel.FormSchemaItemProvider',
        newTypeName: 'Screen',
        changes: [{ name: 'Name', value: name }],
      },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
    this.screenId = (await response.json()).node.origamId;
  }

  async update(body: Record<string, unknown>): Promise<ScreenData> {
    const response = await this.request.post('/ScreenEditor/Update', {
      headers: AGENT_HEADERS,
      data: { schemaItemId: this.screenId, modelChanges: [], ...body },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
    return (await response.json()).data as ScreenData;
  }

  async change(widgetId: string, name: string, newValue: string): Promise<ScreenData> {
    return await this.update({
      modelChanges: [{ schemaItemId: widgetId, changes: [{ name, value: newValue }] }],
    });
  }

  async create(controlName: string, parentId?: string) {
    return await this.request.post('/ScreenEditor/CreateItem', {
      headers: AGENT_HEADERS,
      data: {
        editorSchemaItemId: this.screenId,
        parentControlSetItemId: parentId,
        controlName,
      },
    });
  }

  async createWidget(controlName: string, parentId?: string): Promise<ApiControl> {
    const response = await this.create(controlName, parentId);
    expect(response.ok(), await response.text()).toBeTruthy();
    return (await response.json()).screenItem as ApiControl;
  }

  async save(): Promise<string[]> {
    const response = await this.request.post('/ScreenEditor/Save', {
      headers: AGENT_HEADERS,
      data: { schemaItemId: this.screenId },
    });
    expect(response.ok(), await response.text()).toBeTruthy();
    return (await response.json()).warnings as string[];
  }
}

async function openArchitectWithScreen(page: Page) {
  await page.goto('/');
  await expect(page.getByTestId('ai-input')).toBeVisible();
  await expect(page.locator('#root')).toContainText('Screen Editor: WidgetSectionTest');
}

test.describe('AI builds screens from screen widgets', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
  });

  test('the server lays out and validates what the agent creates', async ({ request }) => {
    const screen = new AgentScreenApi(request);
    await screen.createScreen('AiLayoutScreen');
    const emptyScreen = await screen.update({ selectedDataSourceId: DATA_STRUCTURE_ID });
    expect(emptyScreen.dataMembers).toEqual(
      expect.arrayContaining([MASTER_DATA_MEMBER, DETAIL_DATA_MEMBER]),
    );

    const splitPanel = await screen.createWidget('SplitPanel');
    expect(bounds(splitPanel)).toEqual({ top: 0, left: 0, width: 500, height: 500 });
    const master = await screen.createWidget(MASTER_SECTION, splitPanel.id);
    const tabControl = await screen.createWidget('TabControl', splitPanel.id);
    expect(bounds(tabControl)).toEqual({ top: 255, left: 10, width: 480, height: 235 });
    expect(tabControl.children).toHaveLength(2);

    const thirdChild = await screen.create('Label', splitPanel.id);
    expect(thirdChild.ok()).toBeFalsy();
    expect(await thirdChild.text()).toContain('already has two children');
    const unknownWidget = await screen.create('AsTextBox');
    expect(unknownWidget.ok()).toBeFalsy();
    expect(await unknownWidget.text()).toContain('SplitPanel');

    const detail = await screen.createWidget(DETAIL_SECTION, tabControl.children[0].id);
    expect(bounds(detail)).toEqual({ top: 0, left: 0, width: 470, height: 210 });
    const label = await screen.createWidget('Label', tabControl.children[1].id);
    expect(bounds(label)).toMatchObject({ width: 200, height: 20 });

    const unbound = await screen.update({});
    expect(unbound.warnings.join(' ')).toContain('AsPanel1');
    expect(unbound.warnings.join(' ')).toContain('AsPanel2');
    await screen.change(master.id, 'DataMember', MASTER_DATA_MEMBER);
    await screen.change(detail.id, 'DataMember', DETAIL_DATA_MEMBER);

    const sideBySide = await screen.change(splitPanel.id, 'Orientation', '1');
    expect(bounds(findControl(sideBySide.rootControl, master.id)!)).toEqual({
      top: 10,
      left: 10,
      width: 235,
      height: 480,
    });
    expect(bounds(findControl(sideBySide.rootControl, tabControl.id)!)).toEqual({
      top: 10,
      left: 255,
      width: 235,
      height: 480,
    });

    expect(await screen.save()).toEqual([]);
    const screenXml = readModelFile('Widgets/FormControlSet/AiLayoutScreen.origam');
    for (const widget of ['SplitPanel', 'TabControl', 'TabPage', 'Label']) {
      expect(screenXml).toContain('#' + widget + '/');
    }
  });

  test('an open screen editor shows what the scripted model added', async ({ page, request }) => {
    await deleteChatThreads(request);
    await request.post('/Tab/Open', { data: { schemaItemId: EXISTING_SCREEN_ID } });
    await setAiScript(request, {
      steps: [
        {
          toolCalls: [
            {
              name: 'ScreenEditor-PostScreeneditorUpdate',
              arguments: { schemaItemId: EXISTING_SCREEN_ID, modelChanges: [] },
            },
          ],
        },
        {
          toolCalls: [
            {
              name: 'ScreenEditor-PostScreeneditorCreateitem',
              arguments: {
                editorSchemaItemId: EXISTING_SCREEN_ID,
                parentControlSetItemId: EXISTING_TAB_PAGE_ID,
                controlName: 'Label',
                top: 5,
                left: 5,
              },
            },
          ],
        },
        {
          toolCalls: [
            {
              name: 'ScreenEditor-PostScreeneditorSave',
              arguments: { schemaItemId: EXISTING_SCREEN_ID },
            },
          ],
        },
        { text: SCRIPTED_ANSWER },
      ],
    });

    try {
      await openArchitectWithScreen(page);
      const designSurface = page.getByTestId('design-surface');
      await expect(designSurface.getByText('Label', { exact: true })).toHaveCount(0);

      await page.getByTestId('ai-input').fill('Add a label to the Memo tab and save.');
      await page.getByTestId('ai-send').click();
      await expect(
        page.locator('[data-test-id="ai-message"]').filter({ hasText: SCRIPTED_ANSWER }).last(),
      ).toBeVisible();

      await designSurface.getByText('Memo', { exact: true }).click();
      await expect(designSurface.getByText('Label', { exact: true })).toBeVisible();
      expect(readModelFile(EXISTING_SCREEN_FILE)).toContain('#Label/');
    } finally {
      await clearAiScript(request);
    }
  });
});
