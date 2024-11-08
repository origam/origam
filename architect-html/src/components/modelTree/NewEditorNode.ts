import { EditorType, IApiEditorNode } from "src/API/IArchitectApi.ts";
import {
  TreeNode
} from "src/components/modelTree/TreeNode.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";

export class NewEditorNode implements IEditorNode {
  id: string;
  origamId: string;
  nodeText: string;
  editorType: EditorType;
  parent: TreeNode | null = null;

  constructor(node: IApiEditorNode, parent: TreeNode | null) {
    this.id = node.id;
    this.origamId = node.origamId;
    this.nodeText = node.nodeText;
    this.editorType = node.editorType;
    this.parent = parent;
  }
}