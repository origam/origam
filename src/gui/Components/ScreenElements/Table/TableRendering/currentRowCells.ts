import { Memoized } from "./common/Memoized";
import { scRenderRow, isCheckBoxedTable, scrollLeft } from "./renderingValues";
import { dataRowCellsDraws, dataRowCellsWidths, isCurrentDataRow } from "./rowCells/dataRowCells";
import {
  groupRowCellsDraws,
  groupRowCellsWidths,
  isCurrentGroupRow,
} from "./rowCells/groupRowCells";

function computeDimensions(cellWidths: number[]) {
  const result: { left: number; width: number; right: number; leftVisible: number; widthVisible: number; }[] = [];

  let acc = 0;
  for (let i = 0; i < cellWidths.length; i++) {
    const left = acc;
    const width = cellWidths[i];
    let leftVisible = left;
    let widthVisible = width;
    if(isCheckBoxedTable() && scrollLeft() > left){
      if(i > 0){
        const fixedColumnWidth = cellWidths[0] 
        leftVisible = scrollLeft() + fixedColumnWidth
        widthVisible = width - (leftVisible - left)
      }
      else if(i === 0){
        leftVisible = scrollLeft()
      }
    }
    acc = acc + width;
    const right = acc;
    result.push({ left, width, right, leftVisible, widthVisible});
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
