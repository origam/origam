import bind from "bind-decorator";
import {
  IRenderCellArgs,
  IRenderedCell
} from "../../../Components/ScreenElements/Table/types";
import { CPR } from "../../../../utils/canvas";
import moment from "moment";
import { IDataTable } from "../../../../model/types/IDataTable";
import { IProperty } from "../../../../model/types/IProperty";
import { computed } from "mobx";

export interface ICellRendererData {
  getCellValue: (columnIdx: number, rowIdx: number) => any;
  getTableViewProperties: () => IProperty[];
}

export class CellRenderer {
  constructor(data: ICellRendererData) {
    Object.assign(this, data);
  }

  getCellValue: (rowIdx: number, columnIdx: number) => any = null as any;
  getTableViewProperties: () => IProperty[] = null as any;

  @computed get tableViewProperties() {
    return this.getTableViewProperties();
  }

  @bind
  renderCell({
    rowIndex,
    columnIndex,
    rowHeight,
    columnWidth,
    onCellClick,
    ctx
  }: IRenderCellArgs) {
    const cell = this.getCell(rowIndex, columnIndex);
    onCellClick.subscribe((event: any) => {
      console.log(rowIndex, columnIndex);
    });

    /* BACKGROUND FILL */
    if (cell.isCellCursor) {
      ctx.fillStyle = "#bbbbbb";
    } else if (cell.isRowCursor) {
      ctx.fillStyle = "#dddddd";
    } else {
      ctx.fillStyle = rowIndex % 2 === 0 ? "#ffffff" : "#efefef";
    }
    ctx.fillRect(0, 0, columnWidth * CPR, rowHeight * CPR);

    /* TEXTUAL CONTENT */
    ctx.font = `${12 * CPR}px sans-serif`;
    if (cell.isLoading) {
      ctx.fillStyle = "#888888";
      ctx.fillText("Loading...", 15 * CPR, 15 * CPR);
    } else {
      ctx.fillStyle = "black";
      switch (cell.type) {
        case "CheckBox":
          ctx.font = `${14 * CPR}px "Font Awesome 5 Free"`;
          ctx.textAlign = "center";
          ctx.textBaseline = "middle";
          ctx.fillText(
            cell.value ? "\uf14a" : "\uf0c8",
            (columnWidth / 2) * CPR,
            (rowHeight / 2) * CPR
          );
          break;
        case "Date":
          ctx.fillText(
            moment(cell.value).format(cell.formatterPattern),
            15 * CPR,
            15 * CPR
          );
          break;
        default:
          ctx.fillText("" + cell.value!, 15 * CPR, 15 * CPR);
      }
    }

    if (cell.isInvalid) {
      ctx.save();
      ctx.fillStyle = "red";
      ctx.beginPath();
      ctx.moveTo(0, 0);
      ctx.lineTo(0, rowHeight);
      ctx.lineTo(5, rowHeight / 2);
      ctx.closePath();
      ctx.fill();
      /*ctx.fillRect(0, 0, 5, 5);
      ctx.fillRect(0, rowHeight - 5, 5, 5);
      ctx.fillRect(0, 0, 3, rowHeight);*/
      ctx.restore();
    }
  }

  getCell(rowIndex: number, columnIndex: number): IRenderedCell {
    const value = this.getCellValue(rowIndex, columnIndex);
    const property = this.tableViewProperties[columnIndex];
    // const row = this.table
    return {
      isCellCursor: rowIndex === 3 && columnIndex === 2,
      isRowCursor: rowIndex === 3,
      isLoading: false,
      isInvalid: false,
      formatterPattern: "",
      type: property.column,
      value,
    };
  }
}
