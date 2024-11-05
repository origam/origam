import { observable } from "mobx";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { ApiEditorProperty, DropDownValue } from "src/API/IArchitectApi.ts";

export class EditorState {
  constructor(
    public id: string,
    public origamId: string,
    private architectApi: ArchitectApi
  ) {
  }

  @observable accessor properties: EditorProperty[] = [];
  @observable accessor isDirty= false;
  @observable accessor isSaving = false;
  @observable accessor isActive= true;

  *initialize(): Generator<Promise<ApiEditorProperty[]>, void, ApiEditorProperty[]> {
    const apiProperties = yield this.architectApi.getProperties(this.origamId);
    this.properties = apiProperties.map(apiProperty => new EditorProperty(apiProperty));
  }

  *save() {
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
}

export class EditorProperty implements ApiEditorProperty {
  name: string;
  type: "boolean" | "enum" | "string" | "looukup";
  @observable accessor value: any;
  dropDownValues: DropDownValue[];
  category: string | null;
  description: string;
  readOnly: boolean;

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
