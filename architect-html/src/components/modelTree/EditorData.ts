import {
  EditorType,
  IApiEditorNode,
  IApiEditorData, IApiEditorProperty, ISectionEditorData
} from "src/API/IArchitectApi.ts";
import {
  TreeNode
} from "src/components/modelTree/TreeNode.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";

export class EditorNode implements IEditorNode {
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

export class EditorData implements IApiEditorData {
  parentNodeId: string | undefined;
  isDirty: boolean;
  node: EditorNode;
  data: IApiEditorProperty[] | ISectionEditorData;
  constructor(data: IApiEditorData, parent: TreeNode | null) {
    this.parentNodeId = data.parentNodeId;
    this.isDirty = data.isDirty;
    this.node = new EditorNode(data.node, parent);
    this.data = data.data;
  }
}

