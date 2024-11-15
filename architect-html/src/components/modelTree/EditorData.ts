import {
  EditorType,
  IApiEditorNode,
  IApiEditorData
} from "src/API/IArchitectApi.ts";
import {
  TreeNode
} from "src/components/modelTree/TreeNode.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import {
  EditorProperty
} from "src/components/editors/gridEditor/GridEditorState.ts";

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
  isPersisted: boolean;
  node: EditorNode;
  properties: EditorProperty[];
  constructor(data: IApiEditorData, parent: TreeNode | null) {
    this.parentNodeId = data.parentNodeId;
    this.isPersisted = data.isPersisted;
    this.node = new EditorNode(data.node, parent);
    this.properties = data.properties.map(property => new EditorProperty(property));
  }
}

