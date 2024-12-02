import {
  IApiEditorProperty,
  IDropDownValue, IPropertyChange,
  IPropertyUpdate,
  PropertyType
} from "src/API/IArchitectApi.ts";
import { computed, observable } from "mobx";

export class EditorProperty implements IApiEditorProperty {
  name: string;
  type: PropertyType;
  @observable private accessor _value: any;
  @observable.shallow accessor dropDownValues: IDropDownValue[];
  category: string | null;
  description: string;
  controlPropertyId: string | null;
  readOnly: boolean;
  @observable accessor isDirty = false;
  @observable accessor errors: string[];

  get value(): any {
    return this._value;
  }

  set value(value: any) {
    if (value !== this._value) {
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

  constructor(apiProperty: IApiEditorProperty) {
    this.name = apiProperty.name;
    this.controlPropertyId = apiProperty.controlPropertyId
    this.type = apiProperty.type;
    this._value = apiProperty.value;
    this.dropDownValues = apiProperty.dropDownValues;
    this.category = apiProperty.category;
    this.description = apiProperty.description;
    this.readOnly = apiProperty.readOnly;
    this.errors = apiProperty.errors ?? [];
  }

  update(propertyUpdate: IPropertyUpdate | undefined) {
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

export function toChanges(properties: IApiEditorProperty[]): IPropertyChange[] {
  return properties
    .filter(x => !x.readOnly)
    .map(x => {
      return {
        name: x.name,
        controlPropertyId: x.controlPropertyId,
        value: x.value === undefined || x.value === null ? null : x.value.toString(),
      }
    });
}
