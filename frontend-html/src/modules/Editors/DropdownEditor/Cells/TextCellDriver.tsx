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

import React from "react";
import { bodyCellClass } from "./CellsCommon";
import { DropdownDataTable, IBodyCellDriver } from "../DropdownTableModel";
import { TypeSymbol } from "dic/Container";
import { IDriverState } from "modules/Editors/DropdownEditor/Cells/IDriverState";
import Highlighter from "react-highlight-words";

export class TextCellDriver implements IBodyCellDriver {
  constructor(
    private dataIndex: number,
    private dataTable: DropdownDataTable,
    private driverState: IDriverState,
    private customStyle?: {[key: string]: string} | undefined
  ) {
  }

  formattedText(rowIndex: number){
    if(this.dataTable.rowCount <= rowIndex){
      return "";
    }
    return this.dataTable.getValue(rowIndex, this.dataIndex) ?? "";
  }

  render(rowIndex: number) {
    const rowId = this.dataTable.getRowIdentifierByIndex(rowIndex);
    if(rowId === null || rowId === ""){
      console.error("There are rows with missing identifier in the dropdown. This causes confusing row coloring. Fix your model!")
    }

    return (
      <div
        className={bodyCellClass(
          rowIndex,
          this.driverState.chosenRowId === rowId,
          this.driverState.cursorRowId === rowId
        )}
        style={this.customStyle}
        onClick={(e) => {
          this.driverState.handleTableCellClicked(e, rowIndex)
        }
        }
      >
      <Highlighter
        searchWords={[this.dataTable.filterPhrase]}
        textToHighlight={this.formattedText(rowIndex)}
        autoEscape={true}
      />
      </div>
    );
  }
}

export const ITextCellDriver = TypeSymbol<TextCellDriver>("ITextCellDriver");
