/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o. 

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

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
  @observable protected accessor _value: any;
  @observable.shallow accessor dropDownValues: IDropDownValue[];
  category: string | null;
  description: string;
  controlPropertyId: string | null;
  readOnly: boolean;
  @observable accessor errors: string[];

  get value(): any {
    return this._value;
  }

  set value(value: any) {
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
