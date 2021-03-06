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
import cx from "classnames";
import S from "./NumberCell.module.scss";
import { TypeSymbol } from "dic/Container";
import { IDriverState } from "modules/Editors/DropdownEditor/Cells/IDriverState";

export class NumberCellDriver implements IBodyCellDriver {
  constructor(
    private dataIndex: number,
    private dataTable: DropdownDataTable,
    private driverState: IDriverState
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
    return (
      <div
        className={cx(
          bodyCellClass(
            rowIndex,
            this.driverState.chosenRowId === rowId,
            this.driverState.cursorRowId === rowId
          ),
          S.cell
        )}
        onClick={(e) => this.driverState.handleTableCellClicked(e, rowIndex)}
      >
        {this.formattedText(rowIndex)}
      </div>
    );
  }
}

export const INumberCellDriver = TypeSymbol<NumberCellDriver>("INumberCellDriver");
