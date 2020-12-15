import { getFieldErrorMessage } from "model/selectors/DataView/getFieldErrorMessage";
import { Memoized } from "./common/Memoized";
import { currentRowCellsDimensions, currentRowCellsDraws } from "./currentRowCells";
import stripHtml from "string-strip-html";
import {
  context,
  currentDataRow,
  currentRow,
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
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getDataTable } from "model/selectors/DataView/getDataTable";

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

export const currentProperty = () =>{
  const property = propertyById().get(currentColumnId() as any)!;
  if(property.column === "Polymorph"){
    return property.getPolymophicProperty(currentRow() as any[]);
  }else{
    return property;
  }
};

export const currentCellText = Memoized(() => {
  const value = currentCellValue();
  let text = dataTable().resolveCellText(currentProperty(), value);
  if(text && currentProperty().multiline) {
    text = stripHtml(text).result;
  }
  return text;
});
scRenderCell.push(() => currentCellText.clear());

export const currentCellValue = Memoized(() => {
  const property = propertyById().get(currentColumnId() as any)!
  if(property.column === "Polymorph"){
    const polymorphicProperty = currentProperty();
    return dataTable().getCellValue(currentDataRow(), polymorphicProperty);
  }else{
    return dataTable().getCellValue(currentDataRow(), property);
  }
});
scRenderCell.push(() => currentCellValue.clear());

export const currentCellErrorMessage = Memoized(() => {
  return getFieldErrorMessage(currentProperty())(currentDataRow(), currentProperty());
});
scRenderCell.push(() => currentCellErrorMessage.clear());

export const isCurrentCellLoading = Memoized(() => {
  const value = dataTable().isCellTextResolving(currentProperty(), currentCellValue());
  return value;
});
scRenderCell.push(() => isCurrentCellLoading.clear());
