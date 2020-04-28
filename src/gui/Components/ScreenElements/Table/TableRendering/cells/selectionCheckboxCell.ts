import { isCheckBoxedTable, context2d, context, rowIndex, drawingColumnIndex, dataTable, rowId, dataView, currentDataRow } from "../renderingValues";
import {
  currentColumnLeft,
  currentRowTop,
  currentColumnWidth,
  currentRowHeight,
  currentColumnLeftVisible,
  currentColumnWidthVisible,
} from "../currentCell";
import { applyScrollTranslation } from "./cellsCommon";
import { CPR } from "utils/canvas";
import { onClick } from "../onClick";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getSelectionMember } from "model/selectors/DataView/getSelectionMember";
import { getDataSourceFieldByName } from "model/selectors/DataSources/getDataSourceFieldByName";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import actions from "model/actions-tree";
import { flow } from "mobx";
import { getDataView } from "model/selectors/DataView/getDataView";

export const selectionCheckBoxColumnWidth = 20;

export function selectionCheckboxCellsWidths() {
  return isCheckBoxedTable() ? [selectionCheckBoxColumnWidth] : [];
}

export function selectionCheckboxCellsDraws() {
  return isCheckBoxedTable()
    ? [
      () => {
        applyScrollTranslation();
        drawSelectionCheckboxBackground();
        const ctx2d = context2d();
        ctx2d.fillStyle = "black";
        ctx2d.font = `${CPR * 15}px "Font Awesome 5 Free"`;
        const state = dataView().isSelected(rowId());
        ctx2d.fillText(
          state ? "\uf14a" : "\uf0c8",
          CPR * (currentColumnLeft() + 2),
          CPR * (currentRowTop() + 17)
        );
        registerClickHandler();
      },
    ]
    : [];
}

function registerClickHandler() {
  const ctx = context();
  //const cellRowIndex = rowIndex();
  const row = currentDataRow();
  onClick({
    x: currentColumnLeftVisible(),
    y: currentRowTop(),
    w: currentColumnWidthVisible(),
    h: currentRowHeight(),
    handler(event: any) {
      flow(function* () {
        console.log("click");

        // TODO: Move to tablePanelView
        const dataTable = getDataTable(ctx);
        const selectionMember = getSelectionMember(ctx);
        if (!!selectionMember) {
          const dsField = getDataSourceFieldByName(ctx, selectionMember);
          if (dsField) {
            const value = dataTable.getCellValueByDataSourceField(row, dsField);
            dataTable.setDirtyValue(row, selectionMember, !value);
            yield* getFormScreenLifecycle(ctx).onFlushData();
          }
        } else {
          yield* actions.selectionCheckboxes.toggleSelectedId(ctx)(
            dataTable.getRowId(row)
          );
          return;
        }
      })();
    },
  });
}

export function selectionCheckboxEmptyCellsWidths() {
  return isCheckBoxedTable() ? [selectionCheckBoxColumnWidth] : [];
}

export function selectionCheckboxEmptyCellsDraws() {
  return isCheckBoxedTable()
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
  ctx2d.font = `${CPR * 15}px "Font Awesome 5 Free"`;
  const state = true;
  ctx2d.fillText(
    state ? "\uf14a" : "\uf0c8",
    CPR * (currentColumnLeft() + 2),
    CPR * (currentRowTop() + 17)
  );
}
