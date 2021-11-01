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

  formattedText(rowIndex: number){
    return this.dataTable.getValue(rowIndex, this.dataIndex) ?? "";
  }

  render(rowIndex: number) {
    const rowId = this.dataTable.getRowIdentifierByIndex(rowIndex);
    return (
      <div
        className={bodyCellClass(
          rowIndex,
          this.behavior.chosenRowId === rowId,
          this.behavior.cursorRowId === rowId
        )}
        onClick={(e) => {
          this.behavior.handleTableCellClicked(e, rowIndex)}
        }
      >
        {this.formattedText(rowIndex)}
      </div>
    );
  }
}
export const ITextCellDriver = TypeSymbol<TextCellDriver>("ITextCellDriver");
