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
import { ITablePanelView } from "../../../../model/TablePanelView/types/ITablePanelView";
import { TablePanelView } from "../../../../model/TablePanelView/TablePanelView";
import { getCellValueByIdx } from "../../../../model/selectors/TablePanelView/getCellValue";
import { getTableViewPropertyByIdx } from "../../../../model/selectors/TablePanelView/getTableViewPropertyByIdx";
import { getTableViewRecordByExistingIdx } from "../../../../model/selectors/TablePanelView/getTableViewRecordByExistingIdx";
import { getSelectedColumnId } from "../../../../model/selectors/TablePanelView/getSelectedColumnId";
import { getSelectedRowId } from "../../../../model/selectors/TablePanelView/getSelectedRowId";

export interface ICellRendererData {
  tablePanelView: ITablePanelView;
}

export interface ICellRenderer extends ICellRendererData {}

export class CellRenderer implements ICellRenderer {
  constructor(data: ICellRendererData) {
    Object.assign(this, data);
  }

  tablePanelView: ITablePanelView = null as any;

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
      this.tablePanelView.onCellClick(rowIndex, columnIndex);
    });

    /* BACKGROUND FILL */
    if (cell.isColumnOrderChangeSource) {
      ctx.fillStyle = "#eeeeff";
      //} else if(cell.isColumnOrderChangeTarget){
    } else if (cell.isCellCursor) {
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
          if (cell.value !== null) {
            ctx.font = `${14 * CPR}px "Font Awesome 5 Free"`;
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText(
              cell.value ? "\uf14a" : "\uf0c8",
              (columnWidth / 2) * CPR,
              (rowHeight / 2) * CPR
            );
          }
          break;
        case "Date":
          if (cell.value !== null) {
            ctx.fillText(
              moment(cell.value).format(cell.formatterPattern),
              15 * CPR,
              15 * CPR
            );
          }
          break;
        default:
          if (cell.value !== null) {
            ctx.fillText("" + cell.value!, 15 * CPR, 15 * CPR);
          }
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
    if (cell.isColumnOrderChangeTarget) {
      ctx.save();
      ctx.fillStyle = "blue";
      ctx.beginPath();
      ctx.fillRect(0, 0, 2, rowHeight);
      ctx.restore();
    }
  }

  @computed get getCellValue() {
    return getCellValueByIdx(this.tablePanelView);
  }

  getCell(rowIndex: number, columnIndex: number): IRenderedCell {
    const value = this.getCellValue(rowIndex, columnIndex);
    const property = getTableViewPropertyByIdx(
      this.tablePanelView,
      columnIndex
    );
    const record = getTableViewRecordByExistingIdx(
      this.tablePanelView,
      rowIndex
    );
    const selectedColumnId = getSelectedColumnId(this.tablePanelView);
    const selectedRowId = getSelectedRowId(this.tablePanelView);

    return {
      isCellCursor:
        property.id === selectedColumnId && record[0] === selectedRowId,
      isRowCursor: record[0] === selectedRowId,
      isColumnOrderChangeSource:
        this.tablePanelView.columnOrderChangingSourceId === property.id,
      isColumnOrderChangeTarget:
        this.tablePanelView.columnOrderChangingTargetId === property.id,
      isLoading: false,
      isInvalid: false,
      formatterPattern: property.formatterPattern,
      type: property.column,
      value
    };
  }
}
