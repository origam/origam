import { observable } from "mobx";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import {
  ApiEditorProperty,
  DropDownValue,
  RuleErrors
} from "src/API/IArchitectApi.ts";

export class EditorState {
  constructor(
    public id: string,
    public origamId: string,
    private architectApi: ArchitectApi
  ) {
  }

  @observable accessor properties: EditorProperty[] = [];
  @observable accessor isDirty = false;
  @observable accessor isSaving = false;
  @observable accessor isActive = true;

  * initialize(): Generator<Promise<ApiEditorProperty[]>, void, ApiEditorProperty[]> {
    const apiProperties = yield this.architectApi.getProperties(this.origamId);
    this.properties = apiProperties.map(apiProperty => new EditorProperty(apiProperty));
  }

  * save() {
    try {
      this.isSaving = true;
      yield this.architectApi.persistChanges(this.origamId, this.properties);
      this.isDirty = false;
    } catch (error) {
      console.error('Failed to save content', error);
    } finally {
      this.isSaving = false;
    }
  }

  * onPropertyUpdated(property: EditorProperty, value: any): Generator<Promise<RuleErrors[]>, void, RuleErrors[]> {
    property.value = value;
    const ruleErrors = (yield this.architectApi.checkRules(this.origamId, this.properties)) as RuleErrors[];
    for (const property of this.properties) {
      const ruleError = ruleErrors.find(error => property.name === error.name)
      property.errors = ruleError
        ? ruleError.errors.join("\n")
        : undefined;
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
  @observable accessor errors: string | undefined;

  constructor(apiProperty: ApiEditorProperty) {
    this.name = apiProperty.name;
    this.type = apiProperty.type;
    this.value = apiProperty.value;
    this.dropDownValues = apiProperty.dropDownValues;
    this.category = apiProperty.category;
    this.description = apiProperty.description;
    this.readOnly = apiProperty.readOnly;
  }
}
