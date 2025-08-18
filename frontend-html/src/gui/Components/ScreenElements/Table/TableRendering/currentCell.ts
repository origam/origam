/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { getFieldErrorMessage } from "model/selectors/DataView/getFieldErrorMessage";
import { Memoized } from "./common/Memoized";
import { currentRowCellsDimensions, currentRowCellsDraws } from "./currentRowCells";
import { stripHtml } from "string-strip-html";
import {
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

export const currentProperty = () => {
  const property = propertyById().get(currentColumnId() as any)!;
  if (property.column === "Polymorph") {
    return property.getPolymophicProperty(currentRow() as any[]);
  } else {
    return property;
  }
};

export const currentCellText = Memoized(() => {
  const value = currentCellValue();
  let text = dataTable().resolveCellText(currentProperty(), value);
  if (text !== undefined && text !== null && text.length > 500){
    text = text.substring(0, 500) + "...(TRUNCATED)";
  }
  if (text && currentProperty().isRichText) {
    text = stripHtml(text).result;
  }
  if (Array.isArray(text)) {
    text = text.join(", ");
  }
  return text;
});
scRenderCell.push(() => currentCellText.clear());

export const currentCellTextMultiline = Memoized(() => {
  const value = currentCellValue();
  let text = dataTable().resolveCellText(currentProperty(), value);
  if (text && currentProperty().multiline) {
    text = text.split("\n");
  }
  return text;
});
scRenderCell.push(() => currentCellTextMultiline.clear());

export const currentCellValue = Memoized(() => {
  const property = propertyById().get(currentColumnId() as any)!
  if (property.column === "Polymorph") {
    const polymorphicProperty = currentProperty();
    return dataTable().getCellValue(currentDataRow(), polymorphicProperty);
  } else {
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
