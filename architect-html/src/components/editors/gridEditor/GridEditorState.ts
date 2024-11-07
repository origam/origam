import { computed, observable } from "mobx";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import {
  ApiEditorProperty,
  DropDownValue,
  RuleErrors
} from "src/API/IArchitectApi.ts";
import { IEditorNode } from "src/stores/IEditorManager.ts";

export class EditorState {
  constructor(
    private editorNode: IEditorNode,
    properties: EditorProperty[] | undefined,
    private architectApi: ArchitectApi
  ) {
    this.properties = properties ?? [];
  }

  @observable accessor properties: EditorProperty[];
  @observable accessor isDirty = false;
  @observable accessor isSaving = false;
  @observable accessor isActive = true;

  get label (){
    return this.properties.find(x => x.name === "Name")?.value || "";
  }

  get schemaItemId(){
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
      this.isDirty = false
      if(this.editorNode.parent){
        yield* this.editorNode.parent.loadChildren();
      }
    } catch (error) {
      console.error('Failed to save content', error);
    } finally {
      this.isSaving = false;
    }
  }

  * onPropertyUpdated(property: EditorProperty, value: any): Generator<Promise<RuleErrors[]>, void, RuleErrors[]> {
    property.value = value;
    const ruleErrors = (yield this.architectApi.checkRules(this.editorNode.origamId, this.properties)) as RuleErrors[];
    for (const property of this.properties) {
      const ruleError = ruleErrors.find(error => property.name === error.name)
      property.errors = ruleError
        ? ruleError.errors
        : [];
    }
    this.isDirty = ruleErrors.length === 0
  }
}

export class EditorProperty implements ApiEditorProperty {
  name: string;
  type: "boolean" | "enum" | "string" | "looukup";
  @observable accessor value: any;
  dropDownValues: DropDownValue[];
  category: string | null;
  description: string;
  readOnly: boolean;
  @observable accessor errors: string[];

  @computed
  get error(): string | undefined {
    return this.errors.length === 0
      ? undefined
      : this.errors.join("\n");
  }

  constructor(apiProperty: ApiEditorProperty) {
    this.name = apiProperty.name;
    this.type = apiProperty.type;
    this.value = apiProperty.value;
    this.dropDownValues = apiProperty.dropDownValues;
    this.category = apiProperty.category;
    this.description = apiProperty.description;
    this.readOnly = apiProperty.readOnly;
    this.errors = apiProperty.errors ?? [];
  }
}
