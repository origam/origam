import {
  context,
  context2d,
  currentRow,
  gridLeadCellDimensions,
  groupingColumnCount,
  groupingColumnIds,
  worldWidth,
} from "../renderingValues";
import {currentColumnLeft, currentColumnWidth, currentRowHeight, currentRowTop,} from "../currentCell";
import {IGroupRow, IGroupTreeNode} from "../types";
import {applyScrollTranslation, cellPaddingLeft, clipCell, fontSize, topTextOffset} from "./cellsCommon";
import {isGroupRow} from "../rowCells/groupRowCells";
import {onClick} from "../onClick";
import {CPR} from "utils/canvas";
import {onGroupHeaderToggleClick} from "../../../../../../model/actions-ui/DataView/TableView/onGroupHeaderToggleClick";
import {flow, runInAction} from "mobx";
import {getDataTable} from "../../../../../../model/selectors/DataView/getDataTable";
import {isInfiniteScrollingActive} from "../../../../../../model/selectors/isInfiniteScrollingActive";
import {SCROLL_ROW_CHUNK} from "../../../../../Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import { getGrouper } from "model/selectors/DataView/getGrouper";

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
  clipCell();
  drawEmptyGroupCellBackground();
}

export function drawGroupCell() {
  applyScrollTranslation();
  clipCell();
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
      CPR() * (currentColumnLeft() + cellPaddingLeft),
      CPR() * (currentRowTop() + topTextOffset)
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
      handler(event: any) {
        flow(function* () {
          if (!row.sourceGroup.isExpanded && row.sourceGroup.childGroups.length === 0) {
            yield onGroupHeaderToggleClick(ctx)(event, groupRow);
          }
          runInAction(()=> {
            const allGroups = getGrouper(ctx).allGroups;
            if (shouldCloseOtherGroups(allGroups, ctx)){
              const groupsToKeepOpen = [row.sourceGroup, ...row.sourceGroup.allParents]
              allGroups
                .filter(group => !groupsToKeepOpen.includes(group) && group.isExpanded)
                .forEach(group => group.isExpanded = false);
            }
            row.sourceGroup.isExpanded = !row.sourceGroup.isExpanded;
          });
        })();
      },
    });
  }
}

function shouldCloseOtherGroups(groups: IGroupTreeNode[], ctx: any){
  return isInfiniteScrollingActive(ctx, undefined)
    && groups.some(group => group.isInfinitelyScrolled);
}

function formatColumnValue(value: string){
  if(value === undefined || value === null){
    return "<empty>";
  }else{
    return value
  }
}

export function drawGroupCellBackground() {
  const ctx2d = context2d();
  ctx2d.fillStyle = "#cccccc";
  ctx2d.fillRect(
    CPR() * currentColumnLeft(),
    CPR() * currentRowTop(),
    CPR() * currentColumnWidth(),
    CPR() * currentRowHeight()
  );
}

export function drawEmptyGroupCellBackground() {
  const ctx2d = context2d();
  ctx2d.fillStyle = "#ffffff";
  ctx2d.fillRect(
    CPR() * currentColumnLeft(),
    CPR() * currentRowTop(),
    CPR() * currentColumnWidth(),
    CPR() * currentRowHeight()
  );
}
