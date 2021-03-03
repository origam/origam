import { flow } from "mobx";
import { onPossibleSelectedRowChange } from "model/actions-ui/onPossibleSelectedRowChange";
import selectors from "model/selectors-tree";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getRowStateAllowRead } from "model/selectors/RowState/getRowStateAllowRead";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { CPR } from "utils/canvas";
import { shadeHexColor } from "utils/colorUtils";
import actionsUi from "model/actions-ui-tree";
import { getDataView } from "model/selectors/DataView/getDataView";
import { getShowToolTipsForMemoFieldsOnly } from "model/selectors/Workbench/getShowToolTipsForMemoFieldsOnly";
import {
  currentCellErrorMessage,
  currentCellText,
  currentColumnLeft,
  currentColumnLeftVisible,
  currentColumnWidth,
  currentColumnWidthVisible,
  currentProperty,
  currentRowHeight,
  currentRowTop,
  isCurrentCellLoading,
} from "../currentCell";
import { onClick, onMouseOver } from "../onClick";
import {
  columnWidths,
  context,
  context2d,
  currentDataRow,
  drawingColumnIndex,
  recordId,
  rowIndex,
  tableColumnIds,
  tablePanelView,
} from "../renderingValues";
import {
  applyScrollTranslation,
  checkBoxCharacterFontSize,
  clipCell,
  drawSelectedRowBorder,
  fontSize,
  topTextOffset,
} from "./cellsCommon";
import {
  currentDataCellRenderer,
  getPaddingLeft,
  xCenter,
  getPaddingRight,
} from "gui/Components/ScreenElements/Table/TableRendering/cells/dataCellRenderer";

export function dataColumnsWidths() {
  return tableColumnIds().map((id) => columnWidths().get(id) || 100);
}

export function dataColumnsDraws() {
  return tableColumnIds().map((id) => () => {
    applyScrollTranslation();
    drawDataCellBackground();
    clipCell();
    drawCellValue();
    registerClickHandler(id);
    registerToolTipGetter(id);
  });
}

function registerToolTipGetter(columnId: string) {
  const ctx2d = context2d();
  const property = currentProperty();
  const cellRenderer = currentDataCellRenderer(ctx2d);
  const cellText = cellRenderer.cellTextMulitiline;
  const cellClickableArea = getCellClickableArea();
  const currentRowIndex = rowIndex();
  const currentColumnIndex = drawingColumnIndex();
  const cellWidth = currentColumnWidth();
  const cellHeight = currentRowHeight();

  if (property.column === "CheckBox" || property.column === "Image" || property.column === "Blob") {
    return;
  }

  if (
    getShowToolTipsForMemoFieldsOnly(property) &&
    (property.column !== "Text" || !property.multiline)
  ) {
    return;
  }

  const toolTipPositionRectangle = {
    columnLeft: currentColumnLeft() + currentColumnWidth() * 0.2,
    columnWidth: 0,
    rowTop: currentRowTop() + currentRowHeight() + 10,
    rowHeight: 0,
  };

  const textWidth = ctx2d.measureText(currentCellText()).width / CPR();
  if (cellWidth - getPaddingLeft() - getPaddingRight() < textWidth) {
    onMouseOver({
      x: cellClickableArea.x,
      y: cellClickableArea.y,
      w: cellClickableArea.width,
      h: cellClickableArea.height,
      toolTipGetter() {
        return {
          columnIndex: currentColumnIndex,
          rowIndex: currentRowIndex,
          content: cellText,
          cellWidth: cellWidth,
          cellHeight: cellHeight,
          positionRectangle: toolTipPositionRectangle,
        };
      },
    });
  }
}

function registerClickHandler(columnId: string) {
  const ctx = context();
  const row = currentDataRow();

  const property = currentProperty();

  const cellClickableArea = getCellClickableArea();
  if (property.column === "CheckBox") {
    const checkboxClickableArea = getCheckboxClickableArea();
    onClick({
      x: checkboxClickableArea.x,
      y: checkboxClickableArea.y,
      w: checkboxClickableArea.width,
      h: checkboxClickableArea.height,
      handler(event: any) {
        flow(function* () {
          if (event.isDouble) {
            getTablePanelView(ctx).setEditing(false);
            const defaultAction = getDataView(ctx).firstEnabledDefaultAction;
            if (defaultAction) {
              yield actionsUi.actions.onActionClick(ctx)(event, defaultAction);
            }
          } else {
            yield* getTablePanelView(ctx).onCellClick(event, row, columnId, true);
            yield onPossibleSelectedRowChange(ctx)(
              getMenuItemId(ctx),
              getDataStructureEntityId(ctx),
              getSelectedRowId(ctx)
            );
          }
        })();
      },
    });
    onClick({
      x: cellClickableArea.x,
      y: cellClickableArea.y,
      w: cellClickableArea.width,
      h: cellClickableArea.height,
      handler(event: any) {
        flow(function* () {
          if (event.isDouble) {
            getTablePanelView(ctx).setEditing(false);
            const defaultAction = getDataView(ctx).firstEnabledDefaultAction;
            if (defaultAction && defaultAction.isEnabled) {
              yield actionsUi.actions.onActionClick(ctx)(event, defaultAction);
            }
          } else {
            yield* getTablePanelView(ctx).onCellClick(event, row, columnId, false);
            yield onPossibleSelectedRowChange(ctx)(
              getMenuItemId(ctx),
              getDataStructureEntityId(ctx),
              getSelectedRowId(ctx)
            );
          }
        })();
      },
    });
  } else {
    onClick({
      x: cellClickableArea.x,
      y: cellClickableArea.y,
      w: cellClickableArea.width,
      h: cellClickableArea.height,
      handler(event: any) {
        flow(function* () {
          if (event.isDouble) {
            getTablePanelView(ctx).setEditing(false);
            const defaultAction = getDataView(ctx).firstEnabledDefaultAction;
            if (defaultAction && defaultAction.isEnabled) {
              yield actionsUi.actions.onActionClick(ctx)(event, defaultAction);
            }
          } else {
            yield* getTablePanelView(ctx).onCellClick(event, row, columnId, false);
            yield onPossibleSelectedRowChange(ctx)(
              getMenuItemId(ctx),
              getDataStructureEntityId(ctx),
              getSelectedRowId(ctx)
            );
          }
        })();
      },
    });
  }
}

function getCellClickableArea() {
  return {
    x: currentColumnLeftVisible(),
    y: currentRowTop(),
    width: currentColumnWidthVisible(),
    height: currentRowHeight(),
  };
}

function getCheckboxClickableArea() {
  const fontSize = checkBoxCharacterFontSize;
  return {
    x: xCenter() - fontSize / 2,
    y: currentRowTop() + topTextOffset - 4 - fontSize / 2,
    width: fontSize,
    height: fontSize,
  };
}

export function drawDataCellBackground() {
  const ctx2d = context2d();
  const ctx = context();
  const selectedRowId = getSelectedRowId(tablePanelView());
  const isRowCursor = recordId() === selectedRowId;

  const thisCellRectangle = {
    columnLeft: currentColumnLeft(),
    columnWidth: currentColumnWidth(),
    rowTop: currentRowTop(),
    rowHeight: currentRowHeight(),
  };
  getTablePanelView(ctx).setCellRectangle(rowIndex(), drawingColumnIndex(), thisCellRectangle);
  if (drawingColumnIndex() === 0) {
    getTablePanelView(ctx).firstColumn = currentProperty();
  }

  /*ctx2d.fillStyle = getUnderLineColor();
  ctx2d.fillRect(
    currentColumnLeft() * CPR(),
    currentRowTop() * CPR(),
    currentColumnWidth() * CPR(),
    rowHeight() * CPR()
  );*/

  ctx2d.fillStyle = getBackGroundColor();
  ctx2d.fillRect(
    CPR() * currentColumnLeft(),
    CPR() * currentRowTop(),
    CPR() * currentColumnWidth(),
    CPR() * (currentRowHeight() - 1)
  );
  if (isRowCursor) {
    drawSelectedRowBorder(8);
  }
}

function drawCellValue() {
  const ctx2d = context2d();
  const property = currentProperty();
  const isHidden = !getRowStateAllowRead(tablePanelView(), recordId(), property.id);
  const foregroundColor = getRowStateForegroundColor(tablePanelView(), recordId());
  const type = property.column;

  let isLink = false;
  let isLoading = false;
  let isInvalid = !!currentCellErrorMessage();

  if (property.isLookup && property.lookupEngine) {
    isLoading = isCurrentCellLoading();
    isLink = selectors.column.isLinkToForm(property);
  }

  ctx2d.font = `${fontSize * CPR()}px "IBM Plex Sans", sans-serif`;
  if (isHidden) {
    return;
  }
  if (isLoading) {
    ctx2d.fillStyle = "#888888";
    ctx2d.fillText(
      "Loading...",
      CPR() * (currentColumnLeft() + getPaddingLeft()),
      CPR() * (currentRowTop() + topTextOffset)
    );
  } else {
    if (isInvalid) {
      ctx2d.save();
      ctx2d.fillStyle = "red";
      // Exclamation mark (not working, probably solvable)
      //ctx2d.font = `${checkBoxCharacterFontSize * CPR()}px "Font Awesome 5 Free"`;
      //This character does not work for some reason 😠
      //ctx2d.fillText(`\uf06a`, CPR() * currentColumnLeft(), currentRowTop() + topTextOffset);

      // Or we might put a line as in Flash client:
      /*ctx2d.fillRect(
        currentColumnLeft() * CPR(),
        currentRowTop() * CPR(),
        3 * CPR(),
        currentRowHeight() * CPR()
      );*/

      ctx2d.beginPath();
      ctx2d.moveTo(currentColumnLeft() * CPR(), currentRowTop() * CPR());
      ctx2d.lineTo(
        (currentColumnLeft() + 5) * CPR(),
        (currentRowTop() + 0.5 * currentRowHeight()) * CPR()
      );
      ctx2d.lineTo(currentColumnLeft() * CPR(), (currentRowTop() + currentRowHeight()) * CPR());
      ctx2d.fill();
      ctx2d.restore();
    }

    ctx2d.fillStyle = foregroundColor || "black";
    currentDataCellRenderer(ctx2d).drawCellText();
  }
}

function getUnderLineColor() {
  return "#e5e5e5";
}

function getBackGroundColor() {
  const isColumnOrderChangeSource =
    tablePanelView().columnOrderChangingSourceId === currentProperty().id;
  const selectedColumnId = tableColumnIds()[drawingColumnIndex()];
  const selectedRowId = getSelectedRowId(tablePanelView());

  const isCellCursor = currentProperty().id === selectedColumnId && recordId() === selectedRowId;
  const isRowCursor = recordId() === selectedRowId;

  const backgroundColor = getRowStateRowBgColor(tablePanelView(), recordId());

  if (isColumnOrderChangeSource) {
    return "#eeeeff";
  } else if (isCellCursor) {
    return "#EDF2FF";
  } else if (isRowCursor) {
    return backgroundColor ? shadeHexColor(backgroundColor, -0.1)! : "#EDF2FF";
  } else {
    if (backgroundColor) {
      return backgroundColor;
    } else {
      return rowIndex() % 2 === 1 ? "#f7f6fa" : "#ffffff";
    }
  }
}
