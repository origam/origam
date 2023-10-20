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

import { IDropDownColumn } from "./types/IDropDownColumn";
import { IDropDownParameter, IDropDownType, ILookup, ILookupData } from "./types/ILookup";
import { NewRecordScreen } from "gui/connections/NewRecordScreen";

export enum IIdState {
  LOADING = "LOADING",
  ERROR = "ERROR",
}

export class Lookup implements ILookup {
  constructor(data: ILookupData) {
    Object.assign(this, data);
    this.dropDownColumns.forEach((o) => (o.parent = this));
  }

  $type_ILookup: 1 = 1;

  lookupId: string = "";
  newRecordScreen?: NewRecordScreen = null as any;
  dropDownShowUniqueValues: boolean = false;
  identifier: string = "";
  identifierIndex: number = 0;
  dropDownType: IDropDownType = IDropDownType.EagerlyLoadedGrid;
  cached: boolean = false;
  searchByFirstColumnOnly: boolean = false;
  dropDownColumns: IDropDownColumn[] = [];
  dropDownParameters: IDropDownParameter[] = [];

  parent?: any;

  get parameters() {
    const parameters: { [key: string]: any } = {};
    for (let param of this.dropDownParameters) {
      parameters[param.parameterName] = param.fieldName;
    }
    return parameters;
  }
}
