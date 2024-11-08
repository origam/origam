import { computed, observable } from "mobx";
import {
  ApiEditorProperty,
  DropDownValue, IArchitectApi, PropertyUpdate,
} from "src/API/IArchitectApi.ts";
import { IEditorNode } from "src/stores/IEditorManager.ts";

export class EditorState {
  constructor(
    private editorNode: IEditorNode,
    properties: EditorProperty[] | undefined,
    private architectApi: IArchitectApi
  ) {
    this.properties = properties ?? [];
  }

  @observable accessor properties: EditorProperty[];
  @observable accessor isSaving = false;
  @observable accessor isActive = false;

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

  * initialize(): Generator<Promise<ApiEditorProperty[]>, void, ApiEditorProperty[]> {
    if (this.properties.length === 0) {
      const apiProperties = yield this.architectApi.openEditor(this.editorNode.origamId);
      this.properties = apiProperties.map(apiProperty => new EditorProperty(apiProperty));
    }
  }

  * save() {
    try {
      this.isSaving = true;
      yield this.architectApi.persistChanges(this.editorNode.origamId, this.properties);
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

  * onPropertyUpdated(property: EditorProperty, value: any): Generator<Promise<PropertyUpdate[]>, void, PropertyUpdate[]> {
    property.value = value;
    const propertyUpdates = (yield this.architectApi.updateProperties(this.editorNode.origamId, this.properties)) as PropertyUpdate[];
    for (const property of this.properties) {
      const propertyUpdate = propertyUpdates.find(update => property.name === update.propertyName)
      property.update(propertyUpdate);
    }
  }
}

export class EditorProperty implements ApiEditorProperty {
  name: string;
  type: "boolean" | "enum" | "string" | "looukup";
  @observable private accessor _value: any;
  @observable.shallow accessor dropDownValues: DropDownValue[];
  category: string | null;
  description: string;
  readOnly: boolean;
  @observable accessor isDirty = false;
  @observable accessor errors: string[];

  get value(): any {
    return this._value;
  }

  set value(value: any) {
    if(value !== this._value){
      this.isDirty = true;
    }
    if (this.type === "looukup" && value === "") {
      this._value = null;
    } else {
      this._value = value;
    }
  }
  @computed
  get error(): string | undefined {
    return this.errors.length === 0
      ? undefined
      : this.errors.join("\n");
  }

  constructor(apiProperty: ApiEditorProperty) {
    this.name = apiProperty.name;
    this.type = apiProperty.type;
    this._value = apiProperty.value;
    this.dropDownValues = apiProperty.dropDownValues;
    this.category = apiProperty.category;
    this.description = apiProperty.description;
    this.readOnly = apiProperty.readOnly;
    this.errors = apiProperty.errors ?? [];
  }

  update(propertyUpdate: PropertyUpdate | undefined) {
    if (!propertyUpdate) {
      this.errors = [];
      return;
    }
    this.errors = propertyUpdate.errors ?? [];
    this.dropDownValues = propertyUpdate.dropDownValues;
    if (this.type === "looukup" &&
      this._value != null &&
      this.dropDownValues.map(x => x.value).includes(!this._value)
    ) {
      this._value = null;
    }
  }
}
