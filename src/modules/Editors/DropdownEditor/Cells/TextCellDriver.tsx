import React from "react";
import { bodyCellClass } from "./CellsCommon";
import { IDropdownDataTable, IBodyCellDriver, DropdownDataTable } from "../DropdownTableModel";
import { IDropdownEditorBehavior, DropdownEditorBehavior } from "../DropdownEditorBehavior";
import { TypeSymbol } from "dic/Container";

export class TextCellDriver implements IBodyCellDriver {
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
        className={bodyCellClass(
          rowIndex,
          this.behavior.choosenRowId === rowId,
          this.behavior.cursorRowId === rowId
        )}
        onClick={(e) => {
          this.behavior.handleTableCellClicked(e, rowIndex)}
        }
      >
        {value}
      </div>
    );
  }
}
export const ITextCellDriver = TypeSymbol<TextCellDriver>("ITextCellDriver");
