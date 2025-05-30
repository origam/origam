import {
  EditorProperty,
  toChanges
} from "src/components/editors/gridEditor/EditorProperty.ts";
import { IUpdatePropertiesResult } from "src/API/IArchitectApi.ts";
import {
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";

export class DocumentationEditorState extends GridEditorState {

  get label() {
    return "";
  }

  * save(): Generator<Promise<any>, void, any> {
    try {
      this.isSaving = true;
      yield this.architectApi.persistDocumentationChanges(this.editorNode.origamId);
      if (this.editorNode.parent) {
        yield* this.editorNode.parent.loadChildren();
      }
      this._isDirty = false;
    } finally {
      this.isSaving = false;
    }
  }

  * onPropertyUpdated(property: EditorProperty, value: any): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    property.value = value;
    const changes = toChanges(this.properties);
    const updateResult = (yield this.architectApi.updateDocumentationProperties(this.editorNode.origamId, changes)) as IUpdatePropertiesResult;
    this._isDirty = updateResult.isDirty;
  }
}