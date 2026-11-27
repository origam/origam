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

import { expect, test, type APIRequestContext, type APIResponse } from '@playwright/test';
import { activatePackage } from '@support/activatePackage';
import { expectModelFile } from '@support/modelTree';
import { readModelFile, resetBackend } from '@support/resetBackend';

const PACKAGE = 'AutomaticTests';
const CONSTANTS_DIR = 'AutomaticTests/DataConstant';
const FIELD_PACKAGE = 'Widgets';
const FIELD_NAME = 'asi:name="Boolean1"';
const FIELD_ENTITY_FILE = 'Widgets/DataEntity/Widgets/AllDataTypes.origam';
const FOREIGN_ENTITY_FILE = 'Root/DataEntity/Synchronization/OrigamSyncSystem.origam';
const FOREIGN_ENTITY_EXTENSION_FILE = 'Widgets/DataEntity/Synchronization/OrigamSyncSystem.origam';

interface NodeRef {
  id: string;
  nodeText: string;
  isNonPersistentItem: boolean;
}

function ref(id: string, nodeText: string): NodeRef {
  return { id, nodeText, isNonPersistentItem: false };
}

const UNGROUPED_CONSTANT = ref('024f30d7-64c1-43b7-8e0e-5d185597ae45', 'UngroupedConstant');
const CONSTANT_GROUP = ref('3fb9c2c0-6254-4733-9b49-6058514b1451', 'AutomaticTests');
const SECOND_CONSTANT_GROUP = ref('650ea494-be88-4372-be1a-339115778b2f', 'AutomaticTestsGroup');
const ENTITY_GROUP = ref('d79cfa79-5f2d-4e9f-a655-0bb55c8db553', 'AutomaticTests');
const CONSTANTS_ROOT = ref('Origam.Schema.EntityModel.DataConstantSchemaItemProvider', 'Constants');
// Belongs to Root, which AutomaticTests references.
const FOREIGN_CONSTANT = ref('e42f864f-5018-4967-abdc-5910439adc9a', 'InitialUserCreated');
const FOREIGN_GROUP = ref('9e00cfe2-ad80-40f2-aeb2-321dcc57325e', 'Attachments');
const SECOND_FOREIGN_GROUP = ref('578dbd96-c916-49d7-885c-bc4a7905f712', 'Model');
const THIRD_PACKAGE = 'Widgets';
const THIRD_PACKAGE_GROUP = ref('b95b6f28-62c5-46bb-ab27-6b9555e46856', 'Widgets');
const SCREEN_SECTION = ref('263a4b50-3920-4445-84c7-4df5b3065d74', 'DimensionEntityRelation');
const SECTION_GROUP = ref('d3087181-fe7b-48dd-85b9-97a4f17f3b6d', 'Dimensions');
const SECTIONS_PROVIDER = 'Origam.Schema.GuiModel.PanelSchemaItemProvider';
const FIELD = ref('f5997d8d-384f-4a20-9dc3-490bdc05f791', 'Boolean1');
const FIELD_ENTITY = ref('7a2338f2-7290-4cec-b0ad-2c0ee6386885', 'AllDataTypes');
const FOREIGN_ENTITY = ref('fe4b0866-624b-43b4-8975-5dcbd769e0d1', 'OrigamSyncSystem');
const WIDELY_USED_ENTITY = ref('457a4391-e94f-424c-89d0-dc3804121ce6', 'BusinessPartner');
const ROOT_PACKAGE = 'Root';
const ROOT_ENTITY_GROUP = ref('016bee9c-18e5-47bd-97cb-76eebabeb911', 'Dimensions');
const FIELD_ENTITY_GROUP = ref('4d3cf570-0e2a-42a2-8cf7-a8b17318be3d', 'Widgets');
const CHAT_ENTITY = ref('478da4a8-dc69-42bb-b10a-7fab3eaa8d6d', 'OrigamChatMessage');
const LOOKUP = ref('4b72f11a-4513-4e19-a8f5-addf40a277b1', 'AllDataTypes');
const LOOKUP_FILE = 'Widgets/DataLookup/Widgets/AllDataTypes.origam';
const LOOKUP_GROUP = ref('d685fdb1-6fc7-45ed-b5f8-aedee76ad905', 'Widgets');
const ROOT_LOOKUP_GROUP = ref('4a317fd6-56da-4d3d-8f2f-6022d4ce83f5', 'Synchronization');
const LISTED_ITEMS_LIMIT = 10;

test.describe('Model tree move rules (real backend)', () => {
  test.describe.configure({ mode: 'default', timeout: 45_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, PACKAGE);
  });

  test('Only matching containers accept a constant', async ({ request }) => {
    const verdicts = await getMoveVerdicts(request, UNGROUPED_CONSTANT, [
      CONSTANT_GROUP,
      CONSTANTS_ROOT,
      ENTITY_GROUP,
      UNGROUPED_CONSTANT,
    ]);

    expect(verdicts.get(key(CONSTANT_GROUP))).toEqual({
      canMove: true,
      canCopy: true,
    });
    expect(verdicts.get(key(CONSTANTS_ROOT))).toEqual({
      canMove: true,
      canCopy: true,
    });
    // A group of entities holds a different root item type.
    expect(verdicts.get(key(ENTITY_GROUP))).toEqual({
      canMove: false,
      canCopy: false,
    });
    expect(verdicts.get(key(UNGROUPED_CONSTANT))).toEqual({
      canMove: false,
      canCopy: false,
    });
  });

  test('An item of another package can only be copied by drag and drop', async ({ request }) => {
    const verdicts = await getMoveVerdicts(request, FOREIGN_CONSTANT, [
      CONSTANT_GROUP,
      CONSTANTS_ROOT,
    ]);

    expect(verdicts.get(key(CONSTANT_GROUP))).toEqual({
      canMove: false,
      canCopy: true,
    });
    expect(verdicts.get(key(CONSTANTS_ROOT))).toEqual({
      canMove: false,
      canCopy: true,
    });
  });

  test('A group is not a movable item', async ({ request }) => {
    const verdicts = await getMoveVerdicts(request, CONSTANT_GROUP, [CONSTANTS_ROOT]);

    expect(verdicts.get(key(CONSTANTS_ROOT))).toEqual({
      canMove: false,
      canCopy: false,
    });
  });

  test('A rejected move is refused and changes nothing', async ({ request }) => {
    const response = await request.post('/Model/MoveNode', {
      data: {
        source: UNGROUPED_CONSTANT,
        target: ENTITY_GROUP,
        isCopy: false,
      },
    });

    expect(response.ok()).toBeFalsy();
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, true);
  });

  test('A move to the current location keeps the item where it is', async ({ request }) => {
    await moveNode(request, UNGROUPED_CONSTANT, CONSTANTS_ROOT);

    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, true);
  });

  test('Copying an item of another package writes it into the active one', async ({ request }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: FOREIGN_CONSTANT, target: CONSTANT_GROUP, isCopy: true },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    const { node } = (await response.json()) as { node: { nodeText: string } };
    expect(node.nodeText).toBe('Copy of InitialUserCreated');

    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTests/Copy of InitialUserCreated.origam`,
      true,
    );
  });

  test('Moving a constant into a group of another package keeps its package', async ({
    request,
  }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: UNGROUPED_CONSTANT, target: FOREIGN_GROUP, isCopy: false },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    await expectModelFile(`${CONSTANTS_DIR}/Attachments/UngroupedConstant.origam`, true);
    await expectModelFile(`${CONSTANTS_DIR}/Attachments/.origamGroupReference`, true);
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, false);
    await expectModelFile('Root/DataConstant/Attachments/UngroupedConstant.origam', false);
  });

  test('Copying a constant into a group of another package writes it into the active one', async ({
    request,
  }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: UNGROUPED_CONSTANT, target: FOREIGN_GROUP, isCopy: true },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    await expectModelFile(`${CONSTANTS_DIR}/Attachments/Copy of UngroupedConstant.origam`, true);
    await expectModelFile('Root/DataConstant/Attachments/Copy of UngroupedConstant.origam', false);
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, true);
  });

  test('A field moved into an entity of another package keeps its package and moves back', async ({
    request,
  }) => {
    await activatePackage(request, FIELD_PACKAGE);

    await moveNode(request, FIELD, FOREIGN_ENTITY);
    await expectModelFile(FOREIGN_ENTITY_EXTENSION_FILE, true);
    expect(readModelFile(FOREIGN_ENTITY_EXTENSION_FILE)).toContain(FIELD_NAME);
    expect(readModelFile(FOREIGN_ENTITY_FILE)).not.toContain(FIELD_NAME);
    expect(readModelFile(FIELD_ENTITY_FILE)).not.toContain(FIELD_NAME);

    const { targets, isSourceInActivePackage } = await getMoveTargets(request, FIELD);
    expect(isSourceInActivePackage).toBe(true);
    expect(targets.size).toBeGreaterThan(0);
    const verdicts = await getMoveVerdicts(request, FIELD, [FIELD_ENTITY]);
    expect(verdicts.get(key(FIELD_ENTITY))).toEqual({ canMove: true, canCopy: true });

    await moveNode(request, FIELD, FIELD_ENTITY);
    expect(readModelFile(FIELD_ENTITY_FILE)).toContain(FIELD_NAME);
    expect(readModelFile(FOREIGN_ENTITY_FILE)).not.toContain(FIELD_NAME);
  });

  test('Move targets of a constant are the provider and the constant groups', async ({
    request,
  }) => {
    const { targets, isSourceInActivePackage } = await getMoveTargets(request, UNGROUPED_CONSTANT);

    expect(isSourceInActivePackage).toBe(true);

    expect(targets.get(key(CONSTANTS_ROOT))).toMatchObject({
      canMove: true,
      canCopy: true,
      path: 'Constants',
      depth: 0,
      isCurrentLocation: true,
    });
    expect(targets.get(key(CONSTANT_GROUP))).toMatchObject({
      canMove: true,
      canCopy: true,
      path: 'Constants/AutomaticTests',
      depth: 1,
    });
    expect(targets.get(key(FOREIGN_GROUP))).toMatchObject({
      canMove: true,
      canCopy: true,
      packageName: 'Root',
    });
    expect(targets.has(key(ENTITY_GROUP))).toBe(false);
    // DataConstant has no CanMove override, so no constant can become a parent.
    expect(targets.has(key(FOREIGN_CONSTANT))).toBe(false);
  });

  test('An item of another package can be moved only into the folders of its package', async ({
    request,
  }) => {
    const { targets, isSourceInActivePackage, packageRefusal } = await getMoveTargets(
      request,
      FOREIGN_CONSTANT,
    );

    expect(isSourceInActivePackage).toBe(false);
    expect(packageRefusal).toBeNull();
    expect(targets.get(key(FOREIGN_GROUP))).toMatchObject({ canMove: true, canCopy: true });
    expect(targets.get(key(CONSTANT_GROUP))).toMatchObject({ canMove: false, canCopy: true });
    expect(targets.get(key(CONSTANTS_ROOT))).toMatchObject({ canMove: false, canCopy: true });
  });

  test('A move with another package changes the package and takes it back', async ({ request }) => {
    const rootId = await findPackageId(request, ROOT_PACKAGE);
    const ownId = await findPackageId(request, PACKAGE);
    const { targets, packageRefusal } = await getMoveTargets(request, UNGROUPED_CONSTANT, rootId);

    expect(packageRefusal).toBeNull();
    expect(targets.get(key(FOREIGN_GROUP))).toMatchObject({ canMove: true, canCopy: true });
    expect(targets.has(key(CONSTANT_GROUP))).toBe(false);
    expect(targets.has(key(CONSTANTS_ROOT))).toBe(false);

    await moveNode(request, UNGROUPED_CONSTANT, FOREIGN_GROUP, rootId);
    await expectModelFile('Root/DataConstant/Attachments/UngroupedConstant.origam', true);
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, false);

    await moveNode(request, UNGROUPED_CONSTANT, CONSTANT_GROUP, ownId);
    await expectModelFile(`${CONSTANTS_DIR}/AutomaticTests/UngroupedConstant.origam`, true);
    await expectModelFile('Root/DataConstant/Attachments/UngroupedConstant.origam', false);
  });

  test('A package change needs a folder of the new package', async ({ request }) => {
    const rootId = await findPackageId(request, ROOT_PACKAGE);
    const response = await request.post('/Model/MoveNode', {
      data: {
        source: UNGROUPED_CONSTANT,
        target: CONSTANTS_ROOT,
        isCopy: false,
        packageId: rootId,
      },
    });

    expect(response.ok()).toBeFalsy();
    expect(await errorMessage(response)).toContain(
      "'UngroupedConstant' cannot be moved to 'Constants'",
    );
    await expectModelFile(`${CONSTANTS_DIR}/UngroupedConstant.origam`, true);
  });

  test('An item of another package is not moved into a folder outside its package', async ({
    request,
  }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: FOREIGN_CONSTANT, target: CONSTANT_GROUP, isCopy: false },
    });

    expect(response.ok()).toBeFalsy();
    expect(await errorMessage(response)).toContain("'InitialUserCreated' belongs to package 'Root'");
    await expectModelFile('Root/DataConstant/InitialUserCreated.origam', true);
  });

  test('An item of another package moves into a folder of its package', async ({ request }) => {
    await moveNode(request, FOREIGN_CONSTANT, FOREIGN_GROUP);

    await expectModelFile('Root/DataConstant/Attachments/InitialUserCreated.origam', true);
    await expectModelFile('Root/DataConstant/InitialUserCreated.origam', false);
    await expectModelFile(`${CONSTANTS_DIR}/Attachments/InitialUserCreated.origam`, false);
  });

  test('An item of another package is given to a third package', async ({ request }) => {
    const thirdPackageId = await findPackageId(request, THIRD_PACKAGE);
    const { targets, packageRefusal } = await getMoveTargets(
      request,
      FOREIGN_CONSTANT,
      thirdPackageId,
    );

    expect(packageRefusal).toBeNull();
    expect(targets.get(key(THIRD_PACKAGE_GROUP))).toMatchObject({ canMove: true });

    await moveNode(request, FOREIGN_CONSTANT, THIRD_PACKAGE_GROUP, thirdPackageId);
    await expectModelFile('Widgets/DataConstant/Widgets/InitialUserCreated.origam', true);
    await expectModelFile('Root/DataConstant/InitialUserCreated.origam', false);
  });

  test('A package change is refused when a package that does not see it uses the item', async ({
    request,
  }) => {
    const ownId = await findPackageId(request, PACKAGE);
    const { targets, packageRefusal } = await getMoveTargets(request, FOREIGN_ENTITY, ownId);

    expect(packageRefusal).toBe(
      [
        '"OrigamSyncSystem" cannot be moved to package "AutomaticTests" because it is used by items of packages that do not reference "AutomaticTests" (Root):',
        'Entity "LookupOrigamSyncSystem/OrigamSyncSystem" (Root)',
        'Database Field "OrigamSyncConnection/refOrigamSyncSystemId" (Root)',
        'Database Field "OrigamSyncProvider/refOrigamSyncSystemId" (Root)',
      ].join('\n'),
    );
    expect(targets.size).toBeGreaterThan(0);
    for (const target of targets.values()) {
      expect(target.canMove).toBe(false);
    }
  });

  test('A refusal lists the first items and counts the rest', async ({ request }) => {
    const ownId = await findPackageId(request, PACKAGE);
    const { packageRefusal } = await getMoveTargets(request, WIDELY_USED_ENTITY, ownId);

    const [reason, ...items] = (packageRefusal ?? '').split('\n');
    expect(reason).toMatch(
      /^"BusinessPartner" cannot be moved to package "AutomaticTests" because it is used by items of packages that do not reference "AutomaticTests" \(.*Root.*\):$/,
    );
    expect(items).toHaveLength(LISTED_ITEMS_LIMIT + 1);
    for (const item of items.slice(0, LISTED_ITEMS_LIMIT)) {
      expect(item).toMatch(/^[A-Za-z ]+ ".+" \(.+\)$/);
    }
    expect(items[LISTED_ITEMS_LIMIT]).toMatch(/^and \d+ more$/);
    expect(packageRefusal).not.toMatch(/[0-9a-f]{8}-[0-9a-f]{4}-/);
  });

  test('A package change is refused when the item uses a package the new one does not reference', async ({
    request,
  }) => {
    await activatePackage(request, FIELD_PACKAGE);
    const rootId = await findPackageId(request, ROOT_PACKAGE);
    const { targets, packageRefusal } = await getMoveTargets(request, LOOKUP, rootId);

    expect(packageRefusal).toBe(
      [
        '"AllDataTypes" cannot be moved to package "Root" because it uses items of packages that "Root" does not reference (Widgets):',
        'Data Structure "LookupAllDataTypes" (Widgets)',
        'Filter Set "LookupAllDataTypes/GetId" (Widgets)',
      ].join('\n'),
    );
    expect(targets.get(key(ROOT_LOOKUP_GROUP))).toMatchObject({ canMove: false });

    const response = await request.post('/Model/MoveNode', {
      data: { source: LOOKUP, target: ROOT_LOOKUP_GROUP, isCopy: false, packageId: rootId },
    });

    expect(response.ok()).toBeFalsy();
    expect(await errorMessage(response)).toBe(packageRefusal);
    await expectModelFile(LOOKUP_FILE, true);
    await expectModelFile('Root/DataLookup/Synchronization/AllDataTypes.origam', false);
  });

  test('A refused folder names the item that cannot change its package', async ({ request }) => {
    await activatePackage(request, FIELD_PACKAGE);
    const rootId = await findPackageId(request, ROOT_PACKAGE);
    const { packageRefusal } = await getMoveTargets(request, LOOKUP_GROUP, rootId);

    expect(packageRefusal).toMatch(
      /^"[^"]+" cannot be moved to package "Root" because it uses items of packages that "Root" does not reference \(Widgets\):\n/,
    );

    const response = await request.post('/Model/MoveNode', {
      data: { source: LOOKUP_GROUP, target: ROOT_LOOKUP_GROUP, isCopy: false, packageId: rootId },
    });

    expect(response.ok()).toBeFalsy();
    expect(await errorMessage(response)).toBe(packageRefusal);
    await expectModelFile(LOOKUP_FILE, true);
    await expectModelFile('Widgets/DataLookup/Widgets/.origamGroup', true);
  });

  test('An entity is given to a package that does not reference what its fields use', async ({
    request,
  }) => {
    await activatePackage(request, FIELD_PACKAGE);
    const rootId = await findPackageId(request, ROOT_PACKAGE);
    const copyResponse = await request.post('/Model/MoveNode', {
      data: { source: CHAT_ENTITY, target: FIELD_ENTITY_GROUP, isCopy: true },
    });
    expect(copyResponse.ok(), await copyResponse.text()).toBeTruthy();
    const { node } = (await copyResponse.json()) as {
      node: { origamId: string; nodeText: string };
    };
    const copy = ref(node.origamId, node.nodeText);

    const { targets, packageRefusal } = await getMoveTargets(request, copy, rootId);
    expect(packageRefusal).toBeNull();
    expect(targets.get(key(ROOT_ENTITY_GROUP))).toMatchObject({ canMove: true });

    await moveNode(request, copy, ROOT_ENTITY_GROUP, rootId);
    const movedFile = 'Root/DataEntity/Dimensions/Copy of OrigamChatMessage.origam';
    await expectModelFile(movedFile, true);
    await expectModelFile('Widgets/DataEntity/Widgets/Copy of OrigamChatMessage.origam', false);
    expect(readModelFile(movedFile)).toContain('Chat/DataEntity/Chat/OrigamChatRoom.origam');
  });

  test('A folder moves with its items into a folder of another package', async ({ request }) => {
    const rootId = await findPackageId(request, ROOT_PACKAGE);
    const { targets, packageRefusal } = await getMoveTargets(request, CONSTANT_GROUP, rootId);

    expect(packageRefusal).toBeNull();
    expect(targets.get(key(FOREIGN_GROUP))).toMatchObject({ canMove: true, canCopy: false });

    await moveNode(request, CONSTANT_GROUP, FOREIGN_GROUP, rootId);
    await expectModelFile('Root/DataConstant/Attachments/AutomaticTests/.origamGroup', true);
    await expectModelFile(
      'Root/DataConstant/Attachments/AutomaticTests/GroupedConstant.origam',
      true,
    );
    await expectModelFile(`${CONSTANTS_DIR}/AutomaticTests/GroupedConstant.origam`, false);
  });

  test('A folder cannot be copied', async ({ request }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: CONSTANT_GROUP, target: SECOND_CONSTANT_GROUP, isCopy: true },
    });

    expect(response.ok()).toBeFalsy();
    await expectModelFile(`${CONSTANTS_DIR}/AutomaticTests/GroupedConstant.origam`, true);
    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTestsGroup/AutomaticTests/GroupedConstant.origam`,
      false,
    );
  });

  test('A field cannot be given to a package that does not see its entity', async ({
    request,
  }) => {
    await activatePackage(request, FIELD_PACKAGE);
    const rootId = await findPackageId(request, ROOT_PACKAGE);
    const { targets } = await getMoveTargets(request, FIELD, rootId);

    expect(targets.size).toBe(0);
  });

  test('A folder of another package moves into another folder of its package', async ({
    request,
  }) => {
    const { targets, packageRefusal } = await getMoveTargets(request, FOREIGN_GROUP);

    expect(packageRefusal).toBeNull();
    expect(targets.get(key(SECOND_FOREIGN_GROUP))).toMatchObject({
      canMove: true,
      canCopy: false,
    });
    expect(targets.has(key(CONSTANT_GROUP))).toBe(false);

    await moveNode(request, FOREIGN_GROUP, SECOND_FOREIGN_GROUP);
    await expectModelFile('Root/DataConstant/Model/Attachments/.origamGroup', true);
    await expectModelFile('Root/DataConstant/Model/Attachments/Attachment_Compress.origam', true);
    await expectModelFile('Root/DataConstant/Attachments/.origamGroup', false);
  });

  test('A folder of another package is not moved into a folder outside its package', async ({
    request,
  }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: FOREIGN_GROUP, target: CONSTANT_GROUP, isCopy: false },
    });

    expect(response.ok()).toBeFalsy();
    expect(await errorMessage(response)).toContain("'Attachments' belongs to package 'Root'");
    await expectModelFile('Root/DataConstant/Attachments/.origamGroup', true);
  });

  test('A folder is never its own target', async ({ request }) => {
    const { targets } = await getMoveTargets(request, CONSTANT_GROUP);

    expect(targets.has(key(CONSTANT_GROUP))).toBe(false);
  });

  test('A field copied into its own entity gets a new name', async ({ request }) => {
    await activatePackage(request, FIELD_PACKAGE);
    const response = await request.post('/Model/MoveNode', {
      data: { source: FIELD, target: FIELD_ENTITY, isCopy: true },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    const { node } = (await response.json()) as { node: { nodeText: string } };
    expect(node.nodeText).toBe('Copy of Boolean1');
    expect(readModelFile(FIELD_ENTITY_FILE)).toContain('asi:name="Copy of Boolean1"');
  });

  test('A copied screen section gets its own widget', async ({ request }) => {
    await loadProviderItems(request, SECTIONS_PROVIDER);

    const response = await request.post('/Model/MoveNode', {
      data: { source: SCREEN_SECTION, target: SECTION_GROUP, isCopy: true },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    await expectModelFile(
      'AutomaticTests/PanelControlSet/Dimensions/Copy of DimensionEntityRelation.origam',
      true,
    );
    // Without its own ControlItem the copy cannot be placed on a screen.
    await expectModelFile('AutomaticTests/Control/Copy of DimensionEntityRelation.origam', true);
  });

  test('A repeated copy gets another Copy of', async ({ request }) => {
    await copyIntoGroup(request);
    const second = await copyIntoGroup(request);

    expect(second).toBe('Copy of Copy of UngroupedConstant');
    await expectModelFile(
      `${CONSTANTS_DIR}/AutomaticTests/Copy of Copy of UngroupedConstant.origam`,
      true,
    );
  });
});

async function moveNode(
  request: APIRequestContext,
  source: NodeRef,
  target: NodeRef,
  packageId?: string,
) {
  const response = await request.post('/Model/MoveNode', {
    data: { source, target, isCopy: false, packageId },
  });
  expect(response.ok(), await response.text()).toBeTruthy();
}

async function errorMessage(response: APIResponse): Promise<string> {
  const { message } = (await response.json()) as { message: string };
  return message;
}

async function findPackageId(request: APIRequestContext, packageName: string): Promise<string> {
  const response = await request.get('/Package/GetAll');
  expect(response.ok(), await response.text()).toBeTruthy();
  const { packages } = (await response.json()) as { packages: { id: string; name: string }[] };
  const found = packages.find(candidate => candidate.name === packageName);
  if (!found) {
    throw new Error(`Package "${packageName}" was not found in the test model.`);
  }
  return found.id;
}

async function copyIntoGroup(request: APIRequestContext): Promise<string> {
  const response = await request.post('/Model/MoveNode', {
    data: { source: UNGROUPED_CONSTANT, target: CONSTANT_GROUP, isCopy: true },
  });
  expect(response.ok(), await response.text()).toBeTruthy();
  const { node } = (await response.json()) as { node: { nodeText: string } };
  return node.nodeText;
}

// An item only learns its provider while the provider lists it.
async function loadProviderItems(request: APIRequestContext, providerId: string): Promise<void> {
  const response = await request.get('/Model/GetChildren', {
    params: { id: providerId, isNonPersistentItem: false, nodeText: '' },
  });
  expect(response.ok(), await response.text()).toBeTruthy();
}

function key(node: NodeRef): string {
  return node.id + node.nodeText;
}

interface MoveTargetsResult {
  targets: Map<string, MoveTarget>;
  isSourceInActivePackage: boolean;
  packageRefusal: string | null;
}

interface MoveTarget {
  id: string;
  nodeText: string;
  key: string;
  path: string;
  depth: number;
  packageName: string;
  isCurrentLocation: boolean;
  canMove: boolean;
  canCopy: boolean;
}

async function getMoveTargets(
  request: APIRequestContext,
  source: NodeRef,
  packageId?: string,
): Promise<MoveTargetsResult> {
  const response = await request.post('/Model/GetMoveTargets', {
    data: { source, packageId },
  });
  expect(response.ok(), await response.text()).toBeTruthy();

  const result = (await response.json()) as {
    targets: MoveTarget[];
    isSourceInActivePackage: boolean;
    packageRefusal: string | null;
  };
  return {
    targets: new Map(result.targets.map(target => [target.key, target])),
    isSourceInActivePackage: result.isSourceInActivePackage,
    packageRefusal: result.packageRefusal,
  };
}

async function getMoveVerdicts(
  request: APIRequestContext,
  source: NodeRef,
  targets: NodeRef[],
): Promise<Map<string, { canMove: boolean; canCopy: boolean }>> {
  const response = await request.post('/Model/GetMoveVerdicts', {
    data: { source, targets },
  });
  expect(response.ok(), await response.text()).toBeTruthy();

  const results = (await response.json()) as {
    key: string;
    canMove: boolean;
    canCopy: boolean;
  }[];
  return new Map(
    results.map(result => [result.key, { canMove: result.canMove, canCopy: result.canCopy }]),
  );
}
