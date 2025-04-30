/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

import { IUpdatePropertiesResult } from "src/API/IArchitectApi";
import {
  IPropertyManager
} from "src/components/editors/propertyEditor/IPropertyManager.tsx";
import { EditorProperty } from "../editors/gridEditor/EditorProperty";
import { observable } from "mobx";

export class PropertiesState implements IPropertyManager {
  @observable accessor properties: EditorProperty[] = [];
  @observable accessor editedItemName = "";

  setEdited(itemName: string, properties: EditorProperty[]): void {
    this.properties = properties;
    this.editedItemName = itemName;
  }

  onPropertyUpdated(_property: EditorProperty, _value: any): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
      throw new Error("Method not implemented.");
  }
}