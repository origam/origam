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
import fs from 'node:fs';
import { activatePackage } from '@support/activatePackage';
import { modelFilePath, resetBackend } from '@support/resetBackend';

const PACKAGE = 'AutomaticTests';
const CONSTANTS_DIR = 'AutomaticTests/DataConstant';

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
const ENTITY_GROUP = ref('d79cfa79-5f2d-4e9f-a655-0bb55c8db553', 'AutomaticTests');
const CONSTANTS_ROOT = ref('Origam.Schema.EntityModel.DataConstantSchemaItemProvider', 'Constants');
// Belongs to Root, which AutomaticTests references.
const FOREIGN_CONSTANT = ref('e42f864f-5018-4967-abdc-5910439adc9a', 'InitialUserCreated');
const FOREIGN_GROUP = ref('9e00cfe2-ad80-40f2-aeb2-321dcc57325e', 'Attachments');
const SCREEN_SECTION = ref('263a4b50-3920-4445-84c7-4df5b3065d74', 'DimensionEntityRelation');
const SECTION_GROUP = ref('d3087181-fe7b-48dd-85b9-97a4f17f3b6d', 'Dimensions');
const SECTIONS_PROVIDER = 'Origam.Schema.GuiModel.PanelSchemaItemProvider';

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
    // The constant already sits there, only a copy is left.
    expect(verdicts.get(key(CONSTANTS_ROOT))).toEqual({
      canMove: false,
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

  test('An item of another package can only be copied', async ({ request }) => {
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
    expect(fs.existsSync(modelFilePath(`${CONSTANTS_DIR}/UngroupedConstant.origam`))).toBe(true);
  });

  test('A move to the current location is refused', async ({ request }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: UNGROUPED_CONSTANT, target: CONSTANTS_ROOT, isCopy: false },
    });

    expect(response.ok()).toBeFalsy();
    expect(fs.existsSync(modelFilePath(`${CONSTANTS_DIR}/UngroupedConstant.origam`))).toBe(true);
  });

  test('Copying an item of another package writes it into the active one', async ({ request }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: FOREIGN_CONSTANT, target: CONSTANT_GROUP, isCopy: true },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    const { node } = (await response.json()) as { node: { nodeText: string } };
    expect(node.nodeText).toBe('Copy of InitialUserCreated');

    expect(
      fs.existsSync(
        modelFilePath(`${CONSTANTS_DIR}/AutomaticTests/Copy of InitialUserCreated.origam`),
      ),
    ).toBe(true);
  });

  test('Moving a constant into a group of another package repackages it', async ({ request }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: UNGROUPED_CONSTANT, target: FOREIGN_GROUP, isCopy: false },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    expect(
      fs.existsSync(modelFilePath('Root/DataConstant/Attachments/UngroupedConstant.origam')),
    ).toBe(true);
    expect(fs.existsSync(modelFilePath(`${CONSTANTS_DIR}/UngroupedConstant.origam`))).toBe(false);
    // The group belongs to Root, the source package must not grow a copy of it.
    expect(fs.existsSync(modelFilePath(`${CONSTANTS_DIR}/Attachments`))).toBe(false);
  });

  test('Copying a constant into a group of another package writes it there', async ({
    request,
  }) => {
    const response = await request.post('/Model/MoveNode', {
      data: { source: UNGROUPED_CONSTANT, target: FOREIGN_GROUP, isCopy: true },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    expect(
      fs.existsSync(
        modelFilePath('Root/DataConstant/Attachments/Copy of UngroupedConstant.origam'),
      ),
    ).toBe(true);
    expect(fs.existsSync(modelFilePath(`${CONSTANTS_DIR}/UngroupedConstant.origam`))).toBe(true);
  });

  test('Move targets of a constant are the provider and the constant groups', async ({
    request,
  }) => {
    const { targets, isSourceInActivePackage } = await getMoveTargets(request, UNGROUPED_CONSTANT);

    expect(isSourceInActivePackage).toBe(true);

    expect(targets.get(key(CONSTANTS_ROOT))).toMatchObject({
      canMove: false,
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

  test('Move targets of an item of another package are copy only', async ({ request }) => {
    const { targets, isSourceInActivePackage } = await getMoveTargets(request, FOREIGN_CONSTANT);

    expect(isSourceInActivePackage).toBe(false);

    expect(targets.size).toBeGreaterThan(0);
    for (const target of targets.values()) {
      expect(target).toMatchObject({ canMove: false, canCopy: true });
    }
  });

  test('A group has no move targets', async ({ request }) => {
    const { targets } = await getMoveTargets(request, CONSTANT_GROUP);

    expect(targets.size).toBe(0);
  });

  test('A copied screen section gets its own widget', async ({ request }) => {
    await loadProviderItems(request, SECTIONS_PROVIDER);

    const response = await request.post('/Model/MoveNode', {
      data: { source: SCREEN_SECTION, target: SECTION_GROUP, isCopy: true },
    });

    expect(response.ok(), await response.text()).toBeTruthy();
    expect(
      fs.existsSync(
        modelFilePath('Root/PanelControlSet/Dimensions/Copy of DimensionEntityRelation.origam'),
      ),
    ).toBe(true);
    // Without its own ControlItem the copy cannot be placed on a screen.
    expect(
      fs.existsSync(modelFilePath('Root/Control/Copy of DimensionEntityRelation.origam')),
    ).toBe(true);
  });

  test('A repeated copy gets a numbered name', async ({ request }) => {
    await copyIntoGroup(request);
    const second = await copyIntoGroup(request);

    expect(second).toBe('Copy of UngroupedConstant (2)');
    expect(
      fs.existsSync(
        modelFilePath(`${CONSTANTS_DIR}/AutomaticTests/Copy of UngroupedConstant (2).origam`),
      ),
    ).toBe(true);
  });
});

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
}

interface MoveTarget {
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
): Promise<MoveTargetsResult> {
  const response = await request.post('/Model/GetMoveTargets', {
    data: { source },
  });
  expect(response.ok(), await response.text()).toBeTruthy();

  const result = (await response.json()) as {
    targets: MoveTarget[];
    isSourceInActivePackage: boolean;
  };
  return {
    targets: new Map(result.targets.map(target => [target.key, target])),
    isSourceInActivePackage: result.isSourceInActivePackage,
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
