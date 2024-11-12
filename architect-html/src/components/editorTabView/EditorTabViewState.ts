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
import { TreeNode } from "src/components/modelTree/TreeNode.ts";
import { RootStore } from "src/stores/RootStore.ts";
import { askYesNoQuestion, YesNoResult } from "src/dialog/DialogUtils.tsx";

export class EditorTabViewState {
  @observable accessor editors: Editor[] = [];
  architectApi: IArchitectApi;

  constructor(private rootStore: RootStore) {
    this.architectApi = this.rootStore.architectApi;
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
    const parentNode = this.rootStore.modelTreeState.findNodeById(data.parentNodeId)
    return getEditor({
      editorNode:  new NewEditorNode(data.node, parentNode),
      properties: data.properties.map(property => new EditorProperty(property)),
      isPersisted: data.isPersisted,
      architectApi: this.architectApi
    });
  }

  @action.bound
  openEditor(
    args:{
      node: IEditorNode,
      isPersisted: boolean
      properties?: EditorProperty[],
    }): void
  {
    const { node, properties, isPersisted } = args;
    const alreadyOpenEditor = this.editors.find(editor => editor.state.schemaItemId === node.origamId);
    if (alreadyOpenEditor) {
      this.setActiveEditor(alreadyOpenEditor.state.schemaItemId);
      return;
    }

    const editor =  getEditor({
      editorNode:  node,
      properties: properties,
      isPersisted: isPersisted,
      architectApi: this.architectApi
    });
    if (!editor) {
      return;
    }
    this.editors.push(editor)
    this.setActiveEditor(editor.state.schemaItemId);
  }

  get activeEditorState() {
    return this.editors.find(editor => editor.state.isActive)?.state;
  }

  setActiveEditor(schemaItemId: string) {
    for (const editor of this.editors) {
      editor.state.isActive = editor.state.schemaItemId === schemaItemId;
    }
  }

  closeEditor(schemaItemId: string) {
    return function* (this: EditorTabViewState): Generator<Promise<any>, void, any> {
      if (this.activeEditorState?.isDirty) {
        const saveChanges = yield askYesNoQuestion(this.rootStore.dialogStack, "Save changes", `Do you want to save ${this.activeEditorState.label}?`);
        switch (saveChanges) {
          case YesNoResult.No:
            break;
          case YesNoResult.Cancel:
            return;
          case YesNoResult.Yes:
            yield* this.activeEditorState.save();
            break;
        }
      }
      this.editors = this.editors.filter((editor: Editor) => editor.state.schemaItemId !== schemaItemId);
      yield this.architectApi.closeEditor(schemaItemId);
      if (this.editors.length > 0) {
        const editorToActivate = this.editors[this.editors.length - 1];
        this.setActiveEditor(editorToActivate.state.schemaItemId);
      }

    }.bind(this);
  }
}

export interface IEditorNode extends IApiEditorNode {
  parent: TreeNode | null;
}
