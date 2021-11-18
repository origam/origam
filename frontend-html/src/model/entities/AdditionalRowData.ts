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

import { IAdditionalRowData, } from "./types/IAdditionalRecordData";
import { observable } from "mobx";

export class AdditionalRowData implements IAdditionalRowData {
  $type_IAdditionalRowData: 1 = 1;

  @observable dirtyNew: boolean = false;
  @observable dirtyDeleted: boolean = false;
  @observable dirtyValues: Map<string, any> = new Map();
  @observable dirtyFormValues: Map<string, any> = new Map();

  parent?: any;
}
