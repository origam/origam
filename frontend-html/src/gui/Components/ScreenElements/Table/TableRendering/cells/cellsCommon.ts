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
  context2d,
  dataCellOffset,
  drawingColumnIndex,
  getCurrentRowRightBorderDrawn,
  scrollLeft,
  scrollTop,
  selectionColumnShown,
  setCurrentRowRightBorderDrawn,
} from "../renderingValues";
import {
  currentColumnLeft,
  currentColumnRight,
  currentColumnWidth,
  currentRowHeight,
  currentRowTop,
  isCurrentCellFixed,
} from "../currentCell";
import { CPR } from "utils/canvas";
import { getPaddingLeft, getPaddingRight } from "./dataCellRenderer";

export function applyScrollTranslation() {
  const ctx2d = context2d();
  ctx2d.translate(!isCurrentCellFixed() ? -CPR() * scrollLeft() : 0, -CPR() * scrollTop());
}

export function clipCell() {
  const ctx2d = context2d();
  ctx2d.beginPath();
  ctx2d.rect(
    CPR() * (currentColumnLeft() + getPaddingLeft()),
    CPR() * currentRowTop(),
    CPR() * (currentColumnWidth() - getPaddingLeft() - getPaddingRight()),
    CPR() * currentRowHeight()
  );
  ctx2d.clip();
}

export function drawSelectedRowBorder(frontStripeWidth: number) {
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

  const currentIsTheLeftMostDataColumn = drawingColumnIndex() === dataCellOffset();

  if (currentIsTheLeftMostDataColumn && !selectionColumnShown()) {
    ctx2d.beginPath();
    ctx2d.lineWidth = frontStripeWidth * CPR();
    ctx2d.moveTo(CPR() * currentColumnLeft(), CPR() * (currentRowTop() + 1.5));
    ctx2d.lineTo(CPR() * currentColumnLeft(), CPR() * (currentRowTop() + currentRowHeight() - 1.5));
    ctx2d.stroke();
  }
  if (!getCurrentRowRightBorderDrawn()) {
    ctx2d.beginPath();
    ctx2d.lineWidth = selectRowBorderWidth * CPR();
    ctx2d.moveTo(CPR() * currentColumnRight(), CPR() * (currentRowTop() + 1.5));
    ctx2d.lineTo(CPR() * currentColumnRight(), CPR() * (currentRowTop() + currentRowHeight() - 1.5));
    ctx2d.stroke();
    setCurrentRowRightBorderDrawn(true);
  }
}

export const numberCellPaddingRight = 15;
export const cellPaddingLeft = parseInt(getComputedStyle(document.documentElement)
  .getPropertyValue('--cellLeftPadding'));;
export const cellPaddingRight = parseInt(getComputedStyle(document.documentElement)
  .getPropertyValue('--cellRightPadding'));;
export const cellPaddingLeftFirstCell = 25;
export const cellPaddingRightFirstCell = 25;
export const topTextOffset = 17;
export const fontSize = 12;
export const checkBoxCellPaddingLeft = 5;
export const checkBoxCharacterFontSize = 12;
export const rowHeight = 25;
export const frontStripWidth = 8;
export const selectRowBorderWidth = 1;

