import {
  context2d,
  drawingColumnIndex,
  gridLeadCellDimensions,
  scRenderCell,
  scrollLeft,
  scrollTop
} from "../renderingValues";
import {
  isCurrentCellFixed,
  currentColumnLeft,
  currentRowTop,
  currentColumnWidth,
  currentRowHeight,
} from "../currentCell";
import { CPR } from "utils/canvas";
import {ValueBox} from "../common/ValueBox";
import {Memoized} from "../common/Memoized";

export function applyScrollTranslation() {
  const ctx2d = context2d();
  ctx2d.translate(!isCurrentCellFixed() ? -CPR * scrollLeft() : 0, -CPR * scrollTop());
}

export function clipCell() {
  const ctx2d = context2d();
  ctx2d.beginPath();
  ctx2d.rect(
    CPR * currentColumnLeft(),
    CPR * currentRowTop(),
    CPR * currentColumnWidth(),
    CPR * currentRowHeight()
  );
  ctx2d.clip();
}

export const cellPaddingLeft = Memoized(() =>  drawingColumnIndex() === 0 ? 25 : 15)
scRenderCell.push(() => cellPaddingLeft.clear());