import { dataColumnsDraws, dataColumnsWidths } from "../cells/dataCell";
import { groupRowEmptyCellsDraws, groupRowEmptyCellsWidths } from "../cells/groupCell";
import { selectionCheckboxCellsDraws, selectionCheckboxCellsWidths } from "../cells/selectionCheckboxCell";
import { Memoized } from "../common/Memoized";
import { currentRow } from "../currentCell";
import { scRenderRow, tableColumnIds, scRenderTable } from "../renderingValues";
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