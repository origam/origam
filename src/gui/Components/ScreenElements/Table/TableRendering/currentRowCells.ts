import { Memoized } from "./common/Memoized";
import { scRenderRow } from "./renderingValues";
import { dataRowCellsDraws, dataRowCellsWidths, isCurrentDataRow } from "./rowCells/dataRowCells";
import {
  groupRowCellsDraws,
  groupRowCellsWidths,
  isCurrentGroupRow,
} from "./rowCells/groupRowCells";

function computeDimensions(cellWidths: number[]) {
  const result: { left: number; width: number; right: number }[] = [];
  let acc = 0;
  for (let i = 0; i < cellWidths.length; i++) {
    const left = acc;
    const width = cellWidths[i];
    acc = acc + width;
    const right = acc;
    result.push({ left, width, right });
  }
  return result;
}

export const currentRowCellsDimensions = Memoized(() => {
  if (isCurrentDataRow()) {
    return computeDimensions(dataRowCellsWidths());
  } else if (isCurrentGroupRow()) {
    return computeDimensions(groupRowCellsWidths());
  } else {
    return [];
  }
});
scRenderRow.push(() => currentRowCellsDimensions.clear());

export const currentRowCellsDraws = Memoized(() => {
  if (isCurrentDataRow()) {
    return dataRowCellsDraws();
  } else if (isCurrentGroupRow()) {
    return groupRowCellsDraws();
  } else {
    return [];
  }
});
scRenderRow.push(currentRowCellsDraws.clear);

export function currentRowCellsCount() {
  return currentRowCellsDraws().length;
}
