import { action, observable } from "mobx";
import { Editor, getEditor } from "src/components/editors/GetEditor.tsx";
import {
  IApiEditorNode,
  IArchitectApi,
  IApiEditorData
} from "src/API/IArchitectApi.ts";
import {
  EditorData
} from "src/components/modelTree/EditorData.ts";

import { TreeNode } from "src/components/modelTree/TreeNode.ts";
import { RootStore } from "src/stores/RootStore.ts";
import { askYesNoQuestion, YesNoResult } from "src/dialog/DialogUtils.tsx";


export class EditorTabViewState {
  @observable accessor editors: Editor[] = [];
  architectApi: IArchitectApi;

  constructor(private rootStore: RootStore) {
    this.architectApi = this.rootStore.architectApi;
  }

  * initializeOpenEditors(): Generator<Promise<IApiEditorData[]>, void, IApiEditorData[]> {
    const openEditorsData = (yield this.architectApi.getOpenEditors()) as IApiEditorData[];
    this.editors = openEditorsData
      .map(data => this.toEditor(data)) as Editor[];
    if (this.editors.length > 0) {
      this.setActiveEditor(this.editors[this.editors.length - 1].state.schemaItemId);
    }
  }

  private toEditor(data: IApiEditorData) {
    const treeNode = this.rootStore.modelTreeState.findNodeById(data.node.id);
    const editorData = new EditorData(data, treeNode);

    return getEditor({
      editorData: editorData,
      architectApi: this.architectApi
    });
  }

  async openEditorById(node: TreeNode){
    const apiEditorData = await this.architectApi.openEditor(node.origamId);
    const editorData = new EditorData(apiEditorData, node);
    this.openEditor(editorData);
  }

  @action.bound
  openEditor(editorData: EditorData): void {
    const alreadyOpenEditor = this.editors
      .find(editor => editor.state.schemaItemId === editorData.node.origamId);
    if (alreadyOpenEditor) {
      this.setActiveEditor(alreadyOpenEditor.state.schemaItemId);
      return;
    }

    const editor = getEditor({
      editorData: editorData,
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