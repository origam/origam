import {
  EditorProperty,
  EditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import React from "react";
import { IApiEditorNode } from "src/API/IArchitectApi.ts";
import { TreeNode } from "src/components/lazyLoadedTree/TreeNode.ts";

export interface IEditorManager {
  openEditor(node: IEditorNode, properties?: EditorProperty[]): void;

  get activeEditorState(): EditorState | undefined;

  get activeEditor(): React.ReactElement | undefined;
}

export interface IEditorNode extends IApiEditorNode{
  parent: TreeNode | null;
}

