import {
  DropdownDataTable,
  IBodyCellDriver,
} from "modules/Editors/DropdownEditor/DropdownTableModel";
import { DropdownEditorBehavior } from "modules/Editors/DropdownEditor/DropdownEditorBehavior";
import { bodyCellClass } from "modules/Editors/DropdownEditor/Cells/CellsCommon";
import React from "react";
import { TypeSymbol } from "dic/Container";
import { TextCellDriver } from "modules/Editors/DropdownEditor/Cells/TextCellDriver";
import moment from "moment";
import {
  currentCellText,
  currentProperty,
} from "gui/Components/ScreenElements/Table/TableRendering/currentCell";
import { getProperties } from "model/selectors/DataView/getProperties";

export class DateCellDriver implements IBodyCellDriver {
  constructor(
    private dataIndex: number,
    private dataTable: DropdownDataTable,
    private behavior: DropdownEditorBehavior,
    private formatterPattern: string
  ) {}

  formattedText(rowIndex: number){
    const value = this.dataTable.getValue(rowIndex, this.dataIndex);
    let momentValue = moment(value);
    return momentValue.isValid() ? momentValue.format(this.formatterPattern) : "";
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
          this.behavior.handleTableCellClicked(e, rowIndex);
        }}
      >
        {this.formattedText(rowIndex)}
      </div>
    );
  }
}

export const IDateCellDriver = TypeSymbol<TextCellDriver>("ITextCellDriver");
