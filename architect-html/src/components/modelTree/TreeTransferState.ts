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
import { IMoveNodeResult, IMoveVerdict, INodeLoadData } from '@api/IArchitectApi';
import { TreeNode, toNodeRef } from '@components/modelTree/TreeNode';
import { askYesNoQuestion, YesNoResult } from '@dialogs/DialogUtils';
import { RootStore } from '@stores/RootStore';
import { action, observable, when } from 'mobx';

export type TransferMode = 'cut' | 'copy';

// A deeply expanded tree holds thousands of nodes.
const VERDICT_BATCH_SIZE = 250;

interface ITransferVerdict {
  canMove: boolean;
  canCopy: boolean;
}

export class TreeTransferState {
  @observable accessor clipboardMode: TransferMode | null = null;
  @observable accessor sourceNodeId: string | null = null;
  @observable accessor isDragging: boolean = false;
  @observable accessor isCopyModifier: boolean = false;
  @observable accessor hoverNodeId: string | null = null;
  @observable accessor isBusy: boolean = false;
  @observable private accessor pendingVerdictLoads: number = 0;
  @observable.ref accessor targetVerdicts: Map<string, ITransferVerdict> = new Map();
  @observable.ref private accessor sourceRef: INodeLoadData | null = null;
  @observable accessor isDropping: boolean = false;
  private generation = 0;
  // Kept apart from targetVerdicts so overlapping callers do not ask twice.
  private requestedTargetIds = new Set<string>();

  constructor(private rootStore: RootStore) {}

  // A counter, not a flag - a finished request must not hide the ones still running.
  get isLoadingVerdicts(): boolean {
    return this.pendingVerdictLoads > 0;
  }

  get hasSource(): boolean {
    return this.sourceRef !== null;
  }

  isSource(node: TreeNode): boolean {
    return this.sourceNodeId === node.id;
  }

  isCutSource(node: TreeNode): boolean {
    if (!this.isSource(node)) {
      return false;
    }
    return this.isDragging ? !this.isCopyModifier : this.clipboardMode === 'cut';
  }

  canTransferTo(node: TreeNode, isCopy: boolean): boolean {
    const verdict = this.targetVerdicts.get(node.id);
    if (!verdict) {
      return false;
    }
    return isCopy ? verdict.canCopy : verdict.canMove;
  }

  // Offers the target while its verdict is still on the way, drop() waits for the answer
  // before it acts. Without it a fast drag would land on a node that looks forbidden.
  mayTransferTo(node: TreeNode, isCopy: boolean): boolean {
    if (!this.targetVerdicts.has(node.id)) {
      return this.isLoadingVerdicts;
    }
    return this.canTransferTo(node, isCopy);
  }

  hasVerdictsFor(nodes: TreeNode[]): boolean {
    return nodes.every(node => this.targetVerdicts.has(node.id));
  }

  isDropHighlighted(node: TreeNode): boolean {
    return (
      this.isDragging &&
      this.hoverNodeId === node.id &&
      this.mayTransferTo(node, this.isCopyModifier)
    );
  }

  @action
  beginDrag(isCopy: boolean) {
    this.isDragging = true;
    this.isCopyModifier = isCopy;
  }

  @action
  setCopyModifier(isCopy: boolean) {
    this.isCopyModifier = isCopy;
  }

  @action
  setHoverNode(nodeId: string | null) {
    this.hoverNodeId = nodeId;
  }

  *beginTransfer(
    node: TreeNode,
    mode: TransferMode,
  ): Generator<Promise<IMoveVerdict[]>, void, IMoveVerdict[]> {
    const sourceRef = toNodeRef(node);
    if (sourceRef.id !== this.sourceRef?.id || sourceRef.nodeText !== this.sourceRef?.nodeText) {
      this.targetVerdicts = new Map();
      this.requestedTargetIds.clear();
      this.generation++;
    }
    this.clipboardMode = mode;
    this.sourceNodeId = node.id;
    this.sourceRef = sourceRef;
    yield* this.loadTargetVerdicts(this.rootStore.modelTreeState.visibleNodes);
  }

  *loadTargetVerdicts(nodes: TreeNode[]): Generator<Promise<IMoveVerdict[]>, void, IMoveVerdict[]> {
    const targets = nodes.filter(node => !this.requestedTargetIds.has(node.id));
    const sourceRef = this.sourceRef;
    if (!sourceRef || targets.length === 0) {
      return;
    }
    const generation = this.generation;
    for (const target of targets) {
      this.requestedTargetIds.add(target.id);
    }
    this.pendingVerdictLoads++;
    try {
      for (let start = 0; start < targets.length; start += VERDICT_BATCH_SIZE) {
        const batch = targets.slice(start, start + VERDICT_BATCH_SIZE);
        let results: IMoveVerdict[];
        try {
          results = yield this.rootStore.architectApi.getMoveVerdicts({
            source: sourceRef,
            targets: batch.map(toNodeRef),
          });
        } catch (error) {
          // A failed request must not block a later attempt for the same targets.
          for (const target of targets.slice(start)) {
            this.requestedTargetIds.delete(target.id);
          }
          throw error;
        }
        if (generation !== this.generation) {
          return;
        }
        this.applyVerdicts(batch, results);
      }
    } finally {
      this.pendingVerdictLoads--;
    }
  }

  private applyVerdicts(targets: TreeNode[], results: IMoveVerdict[]) {
    const updated = new Map(this.targetVerdicts);
    // A target left without an answer stays rejected, otherwise callers keep asking for it.
    for (const target of targets) {
      updated.set(target.id, { canMove: false, canCopy: false });
    }
    for (const result of results) {
      updated.set(result.key, { canMove: result.canMove, canCopy: result.canCopy });
    }
    this.targetVerdicts = updated;
  }

  // Keeps the drag state alive until the drop finishes, dragend fires while it still runs.
  *dropFromDrag(target: TreeNode, isCopy: boolean): Generator<Promise<any>, boolean, any> {
    this.isDropping = true;
    try {
      return yield* this.drop(target, isCopy);
    } finally {
      this.isDropping = false;
      this.clear();
    }
  }

  *drop(target: TreeNode, isCopy: boolean): Generator<Promise<any>, boolean, any> {
    const sourceRef = this.sourceRef;
    if (!sourceRef) {
      return false;
    }
    // The target may have been offered before its verdict arrived, or never asked about.
    if (!this.targetVerdicts.has(target.id)) {
      if (this.isLoadingVerdicts) {
        yield when(() => !this.isLoadingVerdicts);
      }
      yield* this.loadTargetVerdicts([target]);
    }
    if (!this.canTransferTo(target, isCopy)) {
      return false;
    }
    const source = this.rootStore.modelTreeState.findNodeById(this.sourceNodeId ?? undefined);
    const moved = yield* this.executeMove(source, sourceRef, toNodeRef(target), target, isCopy);
    if (moved && !isCopy) {
      this.clear();
    }
    return moved;
  }

  // The target picked in the dialog does not have to be loaded in the tree, and the
  // pending cut or copy must survive.
  *moveTo(
    source: TreeNode,
    targetRef: INodeLoadData,
    isCopy: boolean,
  ): Generator<Promise<any>, boolean, any> {
    const targetNode = this.rootStore.modelTreeState.findNodeById(targetRef.id);
    return yield* this.executeMove(source, toNodeRef(source), targetRef, targetNode, isCopy);
  }

  private *executeMove(
    source: TreeNode | null,
    sourceRef: INodeLoadData,
    targetRef: INodeLoadData,
    targetNode: TreeNode | null,
    isCopy: boolean,
  ): Generator<Promise<any>, boolean, any> {
    if (this.isBusy) {
      return false;
    }

    this.isBusy = true;
    try {
      if (!isCopy && !(yield* this.confirmUnsavedChanges(source, sourceRef))) {
        return false;
      }

      const result: IMoveNodeResult = yield this.rootStore.architectApi.moveNode({
        source: sourceRef,
        target: targetRef,
        isCopy: isCopy,
      });

      yield* this.refreshAfterMove(source, targetNode, result);
      return true;
    } finally {
      this.isBusy = false;
    }
  }

  // Persist cascades to child items, so unsaved edits in the subtree get written too.
  private *confirmUnsavedChanges(
    source: TreeNode | null,
    sourceRef: INodeLoadData,
  ): Generator<Promise<any>, boolean, any> {
    const movedIds = source ? this.collectSubtreeIds(source) : [sourceRef.id];
    const dirtyEditors = this.rootStore.editorTabViewState.editorsContainers.filter(
      editor => editor.state.isDirty && movedIds.some(id => editor.state.tabId.includes(id)),
    );
    if (dirtyEditors.length === 0) {
      return true;
    }

    const answer = yield askYesNoQuestion(
      this.rootStore.dialogStack,
      T('Save changes', 'tree_move_save_changes_title'),
      T(
        'Moving "{0}" saves it together with its children. Do you want to save the changes you have open in their editors?',
        'tree_move_save_changes_question',
        sourceRef.nodeText,
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

  private collectSubtreeIds(node: TreeNode): string[] {
    return [node.origamId, ...node.children.flatMap(child => this.collectSubtreeIds(child))];
  }

  private *refreshAfterMove(
    source: TreeNode | null,
    target: TreeNode | null,
    result: IMoveNodeResult,
  ): Generator<Promise<any>, void, any> {
    const modelTreeState = this.rootStore.modelTreeState;
    const oldParent = source?.parent ?? null;
    if (oldParent) {
      yield* oldParent.loadChildren.bind(oldParent)();
    }
    if (target?.childrenInitialized && target !== oldParent) {
      yield* target.loadChildren.bind(target)();
    }

    yield* modelTreeState.expandAndHighlightSchemaItem.bind(modelTreeState)({
      parentNodeIds: result.parentNodeIds,
      schemaItemId: result.node.origamId,
    });
    modelTreeState.selectNode(modelTreeState.findNodeById(result.node.origamId));
  }

  @action
  endDrag() {
    if (this.isDropping) {
      return;
    }
    this.clear();
  }

  @action
  clear() {
    this.generation++;
    this.clipboardMode = null;
    this.sourceNodeId = null;
    this.sourceRef = null;
    this.isDragging = false;
    this.isCopyModifier = false;
    this.hoverNodeId = null;
    this.targetVerdicts = new Map();
    this.requestedTargetIds.clear();
  }
}
