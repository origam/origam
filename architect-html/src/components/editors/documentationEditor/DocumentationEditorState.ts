import {
  EditorProperty,
  toChanges
} from "src/components/editors/gridEditor/EditorProperty.ts";
import {
  DocumentationEditorData,
  IArchitectApi,
  IUpdatePropertiesResult
} from "src/API/IArchitectApi.ts";
import {
  GridEditorState
} from "src/components/editors/gridEditor/GridEditorState.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";

export class DocumentationEditorState extends GridEditorState {

  constructor(
    editorId: string,
    editorNode: IEditorNode,
    private documentationData: DocumentationEditorData,
    isDirty: boolean,
    architectApi: IArchitectApi
  ) {
    const properties = documentationData.properties.map(property => new EditorProperty(property));
    super(editorId, editorNode, properties, isDirty, architectApi);
  }

  get label() {
    return `[${this.documentationData.label}]`;
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