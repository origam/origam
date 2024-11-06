import { EditorType, IApiEditorNode } from "src/API/IArchitectApi.ts";
import {
  TreeNode
} from "src/components/lazyLoadedTree/TreeNode.ts";
import { IEditorNode } from "src/stores/IEditorManager.ts";

export class NewEditorNode implements IEditorNode {
  id: string;
  origamId: string;
  nodeText: string;
  editorType: EditorType;
  parent: TreeNode | null = null;

  constructor(node: IApiEditorNode, parent: TreeNode) {
    this.id = node.id;
    this.origamId = node.origamId;
    this.nodeText = node.nodeText;
    this.editorType = node.editorType;
    this.parent = parent;
  }
}