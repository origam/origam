import {
  groupingColumnIds,
  groupingColumnCount,
  context2d,
  worldWidth,
  gridLeadCellDimensions,
  currentRow, context,
} from "../renderingValues";
import {
  currentColumnLeft,
  currentRowTop,
  currentColumnWidth,
  currentRowHeight,
} from "../currentCell";
import { ITableRow, IGroupRow } from "../types";
import { applyScrollTranslation, clipCell } from "./cellsCommon";
import { isGroupRow } from "../rowCells/groupRowCells";
import { onClick } from "../onClick";
import { CPR } from "utils/canvas";
import {onGroupHeaderToggleClick} from "../../../../../../model/actions-ui/DataView/TableView/onGroupHeaderToggleClick";
import {flow} from "mobx";
import {getTablePanelView} from "../../../../../../model/selectors/TablePanelView/getTablePanelView";
import {onPossibleSelectedRowChange} from "../../../../../../model/actions-ui/onPossibleSelectedRowChange";
import {getMenuItemId} from "../../../../../../model/selectors/getMenuItemId";
import {getDataStructureEntityId} from "../../../../../../model/selectors/DataView/getDataStructureEntityId";
import {getSelectedRowId} from "../../../../../../model/selectors/TablePanelView/getSelectedRowId";
import {lastClickedCellRectangle} from "./dataCell";

export function groupRowEmptyCellsWidths() {
  return groupingColumnIds().map(() => 20);
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
      if (i < row.groupLevel) result.push(20);
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
    ctx2d.font = `${CPR * 15}px "Font Awesome 5 Free"`;
    const state = row.isExpanded;
    ctx2d.fillText(
        state ? "\uf146" : "\uf0fe",
        CPR * (currentColumnLeft() + 2),
        CPR * (currentRowTop() + 17)
    );
    ctx2d.font = `${CPR * 12}px "IBM Plex Sans", Arial, sans-serif`;
    ctx2d.fillText(
        `${row.columnLabel} : ${row.columnValue}`,
        CPR * (currentColumnLeft() + 2 + 20),
        CPR * (currentRowTop() + 17)
    );

    const ctx = context();
    onClick({
      x: currentColumnLeft(),
      y: currentRowTop(),
      w: 20,
      h: 20,
      handler(event: any) {
        flow(function* () {
          console.log("click");
          if(! row.sourceGroup.isExpanded && row.sourceGroup.childGroups.length === 0){
            yield onGroupHeaderToggleClick(ctx)(event, groupRow);
          }
          row.sourceGroup.isExpanded = !row.sourceGroup.isExpanded;
        })();
      },
    });
  }
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
