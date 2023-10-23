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

import { IDropDownColumn } from "./IDropDownColumn";
import { NewRecordScreen } from "gui/connections/NewRecordScreen";


export enum IDropDownType {
  EagerlyLoadedGrid = "EagerlyLoadedGrid",
  LazilyLoadedGrid = "LazilyLoadedGrid",
  EagerlyLoadedTree = "EagerlyLoadedTree",
}

export interface IDropDownParameter {
  parameterName: string;
  fieldName: string;
}

export interface ILookupData {
  lookupId: string;
  dropDownShowUniqueValues: boolean;
  identifier: string;
  identifierIndex: number;
  dropDownType: IDropDownType;
  cached: boolean;
  searchByFirstColumnOnly: boolean;
  dropDownColumns: IDropDownColumn[];
  dropDownParameters: IDropDownParameter[];
  newRecordScreen?: NewRecordScreen;
}

export interface ILookup extends ILookupData {
  $type_ILookup: 1;

  parameters: { [key: string]: any };
  parent?: any;
}

export const isILookup = (o: any): o is ILookup => o.$type_ILookup;
