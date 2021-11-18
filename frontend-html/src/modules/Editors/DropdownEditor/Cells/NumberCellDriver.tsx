import React from "react";
import { bodyCellClass } from "./CellsCommon";
import { IDropdownDataTable, IBodyCellDriver, DropdownDataTable } from "../DropdownTableModel";
import cx from "classnames";
import S from "./NumberCell.module.scss";
import { DropdownEditorBehavior } from "../DropdownEditorBehavior";
import { TypeSymbol } from "dic/Container";

export class NumberCellDriver implements IBodyCellDriver {
  constructor(
    private dataIndex: number,
    private dataTable: DropdownDataTable,
    private behavior: DropdownEditorBehavior
  ) {}

  formattedText(rowIndex: number){
    return this.dataTable.getValue(rowIndex, this.dataIndex) ?? "";
  }

  render(rowIndex: number) {
    const rowId = this.dataTable.getRowIdentifierByIndex(rowIndex);
    return (
      <div
        className={cx(
          bodyCellClass(
            rowIndex,
            this.behavior.chosenRowId === rowId,
            this.behavior.cursorRowId === rowId
          ),
          S.cell
        )}
        onClick={(e) => this.behavior.handleTableCellClicked(e, rowIndex)}
      >
        {this.formattedText(rowIndex)}
      </div>
    );
  }
}
export const INumberCellDriver = TypeSymbol<NumberCellDriver>("INumberCellDriver");
