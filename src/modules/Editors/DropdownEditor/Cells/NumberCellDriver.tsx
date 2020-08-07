import React from "react";
import { bodyCellClass } from "./CellsCommon";
import { IDropdownDataTable, IBodyCellDriver, DropdownDataTable } from "../DropdownTableModel";
import cx from "classnames";
import S from "./NumberCell.module.scss";
import { IDropdownEditorBehavior, DropdownEditorBehavior } from "../DropdownEditorBehavior";
import { TypeSymbol } from "dic/Container";

export class NumberCellDriver implements IBodyCellDriver {
  constructor(
    private dataIndex: number,
    private dataTable: DropdownDataTable,
    private behavior: DropdownEditorBehavior
  ) {}

  render(rowIndex: number) {
    const value = this.dataTable.getValue(rowIndex, this.dataIndex);
    const rowId = this.dataTable.getRowIdentifierByIndex(rowIndex);
    return (
      <div
        className={cx(
          bodyCellClass(
            rowIndex,
            this.behavior.choosenRowId === rowId,
            this.behavior.cursorRowId === rowId
          ),
          S.cell
        )}
        onClick={(e) => this.behavior.handleTableCellClicked(e, rowIndex)}
      >
        {value}
      </div>
    );
  }
}
export const INumberCellDriver = TypeSymbol<NumberCellDriver>("INumberCellDriver");
