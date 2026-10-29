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

import { expect, test, type Page } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { clearAiScript, deleteChatThreads, setAiScript } from '@support/aiScript';
import { readModelFile, resetBackend } from '@support/resetBackend';

test.describe.configure({ mode: 'serial' });

const SECTION_ID = '3ce8ac9f-3ecd-428b-9931-9fb63ff4f7dd';
const ROOT_PANEL_ID = 'ac90c5dc-6fcf-428f-8991-55fbaf193efe';
const SECTION_FILE = 'Widgets/PanelControlSet/Widgets/ArrayTest.origam';
const FIELD_NAME = 'RecordUpdatedBy';
const FIELD_CAPTION = 'Updated By';
const SCRIPTED_ANSWER = 'The scripted model added the widget.';

async function openArchitectWithSection(page: Page) {
  await page.goto('/');
  await expect(page.getByTestId('ai-input')).toBeVisible();
  await expect(page.locator('#root')).toContainText('Screen Section Editor: ArrayTest');
}

test.describe('AI adds a widget to a screen section', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, 'Widgets');
    await deleteChatThreads(request);
    await request.post('/Tab/Open', { data: { schemaItemId: SECTION_ID } });
  });

  test.afterEach(async ({ request }) => {
    await clearAiScript(request);
  });

  test('creates, saves and shows the widget the scripted model asked for', async ({
    page,
    request,
  }) => {
    expect(readModelFile(SECTION_FILE)).not.toContain(`pbi:value="${FIELD_NAME}"`);
    await setAiScript(request, {
      steps: [
        {
          toolCalls: [
            {
              name: 'SectionEditor-PostSectioneditorUpdate',
              arguments: { schemaItemId: SECTION_ID, modelChanges: [] },
            },
          ],
        },
        {
          toolCalls: [
            {
              name: 'SectionEditor-PostSectioneditorCreateitem',
              arguments: {
                editorSchemaItemId: SECTION_ID,
                parentControlSetItemId: ROOT_PANEL_ID,
                componentType: 'Origam.Gui.Win.AsTextBox',
                fieldName: FIELD_NAME,
                top: 120,
                left: 108,
              },
            },
          ],
        },
        {
          toolCalls: [
            {
              name: 'SectionEditor-PostSectioneditorSave',
              arguments: { schemaItemId: SECTION_ID },
            },
          ],
        },
        { text: SCRIPTED_ANSWER },
      ],
    });

    await openArchitectWithSection(page);
    const designSurface = page.getByTestId('design-surface');
    await expect(designSurface.getByText(FIELD_CAPTION)).toHaveCount(0);

    await page.getByTestId('ai-input').fill(`Add a text box for ${FIELD_NAME} and save.`);
    await page.getByTestId('ai-send').click();
    await expect(
      page.locator('[data-test-id="ai-message"]').filter({ hasText: SCRIPTED_ANSWER }).last(),
    ).toBeVisible();

    await expect(designSurface.getByText(FIELD_CAPTION)).toBeVisible();
    expect(readModelFile(SECTION_FILE)).toContain(`pbi:value="${FIELD_NAME}"`);
  });
});
