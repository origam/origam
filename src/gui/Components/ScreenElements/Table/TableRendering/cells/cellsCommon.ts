import { context2d, scrollLeft, scrollTop } from "../renderingValues";
import {
  isCurrentCellFixed,
  currentColumnLeft,
  currentRowTop,
  currentColumnWidth,
  currentRowHeight,
} from "../currentCell";
import { CPR } from "utils/canvas";

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
