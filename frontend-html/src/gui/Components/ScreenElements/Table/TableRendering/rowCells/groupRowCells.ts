import {IGroupRow, ITableRow} from "../types";
import {selectionCheckboxEmptyCellsDraws, selectionCheckboxEmptyCellsWidths,} from "../cells/selectionCheckboxCell";
import {groupRowContentCellsDraws, groupRowContentCellsWidths, groupRowEmptyCellsWidths} from "../cells/groupCell";
import {currentRow} from "../renderingValues";
import {aggregationCellDraws, aggregationColumnsWidths} from "../cells/aggregationCell";

export function groupRowCellsWidths() {
  return [
      [...selectionCheckboxEmptyCellsWidths(), ...groupRowContentCellsWidths()],
      [...selectionCheckboxEmptyCellsWidths(), ...groupRowEmptyCellsWidths(), ...aggregationColumnsWidths()]
  ];
}

export function groupRowCellsDraws() {
  return [
      [...selectionCheckboxEmptyCellsDraws(), ...groupRowContentCellsDraws()],
      [...selectionCheckboxEmptyCellsDraws().map(x => ()=>{}), ...aggregationCellDraws()]
  ];
}

export function isGroupRow(row: ITableRow): row is IGroupRow {
  return (row as any).groupLevel !== undefined;
}

export function isCurrentGroupRow() {
  return isGroupRow(currentRow());
}
