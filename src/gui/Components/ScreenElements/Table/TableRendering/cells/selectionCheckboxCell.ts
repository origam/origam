import { isCheckboxedTable, context2d } from "../renderingValues";
import {
  currentColumnLeft,
  currentRowTop,
  currentColumnWidth,
  currentRowHeight,
} from "../currentCell";
import { applyScrollTranslation } from "./cellsCommon";
import { CPR } from "utils/canvas";

export function selectionCheckboxCellsWidths() {
  return isCheckboxedTable() ? [20] : [];
}

export function selectionCheckboxCellsDraws() {
  return isCheckboxedTable()
    ? [
        () => {
          applyScrollTranslation();
          drawSelectionCheckboxBackground();
          const ctx2d = context2d();
          ctx2d.fillStyle = "black";
          ctx2d.font = `${CPR*15}px "Font Awesome 5 Free"`;
          const state = true;
          ctx2d.fillText(
            state ? "\uf14a" : "\uf0c8",
            CPR * (currentColumnLeft() + 2),
            CPR * (currentRowTop() + 17)
          );
        },
      ]
    : [];
}

export function selectionCheckboxEmptyCellsWidths() {
  return isCheckboxedTable() ? [20] : [];
}

export function selectionCheckboxEmptyCellsDraws() {
  return isCheckboxedTable()
    ? [
        () => {
          applyScrollTranslation();
          drawSelectionCheckboxBackground();
        },
      ]
    : [];
}

export function drawSelectionCheckboxBackground() {
  const ctx2d = context2d();
  ctx2d.fillStyle = "#ffffff";
  ctx2d.fillRect(
    CPR * currentColumnLeft(),
    CPR * currentRowTop(),
    CPR * currentColumnWidth(),
    CPR * currentRowHeight()
  );
}

export function drawSelectionCheckboxContent() {
  const ctx2d = context2d();
  applyScrollTranslation();
  drawSelectionCheckboxBackground();
  ctx2d.fillStyle = "black";
  ctx2d.font = `${CPR*15}px "Font Awesome 5 Free"`;
  const state = true;
  ctx2d.fillText(
    state ? "\uf14a" : "\uf0c8",
    CPR * (currentColumnLeft() + 2),
    CPR * (currentRowTop() + 17)
  );
}
