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

import { dataColumnsDraws, dataColumnsWidths } from "../cells/dataCell";
import { groupRowEmptyCellsDraws, groupRowEmptyCellsWidths } from "../cells/groupCell";
import { selectionCheckboxCellsDraws, selectionCheckboxCellsWidths } from "../cells/selectionCheckboxCell";
import { Memoized } from "../common/Memoized";
import { currentRow, scRenderRow, scRenderTable, tableColumnIds } from "../renderingValues";
import { ITableRow } from "../types";


export const dataRowCellsWidths = Memoized(() => {
  return [
    ...selectionCheckboxCellsWidths(),
    ...groupRowEmptyCellsWidths(),
    ...dataColumnsWidths()
  ]
})
scRenderRow.push(dataRowCellsWidths.clear);

export const dataRowCellsDraws = Memoized(() => {
  return [
    ...selectionCheckboxCellsDraws(),
    ...groupRowEmptyCellsDraws(),
    ...dataColumnsDraws()
  ]
});
scRenderRow.push(dataRowCellsDraws.clear);

export const dataRowColumnIds = Memoized(() => {
  return [
    ...selectionCheckboxCellsDraws().map(item => null),
    ...groupRowEmptyCellsDraws().map(item => null),
    ...tableColumnIds()
  ]
})
scRenderTable.push(dataRowColumnIds.clear);

export function isDataRow(row: ITableRow): row is any[] {
  return Array.isArray(row);
}

export function isCurrentDataRow() {
  return isDataRow(currentRow())
}