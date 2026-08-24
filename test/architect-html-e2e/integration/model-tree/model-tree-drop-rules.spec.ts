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

test.describe('Model tree drop rules (real backend)', () => {
  test.describe.configure({ mode: 'default', timeout: 45_000 });

  test.beforeEach(async ({ request }) => {
    await resetBackend(request);
    await activatePackage(request, PACKAGE);
  });

  test('Only matching containers accept a constant', async ({ request }) => {
    const verdicts = await getDropTargets(request, UNGROUPED_CONSTANT, [
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

  test('An item of another package can only be copied', async ({ request }) => {
    const verdicts = await getDropTargets(request, FOREIGN_CONSTANT, [
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
    const verdicts = await getDropTargets(request, CONSTANT_GROUP, [CONSTANTS_ROOT]);

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
    const targets = await getMoveTargets(request, UNGROUPED_CONSTANT);

    expect(targets.get(key(CONSTANTS_ROOT))).toMatchObject({
      canMove: true,
      canCopy: true,
      kind: 'Provider',
    });
    expect(targets.get(key(CONSTANT_GROUP))).toMatchObject({
      canMove: true,
      canCopy: true,
      kind: 'Group',
    });
    expect(targets.get(key(FOREIGN_GROUP))).toMatchObject({
      canMove: true,
      canCopy: true,
      packageName: 'Root',
    });
    expect(targets.has(key(ENTITY_GROUP))).toBe(false);
    // DataConstant has no CanMove override, so no item can become its parent.
    expect([...targets.values()].some(target => target.kind === 'Item')).toBe(false);
  });

  test('Move targets of an item of another package are copy only', async ({ request }) => {
    const targets = await getMoveTargets(request, FOREIGN_CONSTANT);

    expect(targets.size).toBeGreaterThan(0);
    for (const target of targets.values()) {
      expect(target).toMatchObject({ canMove: false, canCopy: true });
    }
  });

  test('A group has no move targets', async ({ request }) => {
    const targets = await getMoveTargets(request, CONSTANT_GROUP);

    expect(targets.size).toBe(0);
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

function key(node: NodeRef): string {
  return node.id + node.nodeText;
}

interface MoveTarget {
  key: string;
  kind: string;
  packageName: string;
  canMove: boolean;
  canCopy: boolean;
}

async function getMoveTargets(
  request: APIRequestContext,
  source: NodeRef,
): Promise<Map<string, MoveTarget>> {
  const response = await request.post('/Model/GetMoveTargets', {
    data: { source },
  });
  expect(response.ok(), await response.text()).toBeTruthy();

  const { targets } = (await response.json()) as { targets: MoveTarget[] };
  return new Map(targets.map(target => [target.key, target]));
}

async function getDropTargets(
  request: APIRequestContext,
  source: NodeRef,
  targets: NodeRef[],
): Promise<Map<string, { canMove: boolean; canCopy: boolean }>> {
  const response = await request.post('/Model/GetDropTargets', {
    data: { source, targets },
  });
  expect(response.ok(), await response.text()).toBeTruthy();

  const results = (await response.json()) as {
    id: string;
    canMove: boolean;
    canCopy: boolean;
  }[];
  return new Map(
    results.map(result => [result.id, { canMove: result.canMove, canCopy: result.canCopy }]),
  );
}
