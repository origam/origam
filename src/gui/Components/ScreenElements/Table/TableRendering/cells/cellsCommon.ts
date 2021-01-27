import {context2d, drawingColumnIndex, scRenderCell, scrollLeft, scrollTop} from "../renderingValues";
import {
  currentColumnLeft,
  currentColumnWidth,
  currentRowHeight,
  currentRowTop,
  isCurrentCellFixed,
} from "../currentCell";
import {CPR} from "utils/canvas";
import {Memoized} from "../common/Memoized";

export function applyScrollTranslation() {
  const ctx2d = context2d();
  ctx2d.translate(!isCurrentCellFixed() ? -CPR() * scrollLeft() : 0, -CPR() * scrollTop());
}

export function clipCell() {
  const ctx2d = context2d();
  ctx2d.beginPath();
  ctx2d.rect(
    CPR() * currentColumnLeft(),
    CPR() * currentRowTop(),
    CPR() * currentColumnWidth(),
    CPR() * currentRowHeight()
  );
  ctx2d.clip();
}


export function drawSelectedRowBorder(frontStripeWidth: number){
  const ctx2d = context2d();
  ctx2d.beginPath();
  ctx2d.strokeStyle = "#4C84FF";
  ctx2d.lineWidth = 1 * CPR();
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
  if (drawingColumnIndex() === 0) {
    ctx2d.beginPath();
    ctx2d.lineWidth = frontStripeWidth * CPR();
    ctx2d.moveTo(CPR() * currentColumnLeft(), CPR() * (currentRowTop() + 1.5));
    ctx2d.lineTo(CPR() * currentColumnLeft(), CPR() * (currentRowTop() + currentRowHeight() - 1.5));
    ctx2d.stroke();
  }
}


export const numberCellPaddingRight = Memoized(() => 15)
scRenderCell.push(() => numberCellPaddingRight.clear());

export const cellPaddingLeft = 5
export const cellPaddingLeftFirstCell = 25;
export const cellPaddingRightFirstCell = 25;
export const topTextOffset = 17;
export const fontSize = 12;
export const checkSymbolFontSize = 15;
export const checkBoxCellPaddingLeft = 3;
export const checkBoxCharacterFontSize = 14;
export const rowHeight = 20;
