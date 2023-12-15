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

import { context2d, currentRow, recordId, tableColumnIds, tablePanelView } from "../renderingValues";
import { applyScrollTranslation, clipCell, fontSize, numberCellPaddingRight, topTextOffset } from "./cellsCommon";
import { getRowStateAllowRead } from "model/selectors/RowState/getRowStateAllowRead";
import { currentColumnId, currentColumnLeft, currentColumnWidth, currentProperty, currentRowTop } from "../currentCell";
import { CPR } from "utils/canvas";
import { getGroupLevelCount, isGroupRow } from "../rowCells/groupRowCells";
import { IGroupRow } from "../types";
import { dataColumnsWidths } from "./dataCell";
import { aggregationToString } from "model/entities/Aggregatioins";
import { v4 as uuidv4 } from "uuid";


export function aggregationColumnsWidths() {
  return dataColumnsWidths();
}

export function aggregationCellDraws() {
  const row = currentRow();
  if (isGroupRow(row)) {
    const groupLevelCount = getGroupLevelCount(row);
    const dummyIds = [...Array(groupLevelCount + 1).keys()]
      .map(x => "dummy_" + uuidv4().toString());
    return [...dummyIds, ...tableColumnIds()].map((id) => () => drawAggregationCell());
  } else return [];
}

export function drawAggregationCell() {
  applyScrollTranslation();
  clipCell();
  drawAggregationText();
}

function drawAggregationText() {
  const ctx2d = context2d();
  if (!currentColumnId()) return;
  const isHidden = !getRowStateAllowRead(tablePanelView(), recordId(), currentProperty().id)
  const row = currentRow();
  if (isHidden || !isGroupRow(row)) {
    return;
  }
  const groupRow = row as IGroupRow;
  if (!groupRow.sourceGroup.aggregations) {
    return;
  }
  const aggregation = groupRow.sourceGroup.aggregations
    .find(aggregation => aggregation.columnId === currentProperty().id)
  if (!aggregation) {
    return;
  }

  ctx2d.font = `${fontSize * CPR()}px "IBM Plex Sans", sans-serif`;
  ctx2d.fillStyle = "black";
  ctx2d.textAlign = "right";
  ctx2d.fillText(
    aggregationToString(aggregation, currentProperty()),
    CPR() * (currentColumnLeft() + currentColumnWidth() - numberCellPaddingRight),
    CPR() * (currentRowTop() + topTextOffset));
}
