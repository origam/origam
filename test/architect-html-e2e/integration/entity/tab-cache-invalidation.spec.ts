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

import { expect, test, type APIRequestContext } from '@playwright/test';
import { readModelFile, resetBackend } from '@support/resetBackend';

test.describe.configure({ mode: 'serial' });

const API_PACKAGE_ID = 'dc245ea1-d7d1-4405-a2e8-a7ca9d6a1946';
const API_MAIN_ENTITY_ID = 'eb3abda0-f5ed-4c69-a25c-29492f0b4ffc';
const API_MAIN_ENTITY_FILE = 'Api/DataEntity/Api/ApiMainEntity.origam';
const VIRTUAL_FIELD_TYPE = 'Origam.Schema.EntityModel.DetachedField';

async function createPersistedField(
  request: APIRequestContext,
  fieldName: string,
): Promise<string> {
  const response = await request.post('/Tab/CreateNode', {
    data: {
      nodeId: API_MAIN_ENTITY_ID,
      newTypeName: VIRTUAL_FIELD_TYPE,
      changes: [{ name: 'Name', value: fieldName }],
      persist: true,
    },
  });
  expect(response.ok(), await response.text()).toBe(true);
  const openTabData = await response.json();
  return openTabData.node.origamId;
}

async function readOpenTabs(request: APIRequestContext): Promise<{ node: { text: string } }[]> {
  const response = await request.get('/Tab/GetOpen');
  expect(response.ok(), await response.text()).toBe(true);
  return await response.json();
}

async function updateProperty(
  request: APIRequestContext,
  schemaItemId: string,
  propertyName: string,
  value: string,
): Promise<void> {
  const response = await request.post('/PropertyEditor/Update', {
    data: { schemaItemId: schemaItemId, changes: [{ name: propertyName, value: value }] },
  });
  expect(response.ok(), await response.text()).toBe(true);
}

async function persistChanges(request: APIRequestContext, schemaItemId: string): Promise<void> {
  const response = await request.post('/Tab/PersistChanges', {
    data: { schemaItemId: schemaItemId },
  });
  expect(response.ok(), await response.text()).toBe(true);
}

test.describe('Editor tab cache against changes made outside the tab (real backend)', () => {
  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    const setActive = await request.post('/Package/SetActive', {
      data: { id: API_PACKAGE_ID },
    });
    expect(setActive.ok(), await setActive.text()).toBe(true);
  });

  test('creating and saving an item in one call leaves no editor tab behind', async ({
    request,
  }) => {
    await createPersistedField(request, 'ProbeTabLeak');

    expect(await readOpenTabs(request)).toHaveLength(0);
  });

  test('saving an entity does not roll back a field renamed outside its tab', async ({
    request,
  }) => {
    const fieldId = await createPersistedField(request, 'FieldBeforeRename');

    const openEntityTab = await request.post('/Tab/Open', {
      data: { schemaItemId: API_MAIN_ENTITY_ID },
    });
    expect(openEntityTab.ok(), await openEntityTab.text()).toBe(true);

    await updateProperty(request, fieldId, 'Name', 'FieldAfterRename');
    await persistChanges(request, fieldId);

    const afterRename = readModelFile(API_MAIN_ENTITY_FILE);
    expect(afterRename).toContain('FieldAfterRename');
    expect(afterRename).not.toContain('FieldBeforeRename');

    await updateProperty(request, API_MAIN_ENTITY_ID, 'Caption', 'EntityCaptionProbe');
    await persistChanges(request, API_MAIN_ENTITY_ID);

    const afterEntitySave = readModelFile(API_MAIN_ENTITY_FILE);
    expect(afterEntitySave).toContain('EntityCaptionProbe');
    expect(afterEntitySave).toContain('FieldAfterRename');
    expect(afterEntitySave).not.toContain('FieldBeforeRename');
  });
});
