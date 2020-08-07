import cx from "classnames";
import { TypeSymbol } from "dic/Container";
import React from "react";
import { DropdownEditorBehavior } from "../DropdownEditorBehavior";
import { DropdownDataTable, IBodyCellDriver } from "../DropdownTableModel";
import S from "./BooleanCell.module.scss";
import { bodyCellClass } from "./CellsCommon";

export class BooleanCellDriver implements IBodyCellDriver {
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
          bodyCellClass(rowIndex, this.behavior.choosenRowId === rowId, this.behavior.cursorRowId === rowId),
          S.cell
        )}
        onClick={(e) => this.behavior.handleTableCellClicked(e, rowIndex)}
      >
        {value ? <i className="far fa-check-square"></i> : <i className="far fa-square"></i>}
      </div>
    );
  }
}
export const IBooleanCellDriver = TypeSymbol<BooleanCellDriver>("IBooleanCellDriver");
