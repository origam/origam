import { ITableRow, IGroupRow } from "../types";
import {
  selectionCheckboxEmptyCellsWidths,
  selectionCheckboxEmptyCellsDraws,
} from "../cells/selectionCheckboxCell";
import {groupRowContentCellsWidths, groupRowContentCellsDraws, groupRowEmptyCellsWidths} from "../cells/groupCell";
import { currentRow } from "../renderingValues";
import {aggregationCellDraws} from "../cells/aggregationCell";
import {dataColumnsWidths} from "../cells/dataCell";

export function groupRowCellsWidths() {
  return [
      [...selectionCheckboxEmptyCellsWidths(), ...groupRowContentCellsWidths()],
      [...selectionCheckboxEmptyCellsWidths(), ...groupRowEmptyCellsWidths(), ...dataColumnsWidths()]
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
