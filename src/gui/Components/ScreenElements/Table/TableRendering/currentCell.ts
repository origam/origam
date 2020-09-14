import { getFieldErrorMessage } from "model/selectors/DataView/getFieldErrorMessage";
import { Memoized } from "./common/Memoized";
import { currentRowCellsDimensions, currentRowCellsDraws } from "./currentRowCells";
import {
  currentDataRow,
  dataTable,
  drawingColumnIndex,
  gridLeadCellDimensions,
  propertyById,
  realFixedColumnCount,
  rowHeight,
  rowIndex,
  scRenderCell,
} from "./renderingValues";
import { dataRowColumnIds } from "./rowCells/dataRowCells";

export function drawCurrentCell() {
  const colIdx = drawingColumnIndex();
  const cellDraws = currentRowCellsDraws();
  if (colIdx >= cellDraws.length) return;
  currentRowCellsDraws()[drawingColumnIndex()]();
}

export function currentColumnLeft() {
  return currentRowCellsDimensions()[drawingColumnIndex()].left;
}

export function currentColumnLeftVisible() {
  return currentRowCellsDimensions()[drawingColumnIndex()].leftVisible;
}

export function currentColumnWidthVisible() {
  return currentRowCellsDimensions()[drawingColumnIndex()].widthVisible;
}

export function currentColumnWidth() {
  return currentRowCellsDimensions()[drawingColumnIndex()].width;
}

export function currentColumnRight() {
  return currentRowCellsDimensions()[drawingColumnIndex()].right;
}

export function currentRowTop() {
  return rowIndex() * rowHeight();
}

export function currentRowHeight() {
  return rowHeight();
}

export function currentRowBottom() {
  return currentRowTop() + currentRowHeight();
}

export function isCurrentCellFixed() {
  return drawingColumnIndex() < realFixedColumnCount();
}

export function currentGridLeadCellLeft() {
  return gridLeadCellDimensions()[drawingColumnIndex()].left;
}

export function currentGridLeadCellWidth() {
  return gridLeadCellDimensions()[drawingColumnIndex()].width;
}

export function currentGridLeadCellRight() {
  return gridLeadCellDimensions()[drawingColumnIndex()].right;
}

export function currentColumnId() {
  return dataRowColumnIds()[drawingColumnIndex()];
}

export const currentProperty = () => propertyById().get(currentColumnId() as any)!;

export const currentCellText = Memoized(() => {
  const value = currentCellValue();
  return dataTable().resolveCellText(currentProperty(), value);
});
scRenderCell.push(() => currentCellText.clear());

export const currentCellValue = Memoized(() => {
  const value = currentDataRow()[currentProperty().dataIndex];
  return value;
});
scRenderCell.push(() => currentCellValue.clear());

export const currentCellErrorMessage = Memoized(() => {
  const errMsg = getFieldErrorMessage(currentProperty())(currentDataRow(), currentProperty());
  return errMsg;
});
scRenderCell.push(() => currentCellErrorMessage.clear());

export const isCurrentCellLoading = Memoized(() => {
  const value = dataTable().isCellTextResolving(currentProperty(), currentCellValue());
  return value;
});
scRenderCell.push(() => isCurrentCellLoading.clear());
