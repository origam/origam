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

import { Memoized } from "./common/Memoized";
import { isCheckBoxedTable, scRenderRow, scrollLeft } from "./renderingValues";
import { dataRowCellsDraws, dataRowCellsWidths, isCurrentDataRow } from "./rowCells/dataRowCells";
import { groupRowCellsDraws, groupRowCellsWidths, isCurrentGroupRow, } from "./rowCells/groupRowCells";
import { currentCellLayerIndex } from "./currentCellLayerIndex";

function computeDimensions(cellWidths: number[]) {
  const result: { left: number; width: number; right: number; leftVisible: number; widthVisible: number; }[] = [];

  let acc = 0;
  for (let i = 0; i < cellWidths.length; i++) {
    const left = acc;
    const width = cellWidths[i];
    let leftVisible = left;
    let widthVisible = width;
    if (isCheckBoxedTable() && scrollLeft() > left) {
      if (i > 0) {
        const fixedColumnWidth = cellWidths[0]
        leftVisible = scrollLeft() + fixedColumnWidth
        widthVisible = width - (leftVisible - left)
      } else if (i === 0) {
        leftVisible = scrollLeft()
      }
    }
    acc = acc + width;
    const right = acc;
    result.push({left, width, right, leftVisible, widthVisible});
  }
  return result;
}

export const currentRowCellsDimensions = Memoized(() => {
  if (isCurrentDataRow()) {
    return computeDimensions(dataRowCellsWidths());
  } else if (isCurrentGroupRow()) {
    return computeDimensions(groupRowCellsWidths()[currentCellLayerIndex()]);
  } else {
    return [];
  }
});
scRenderRow.push(() => currentRowCellsDimensions.clear());

export const currentRowCellsDraws = Memoized(() => {
  if (isCurrentDataRow()) {
    return dataRowCellsDraws();
  } else if (isCurrentGroupRow()) {
    return groupRowCellsDraws()[currentCellLayerIndex()];
  } else {
    return [];
  }
});
scRenderRow.push(currentRowCellsDraws.clear);

export function currentRowCellsCount() {
  return currentRowCellsDraws().length;
}
