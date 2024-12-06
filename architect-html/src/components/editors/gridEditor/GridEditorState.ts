import { computed, observable } from "mobx";
import {
  IArchitectApi,
  IUpdatePropertiesResult,
} from "src/API/IArchitectApi.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import {
  EditorProperty,
  toChanges
} from "src/components/editors/gridEditor/EditorProperty.ts";

import {
  IPropertyManager
} from "src/components/editors/propertyEditor/IPropertyManager.tsx";

export class GridEditorState implements IEditorState, IPropertyManager {
  @observable accessor properties: EditorProperty[];
  @observable accessor isSaving = false;
  @observable accessor isActive = false;
  @observable accessor _isDirty: boolean;

  constructor(
    private editorNode: IEditorNode,
    properties: EditorProperty[] | undefined,
    isDirty: boolean,
    private architectApi: IArchitectApi
  ) {
    this._isDirty = isDirty;
    this.properties = properties ?? [];
  }

  @computed
  get isDirty() {
    let someHasError = false;
    for (const property of this.properties) {
      someHasError = someHasError || !!property.error;
    }
    return this._isDirty && !someHasError;
  }

  get label() {
    return this.properties.find(x => x.name === "Name")?.value || "";
  }

  get schemaItemId() {
    return this.editorNode.origamId;
  }

  * save(): Generator<Promise<any>, void, any> {
    try {
      this.isSaving = true;
      yield this.architectApi.persistChanges(this.editorNode.origamId);
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
    const updateResult = (yield this.architectApi.updateProperties(this.editorNode.origamId, changes)) as IUpdatePropertiesResult;
    for (const property of this.properties) {
      const propertyUpdate = updateResult.propertyUpdates
        .find(update => property.name === update.propertyName)
      property.update(propertyUpdate);
    }
    this._isDirty = updateResult.isDirty;
  }
}

