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

import {
  context,
  context2d,
  currentRow,
  gridLeadCellDimensions,
  groupingColumnCount,
  groupingColumnIds,
  isCheckBoxedTable,
  recordId,
  tablePanelView,
  worldWidth,
} from "../renderingValues";
import { currentColumnLeft, currentColumnWidth, currentRowHeight, currentRowTop, } from "../currentCell";
import { IGroupRow, IGroupTreeNode } from "../types";
import { applyScrollTranslation, cellPaddingLeft, fontSize, selectRowBorderWidth, topTextOffset } from "./cellsCommon";
import { isGroupRow } from "../rowCells/groupRowCells";
import { onClick } from "../onClick";
import { CPR } from "utils/canvas";
import { onGroupHeaderToggleClick } from "model/actions-ui/DataView/TableView/onGroupHeaderToggleClick";
import { flow } from "mobx";
import { isInfiniteScrollingActive } from "model/selectors/isInfiniteScrollingActive";
import { getGrouper } from "model/selectors/DataView/getGrouper";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getSelectedRowId } from "../../../../../../model/selectors/TablePanelView/getSelectedRowId";

const groupCellWidth = 20;
const expandSymbolFontSize = 15;

export function groupRowEmptyCellsWidths() {
  return groupingColumnIds().map(() => groupCellWidth);
}

export function groupRowEmptyCellsDraws() {
  return groupingColumnIds().map((id) => () => {
    drawEmptyGroupCell();
  });
}

export function groupRowContentCellsWidths() {
  const row = currentRow();
  if (isGroupRow(row)) {
    const grpCnt = groupingColumnCount();
    const result: number[] = [];
    for (let i = 0; i < grpCnt; i++) {
      if (i < row.groupLevel) result.push(groupCellWidth);
      else {
        result.push(worldWidth() - gridLeadCellDimensions()[i].left);
        break;
      }
    }
    return result;
  } else return [];
}

export function groupRowContentCellsDraws() {
  const row = currentRow();
  if (isGroupRow(row)) {
    const grpCnt = groupingColumnCount();
    const result: (() => void)[] = [];
    for (let i = 0; i < grpCnt; i++) {
      if (i < row.groupLevel) result.push(drawEmptyGroupCell);
      else {
        result.push(drawGroupCell);
        break;
      }
    }
    return result;
  } else return [];
}

export function drawEmptyGroupCell() {
  applyScrollTranslation();
  // clipCell();
  drawEmptyGroupCellBackground();
}

export function drawGroupCell() {
  applyScrollTranslation();
  drawGroupCellBackground();

  const ctx2d = context2d();
  const row = currentRow();
  if (isGroupRow(row)) {
    const groupRow = row as IGroupRow;
    ctx2d.fillStyle = "black";
    ctx2d.font = `${CPR() * expandSymbolFontSize}px "Font Awesome 5 Free"`;
    const state = row.isExpanded;
    ctx2d.fillText(
      state ? "\uf146" : "\uf0fe",
      CPR() * (currentColumnLeft() + 6),
      CPR() * (currentRowTop() + topTextOffset + 1)
    );
    ctx2d.font = `${CPR() * fontSize}px "IBM Plex Sans", Arial, sans-serif`;
    ctx2d.fillText(
      `${row.columnLabel} : ${formatColumnValue(row.columnValue)} [${row.sourceGroup.rowCount}]`,
      CPR() * (currentColumnLeft() + cellPaddingLeft + groupCellWidth),
      CPR() * (currentRowTop() + topTextOffset)
    );

    const ctx = context();
    onClick({
      x: currentColumnLeft(),
      y: currentRowTop(),
      w: groupCellWidth,
      h: 20,
      async handler(event: any) {
        await flow(function*() {
          if (!row.sourceGroup.isExpanded && row.sourceGroup.childGroups.length === 0) {
            yield onGroupHeaderToggleClick(ctx)(event, groupRow);
          }
          const allGroups = getGrouper(ctx).allGroups;
          if (shouldCloseOtherGroups(row.sourceGroup, allGroups, ctx)) {
            const groupsToKeepOpen = [row.sourceGroup, ...row.sourceGroup.allParents]
            allGroups
              .filter(group => !groupsToKeepOpen.includes(group) && group.isExpanded)
              .forEach(group => group.isExpanded = false);
          }
          row.sourceGroup.isExpanded = !row.sourceGroup.isExpanded;

          yield*unselectRowIfInClosedGroup(ctx, row);
        })();
      },
    });
  }
}

function* unselectRowIfInClosedGroup(ctx: any, row: IGroupRow): Generator {
  const dataView = getDataView(ctx);
  if (!row.sourceGroup.isExpanded && dataView.selectedRowId) {
    const containsSelectedRow = !!row.sourceGroup.getRowById(dataView.selectedRowId);
    if (containsSelectedRow) {
      yield*dataView.setSelectedRowId(undefined);
    }
  }
}

function shouldCloseOtherGroups(sourceGroup: IGroupTreeNode, allGroups: IGroupTreeNode[], ctx: any) {
  const otherGroups = Array.from(allGroups).remove(sourceGroup);
  return isInfiniteScrollingActive(ctx, undefined)
    && otherGroups.some(group => group.isInfinitelyScrolled);
}

function formatColumnValue(value: string) {
  if (value === undefined || value === null) {
    return "<empty>";
  } else {
    return value
  }
}

export function drawGroupCellBackground() {
  const ctx2d = context2d();
  ctx2d.fillStyle = getComputedStyle(document.documentElement).getPropertyValue('--background4');
  ctx2d.fillRect(
    CPR() * currentColumnLeft(),
    CPR() * currentRowTop(),
    CPR() * currentColumnWidth(),
    CPR() * currentRowHeight()
  );
}

export function drawEmptyGroupCellBackground() {
  const ctx2d = context2d();
  ctx2d.fillStyle = getComputedStyle(document.documentElement).getPropertyValue('--background1');
  ctx2d.fillRect(
    CPR() * currentColumnLeft(),
    CPR() * currentRowTop(),
    CPR() * currentColumnWidth(),
    CPR() * currentRowHeight()
  );
  const selectedRowId = getSelectedRowId(tablePanelView());
  const isSelected = recordId() === selectedRowId;

  if (isSelected && isCheckBoxedTable()) {
    drawSelectedRowBorder();
  }
}

function drawSelectedRowBorder() {
  const ctx2d = context2d();
  ctx2d.beginPath();
  ctx2d.strokeStyle = getComputedStyle(document.documentElement).getPropertyValue('--foreground1');
  ctx2d.lineWidth = selectRowBorderWidth * CPR();
  ctx2d.moveTo(CPR() * currentColumnLeft(), CPR() * (currentRowTop() + 1.5));
  ctx2d.lineTo(
    CPR() * currentColumnLeft() + CPR() * currentColumnWidth(),
    CPR() * (currentRowTop() + 1.5)
  );
  ctx2d.moveTo(CPR() * currentColumnLeft(), CPR() * (currentRowTop() + currentRowHeight() - 1.5));
  ctx2d.lineTo(
    CPR() * currentColumnLeft() + CPR() * currentColumnWidth(),
    CPR() * (currentRowTop() + currentRowHeight() - 1.5)
  );
  ctx2d.stroke();
}
