import { tableColumnIds, columnWidths, context2d } from "../renderingValues";
import {
  currentColumnLeft,
  currentRowTop,
  currentCellText,
  currentColumnWidth,
  currentRowHeight,
} from "../currentCell";
import { applyScrollTranslation, clipCell } from "./cellsCommon";
import { CPR } from "utils/canvas";

export function dataColumnsWidths() {
  return tableColumnIds().map((id) => columnWidths().get(id) || 100);
}

export function dataColumnsDraws() {
  return tableColumnIds().map((id) => () => {
    const ctx2d = context2d();
    applyScrollTranslation();
    clipCell();
    drawDataCellBackground();
    ctx2d.fillStyle = "black";
    ctx2d.font = `${CPR*12}px "IBM Plex Sans", Arial, sans-serif`;
    ctx2d.fillText(
      currentCellText(),
      CPR * (currentColumnLeft() + 2),
      CPR * (currentRowTop() + 17)
    );
  });
}

export function drawDataCellBackground() {
  const ctx2d = context2d();
  ctx2d.fillStyle = "#eeeeee";
  ctx2d.fillRect(
    CPR * currentColumnLeft(),
    CPR * currentRowTop(),
    CPR * currentColumnWidth(),
    CPR * currentRowHeight()
  );
}
