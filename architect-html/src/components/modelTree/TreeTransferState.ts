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

import { T } from '@/main';
import { IDropTargetResult, IMoveNodeResult, INodeLoadData } from '@api/IArchitectApi';
import { TreeNode, toNodeRef } from '@components/modelTree/TreeNode';
import { askYesNoQuestion, YesNoResult } from '@dialogs/DialogUtils';
import { RootStore } from '@stores/RootStore';
import { observable } from 'mobx';

export type TransferMode = 'cut' | 'copy';

interface IDropFlags {
  canMove: boolean;
  canCopy: boolean;
}

// Clipboard and drag are the same operation with a different delay. The backend
// decides whether a drop is legal, dropTargets caches it for synchronous dragover.
export class TreeTransferState {
  @observable accessor mode: TransferMode | null = null;
  @observable accessor sourceNodeId: string | null = null;
  @observable accessor isDragging: boolean = false;
  @observable accessor isCopyModifier: boolean = false;
  @observable accessor hoverNodeId: string | null = null;
  @observable.ref accessor dropTargets: Map<string, IDropFlags> = new Map();
  @observable.ref private accessor sourceRef: INodeLoadData | null = null;

  constructor(private rootStore: RootStore) {}

  get hasSource(): boolean {
    return this.sourceRef !== null;
  }

  isSource(node: TreeNode): boolean {
    return this.sourceNodeId === node.id;
  }

  canDropOn(node: TreeNode, isCopy: boolean): boolean {
    const flags = this.dropTargets.get(node.id);
    if (!flags) {
      return false;
    }
    return isCopy ? flags.canCopy : flags.canMove;
  }

  isDropHighlighted(node: TreeNode): boolean {
    return (
      this.isDragging &&
      this.hoverNodeId === node.id &&
      this.canDropOn(node, this.isCopyModifier)
    );
  }

  *beginTransfer(
    node: TreeNode,
    mode: TransferMode,
  ): Generator<Promise<IDropTargetResult[]>, void, IDropTargetResult[]> {
    this.mode = mode;
    this.sourceNodeId = node.id;
    this.sourceRef = toNodeRef(node);
    this.dropTargets = new Map();
    yield* this.loadDropTargets(this.rootStore.modelTreeState.visibleNodes);
  }

  *addDropTargets(
    nodes: TreeNode[],
  ): Generator<Promise<IDropTargetResult[]>, void, IDropTargetResult[]> {
    if (!this.sourceRef) {
      return;
    }
    yield* this.loadDropTargets(nodes.filter(node => !this.dropTargets.has(node.id)));
  }

  private *loadDropTargets(
    nodes: TreeNode[],
  ): Generator<Promise<IDropTargetResult[]>, void, IDropTargetResult[]> {
    if (!this.sourceRef || nodes.length === 0) {
      return;
    }
    const results = yield this.rootStore.architectApi.getDropTargets({
      source: this.sourceRef,
      targets: nodes.map(toNodeRef),
    });
    const updated = new Map(this.dropTargets);
    for (const result of results) {
      updated.set(result.id, { canMove: result.canMove, canCopy: result.canCopy });
    }
    this.dropTargets = updated;
  }

  *drop(target: TreeNode, isCopy: boolean): Generator<Promise<any>, boolean, any> {
    // Captured before the first yield, the drop handler clears the state on return.
    const sourceRef = this.sourceRef;
    const source = this.rootStore.modelTreeState.findNodeById(this.sourceNodeId ?? undefined);
    if (!sourceRef) {
      return false;
    }
    if (!isCopy && !(yield* this.confirmUnsavedChanges(source))) {
      return false;
    }

    const result: IMoveNodeResult = yield this.rootStore.architectApi.moveNode({
      source: sourceRef,
      target: toNodeRef(target),
      isCopy: isCopy,
    });

    yield* this.refreshAfterMove(source, target, result);
    if (!isCopy) {
      this.clear();
    }
    return true;
  }

  // A move persists the same in memory instance, an open unsaved edit would go with it.
  private *confirmUnsavedChanges(source: TreeNode | null): Generator<Promise<any>, boolean, any> {
    if (!source) {
      return true;
    }
    const dirtyEditors = this.rootStore.editorTabViewState.editorsContainers.filter(
      editor => editor.state.isDirty && editor.state.tabId.includes(source.origamId),
    );
    if (dirtyEditors.length === 0) {
      return true;
    }

    const answer = yield askYesNoQuestion(
      this.rootStore.dialogStack,
      T('Save changes', 'tree_move_save_changes_title'),
      T(
        'Moving "{0}" saves the item. Do you want to save the changes you have open in its editor?',
        'tree_move_save_changes_question',
        source.nodeText,
      ),
    );
    if (answer !== YesNoResult.Yes) {
      return false;
    }
    for (const editor of dirtyEditors) {
      yield* editor.state.save();
    }
    return true;
  }

  private *refreshAfterMove(
    source: TreeNode | null,
    target: TreeNode,
    result: IMoveNodeResult,
  ): Generator<Promise<any>, void, any> {
    const modelTreeState = this.rootStore.modelTreeState;
    const oldParent = source?.parent ?? null;
    if (oldParent) {
      yield* oldParent.loadChildren.bind(oldParent)();
    }
    if (target.childrenInitialized && target !== oldParent) {
      yield* target.loadChildren.bind(target)();
    }

    yield* modelTreeState.expandAndHighlightSchemaItem.bind(modelTreeState)({
      parentNodeIds: result.parentNodeIds,
      schemaItemId: result.node.origamId,
    });
    modelTreeState.selectNode(modelTreeState.findNodeById(result.node.origamId));
  }

  clear() {
    this.mode = null;
    this.sourceNodeId = null;
    this.sourceRef = null;
    this.isDragging = false;
    this.isCopyModifier = false;
    this.hoverNodeId = null;
    this.dropTargets = new Map();
  }
}
