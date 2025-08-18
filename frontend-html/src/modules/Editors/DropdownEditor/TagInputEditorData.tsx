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
import { DataViewData } from "../../DataView/DataViewData";
import { RowCursor } from "../../DataView/TableCursor";
import { action, computed } from "mobx";
import { DropdownEditorData, IDropdownEditorData } from "./DropdownEditorData";
import { DropdownEditorSetup } from "modules/Editors/DropdownEditor/DropdownEditorSetup";

@bind
export class TagInputEditorData implements IDropdownEditorData {
  dropdownEditorData: IDropdownEditorData;

  constructor(
    private dataTable: DataViewData,
    private rowCursor: RowCursor,
    private setup: () => DropdownEditorSetup
  ) {
    this.dropdownEditorData = new DropdownEditorData(dataTable, rowCursor, setup);
  }

  setValue(value: string[]) {
  }

  @computed get value(): string | string[] | null {
    return this.dropdownEditorData.value;
  }

  @computed get text(): string {
    return this.dropdownEditorData.text;
  }

  get isResolving() {
    return this.dropdownEditorData.isResolving;
  }

  @action.bound async chooseNewValue(value: any) {
    if (this.value && this.value.includes(value)) {
      return;
    }
    const newArray = [...this.value ?? [], value];
    if (this.rowCursor.selectedId) {
      await this.dataTable.setNewValue(this.rowCursor.selectedId, this.setup().propertyId, newArray);
    }
  }

  get idsInEditor() {
    return (this.value ? this.value : []) as string[];
  }

  remove(value: any): void {
    // not needed in TagInputEditorData
  }
}
