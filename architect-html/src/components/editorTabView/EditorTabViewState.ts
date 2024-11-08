import { action, observable } from "mobx";
import { Editor, getEditor } from "src/components/editors/GetEditor.tsx";
import {
  IApiEditorNode,
  IArchitectApi,
  IEditorData
} from "src/API/IArchitectApi.ts";
import { NewEditorNode } from "src/components/modelTree/NewEditorNode.ts";
import {
  EditorProperty
} from "src/components/editors/gridEditor/GridEditorState.ts";
import { IModelNodesContainer } from "src/stores/RootStore.ts";
import { TreeNode } from "src/components/modelTree/TreeNode.ts";

export class EditorTabViewState {
  @observable accessor editors: Editor[] = [];

  constructor(
    private architectApi: IArchitectApi,
    private nodesContainer: IModelNodesContainer) {
  }

  * initializeOpenEditors(): Generator<Promise<IEditorData[]>, void, IEditorData[]> {
    const openEditorsData = (yield this.architectApi.getOpenEditors()) as IEditorData[];
    this.editors = openEditorsData
      .map(data => this.toEditor(data)) as Editor[];
    if (this.editors.length > 0) {
      this.setActiveEditor(this.editors[this.editors.length - 1].state.schemaItemId);
    }
  }

  private toEditor(data: IEditorData) {
    const parentNode = this.nodesContainer.findNodeById(data.parentNodeId)
    return getEditor(
      new NewEditorNode(data.node, parentNode),
      data.properties.map(property => new EditorProperty(property)),
      this.architectApi)
  }

  @action.bound
  openEditor(node: IEditorNode, properties?: EditorProperty[]): void {
    const alreadyOpenEditor = this.editors.find(editor => editor.state.schemaItemId === node.origamId);
    if (alreadyOpenEditor) {
      this.setActiveEditor(alreadyOpenEditor.state.schemaItemId);
      return;
    }

    const editor = getEditor(node, properties, this.architectApi);
    if (!editor) {
      return;
    }
    this.editors.push(editor)
    this.setActiveEditor(editor.state.schemaItemId);
  }

  get activeEditorState() {
    return this.editors.find(editor => editor.state.isActive)?.state;
  }

  get activeEditor() {
    return this.editors.find(editor => editor.state.isActive)?.element;
  }

  setActiveEditor(schemaItemId: string) {
    for (const editor of this.editors) {
      editor.state.isActive = editor.state.schemaItemId === schemaItemId;
    }
  }

  closeEditor(schemaItemId: string) {
    return function* (this: any) {
      this.editors = this.editors.filter(editor => editor.state.schemaItemId !== schemaItemId);
      yield this.architectApi.closeEditor(schemaItemId);
    }.bind(this);
  }
}

export interface IEditorNode extends IApiEditorNode{
  parent: TreeNode | null;
}
