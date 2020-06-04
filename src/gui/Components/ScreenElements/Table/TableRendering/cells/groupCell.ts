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
import {SCROLL_DATA_INCREMENT_SIZE} from "../../../../../Workbench/ScreenArea/TableView/InfiniteScrollLoader";

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
    ctx2d.font = `${CPR * expandSymbolFontSize}px "Font Awesome 5 Free"`;
    const state = row.isExpanded;
    ctx2d.fillText(
      state ? "\uf146" : "\uf0fe",
      CPR * (currentColumnLeft() + cellPaddingLeft),
      CPR * (currentRowTop() + topTextOffset)
    );
    ctx2d.font = `${CPR * fontSize}px "IBM Plex Sans", Arial, sans-serif`;
    ctx2d.fillText(
      `${row.columnLabel} : ${formatColumnValue(row.columnValue)} [${row.sourceGroup.rowCount}]`,
      CPR * (currentColumnLeft() + cellPaddingLeft + groupCellWidth),
      CPR * (currentRowTop() + topTextOffset)
    );

    const ctx = context();
    onClick({
      x: currentColumnLeft(),
      y: currentRowTop(),
      w: groupCellWidth,
      h: 20,
      handler(event: any) {
        flow(function* () {
          console.log("click");
          if (!row.sourceGroup.isExpanded && row.sourceGroup.childGroups.length === 0) {
            yield onGroupHeaderToggleClick(ctx)(event, groupRow);
          }
          runInAction(()=> {
            if (shouldCloseOtherGroups(row.sourceGroup, ctx)){
              for (let group of getDataTable(ctx).groups) {
                group.isExpanded = false;
              }
            }
            row.sourceGroup.isExpanded = !row.sourceGroup.isExpanded;
          });
        })();
      },
    });
  }
}

function shouldCloseOtherGroups(clickedGroup: IGroupTreeNode, ctx: any){
  const groups = getDataTable(ctx).groups;
  const someInfinitelyScrolledGroupsAreExpanded = groups
    .some(group => group.rowCount >= SCROLL_DATA_INCREMENT_SIZE && group.isExpanded);
  return !clickedGroup.isExpanded
    && isInfiniteScrollingActive(ctx, undefined)
    && (clickedGroup.rowCount >= SCROLL_DATA_INCREMENT_SIZE || someInfinitelyScrolledGroupsAreExpanded);
}

function formatColumnValue(value: string){
  return value || "<empty>"
}

export function drawGroupCellBackground() {
  const ctx2d = context2d();
  ctx2d.fillStyle = "#cccccc";
  ctx2d.fillRect(
    CPR * currentColumnLeft(),
    CPR * currentRowTop(),
    CPR * currentColumnWidth(),
    CPR * currentRowHeight()
  );
}

export function drawEmptyGroupCellBackground() {
  const ctx2d = context2d();
  ctx2d.fillStyle = "#ffffff";
  ctx2d.fillRect(
    CPR * currentColumnLeft(),
    CPR * currentRowTop(),
    CPR * currentColumnWidth(),
    CPR * currentRowHeight()
  );
}
