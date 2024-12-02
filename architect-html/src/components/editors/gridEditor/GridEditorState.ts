import { computed, observable } from "mobx";
import { IArchitectApi, IPropertyUpdate, } from "src/API/IArchitectApi.ts";
import {
  IEditorNode
} from "src/components/editorTabView/EditorTabViewState.ts";
import { IEditorState } from "src/components/editorTabView/IEditorState.ts";
import {
  EditorProperty, toChanges
} from "src/components/editors/gridEditor/EditorProperty.ts";

export class GridEditorState implements IEditorState {
  @observable accessor properties: EditorProperty[];
  @observable accessor isSaving = false;
  @observable accessor isActive = false;
  @observable accessor isPersisted: boolean;

  constructor(
    private editorNode: IEditorNode,
    properties: EditorProperty[] | undefined,
    isPersisted: boolean,
    private architectApi: IArchitectApi
  ) {
    this.properties = properties ?? [];
    this.isPersisted = isPersisted;
  }

  @computed
  get isDirty() {
    let someIsDirty = false;
    let someHasError = false;
    for (const property of this.properties) {
      someIsDirty = someIsDirty || property.isDirty;
      someHasError = someHasError || !!property.error;
    }
    return someIsDirty && !someHasError;
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
      yield this.architectApi.persistChanges(this.editorNode.origamId, this.properties);
      this.isPersisted = true;
      for (const property of this.properties) {
        property.isDirty = false;
      }
      if (this.editorNode.parent) {
        yield* this.editorNode.parent.loadChildren();
      }
    } catch (error) {
      console.error('Failed to save content', error);
    } finally {
      this.isSaving = false;
    }
  }

  * onPropertyUpdated(property: EditorProperty, value: any): Generator<Promise<IPropertyUpdate[]>, void, IPropertyUpdate[]> {
    property.value = value;
    const changes = toChanges(this.properties);
    const propertyUpdates = (yield this.architectApi.updateProperties(this.editorNode.origamId, changes)) as IPropertyUpdate[];
    for (const property of this.properties) {
      const propertyUpdate = propertyUpdates.find(update => property.name === update.propertyName)
      property.update(propertyUpdate);
    }
  }
}

