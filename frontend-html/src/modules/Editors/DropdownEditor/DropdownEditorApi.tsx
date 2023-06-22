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

import { bind } from "@decorize/bind";
import { DataViewAPI } from "modules/DataView/DataViewAPI";
import { RowCursor } from "modules/DataView/TableCursor";
import { dropdownPageSize, IDropdownEditorBehavior } from "./DropdownEditorBehavior";
import { EagerlyLoadedGrid } from "./DropdownEditorCommon";
import { DropdownEditorSetup } from "modules/Editors/DropdownEditor/DropdownEditorSetup";

export interface IDropdownEditorApi {
  getLookupList(searchTerm: string): Generator;
}

@bind
export class DropdownEditorApi implements IDropdownEditorApi {
  constructor(
    private setup: () => DropdownEditorSetup,
    private rowCursor: RowCursor,
    private api: DataViewAPI,
    private behavior: () => IDropdownEditorBehavior,
  ) {
  }

  *getLookupList(searchTerm: string): Generator {
    const setup = this.setup();
    if (setup.dropdownType === EagerlyLoadedGrid) {
      return yield*this.api.getLookupList({
        ColumnNames: setup.columnNames,
        Property: setup.propertyId,
        Id: this.rowCursor.selectedId!,
        LookupId: setup.lookupId,
        Parameters: setup.parameters,
        ShowUniqueValues: setup.showUniqueValues,
        SearchText: searchTerm || "",
        PageSize: -1,
        PageNumber: 1,
      });
    } else {
      return yield*this.api.getLookupList({
        ColumnNames: setup.columnNames,
        Property: setup.propertyId,
        Id: this.rowCursor.selectedId!,
        LookupId: setup.lookupId,
        Parameters: setup.parameters,
        ShowUniqueValues: setup.showUniqueValues,
        SearchText: searchTerm || "",
        PageSize: dropdownPageSize,
        PageNumber: this.behavior().willLoadPage,
      });
    }
  }
}

