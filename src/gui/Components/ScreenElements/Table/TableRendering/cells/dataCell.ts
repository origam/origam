import { tableColumnIds, columnWidths, context2d, context, columnIndex, rowHeight, rowIndex } from "../renderingValues";
import {
  currentColumnLeft,
  currentRowTop,
  currentCellText,
  currentColumnWidth,
  currentRowHeight,
} from "../currentCell";
import { applyScrollTranslation, clipCell } from "./cellsCommon";
import { CPR } from "utils/canvas";
import { getTableViewPropertyByIdx } from "model/selectors/TablePanelView/getTableViewPropertyByIdx";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getTableViewRecordByExistingIdx } from "model/selectors/TablePanelView/getTableViewRecordByExistingIdx";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getRowStateColumnBgColor } from "model/selectors/RowState/getRowStateColumnBgColor";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";

export function dataColumnsWidths() {
  return tableColumnIds().map((id) => columnWidths().get(id) || 100);
}

export function dataColumnsDraws() {
  return tableColumnIds().map((id) => () => {
    const ctx2d = context2d();
    applyScrollTranslation();
    clipCell();
    drawDataCellBackground();
    ctx2d.fillStyle = "black";
    ctx2d.font = `${CPR * 12}px "IBM Plex Sans", Arial, sans-serif`;
    ctx2d.fillText(
      currentCellText(),
      CPR * (currentColumnLeft() + 2),
      CPR * (currentRowTop() + 17)
    );
  });
}

export function drawDataCellBackground() {

  const ctx2d = context2d();

  ctx2d.fillStyle = getUnderLineColor();
  ctx2d.fillRect(0, 0, currentColumnWidth() * CPR, rowHeight() * CPR);

  ctx2d.fillStyle = getBackGroundColor();
  ctx2d.fillRect(
    CPR * currentColumnLeft(),
    CPR * currentRowTop(),
    CPR * currentColumnWidth(),
    CPR * currentRowHeight()
  );
}

function getUnderLineColor(){ return "#e5e5e5";}

function getBackGroundColor() {
  const ctx = context()
  const tablePanelView = getTablePanelView(ctx);
  const property = getTableViewPropertyByIdx(tablePanelView, columnIndex());
  const isColumnOrderChangeSource = tablePanelView.columnOrderChangingSourceId === property.id;
  const selectedColumnId = tableColumnIds()[columnIndex()];
  const dataTable = getDataTable(tablePanelView);
  const record = getTableViewRecordByExistingIdx(tablePanelView, rowIndex());
  const recordId = dataTable.getRowId(record);
  const selectedRowId = getSelectedRowId(tablePanelView);

  const isCellCursor = property.id === selectedColumnId && recordId === selectedRowId;
  const isRowCursor = recordId === selectedRowId;

  const backgroundColor =
    getRowStateColumnBgColor(tablePanelView, recordId, "") ||
    getRowStateRowBgColor(tablePanelView, recordId);

  if (isColumnOrderChangeSource) {
    return "#eeeeff";
    //} else if(cell.isColumnOrderChangeTarget){
  } else if (isCellCursor) {
    return "#bbbbbb";
  } else if (isRowCursor) {
    return "#dddddd";
  } else {
    if (backgroundColor) {
      return backgroundColor;
    } else {
      return rowIndex() % 2 === 0 ? "#f7f6fa" : "#ffffff";
    }
  }
}
