import bind from "bind-decorator";
import {
  IRenderCellArgs,
  IRenderedCell
} from "../../../Components/ScreenElements/Table/types";
import { CPR } from "../../../../utils/canvas";
import moment from "moment";
import { IDataTable } from "../../../../model/entities/types/IDataTable";
import { IProperty } from "../../../../model/entities/types/IProperty";
import { computed } from "mobx";
import { ITablePanelView } from "../../../../model/entities/TablePanelView/types/ITablePanelView";
import { TablePanelView } from "../../../../model/entities/TablePanelView/TablePanelView";
import { getCellValueByIdx } from "../../../../model/selectors/TablePanelView/getCellValue";
import { getTableViewPropertyByIdx } from "../../../../model/selectors/TablePanelView/getTableViewPropertyByIdx";
import { getTableViewRecordByExistingIdx } from "../../../../model/selectors/TablePanelView/getTableViewRecordByExistingIdx";
import { getSelectedColumnId } from "../../../../model/selectors/TablePanelView/getSelectedColumnId";
import { getSelectedRowId } from "../../../../model/selectors/TablePanelView/getSelectedRowId";
import { getCellTextByIdx } from "../../../../model/selectors/TablePanelView/getCellText";
import { getDataTable } from "../../../../model/selectors/DataView/getDataTable";
import { getRowStates } from "model/selectors/RowState/getRowStates";
import { getRowStateBackgroundColor } from "model/selectors/RowState/getRowStateBackgroundColor";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";

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

    // TODO: background color ?
    // TODO: Read only for bool fields in grid

    /* CONTENT */
    ctx.font = `${12 * CPR}px sans-serif`;
    if (cell.isLoading) {
      ctx.fillStyle = "#888888";
      ctx.fillText("Loading...", 15 * CPR, 15 * CPR);
    } else {
      ctx.fillStyle = cell.foregroundColor || "black";
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
        case "ComboBox":
          if (cell.value !== null) {
            ctx.fillText("" + cell.text!, 15 * CPR, 15 * CPR);
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

  @computed get getCellText() {
    return getCellTextByIdx(this.tablePanelView);
  }

  getCell(rowIndex: number, columnIndex: number): IRenderedCell {
    const dataTable = getDataTable(this.tablePanelView);
    const value = this.getCellValue(rowIndex, columnIndex);
    let text = value;
    let isLoading = false;
    const property = getTableViewPropertyByIdx(
      this.tablePanelView,
      columnIndex
    );
    const record = getTableViewRecordByExistingIdx(
      this.tablePanelView,
      rowIndex
    );
    if (property.isLookup) {
      text = this.getCellText(rowIndex, columnIndex);
      isLoading = property.lookup!.isLoading(value);
    }
    const selectedColumnId = getSelectedColumnId(this.tablePanelView);
    const selectedRowId = getSelectedRowId(this.tablePanelView);
    const recordId = dataTable.getRowId(record);


    let isInvalid = false;
    let invalidMessage: string | undefined = undefined;
    if (record) {
      const dataView = getDataTable(property);
      const dsFieldErrors = getDataSourceFieldByName(
        property,
        "__Errors"
      );
      const errors = dsFieldErrors
        ? dataView.getCellValueByDataSourceField(record, dsFieldErrors)
        : null;

      const errMap: Map<number, string> | undefined = errors
        ? new Map(
            Object.entries<string>(errors.fieldErrors).map(
              ([dsIndexStr, errMsg]: [string, string]) => [
                parseInt(dsIndexStr, 10),
                errMsg
              ]
            )
          )
        : undefined;

      const errMsg =
        dsFieldErrors && errMap
          ? errMap.get(property.dataSourceIndex)
          : undefined;
      if (errMsg) {
        isInvalid = true;
        invalidMessage = errMsg;
      }
    }

    return {
      isCellCursor:
        property.id === selectedColumnId && recordId === selectedRowId,
      isRowCursor: recordId === selectedRowId,
      isColumnOrderChangeSource:
        this.tablePanelView.columnOrderChangingSourceId === property.id,
      isColumnOrderChangeTarget:
        this.tablePanelView.columnOrderChangingTargetId === property.id,
      isLoading,
      isInvalid,
      formatterPattern: property.formatterPattern,
      type: property.column,
      value,
      text,
      backgroundColor: getRowStateBackgroundColor(
        this.tablePanelView,
        recordId,
        property.id
      ),
      foregroundColor: getRowStateForegroundColor(
        this.tablePanelView,
        recordId,
        property.id
      )
    };
  }
}
