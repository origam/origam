import { flow } from "mobx";
import { onPossibleSelectedRowChange } from "model/actions-ui/onPossibleSelectedRowChange";
import selectors from "model/selectors-tree";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getRowStateAllowRead } from "model/selectors/RowState/getRowStateAllowRead";
import { getRowStateColumnBgColor } from "model/selectors/RowState/getRowStateColumnBgColor";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import moment from "moment";
import { CPR } from "utils/canvas";
import actionsUi from "../../../../../../model/actions-ui-tree";
import { getDataView } from "../../../../../../model/selectors/DataView/getDataView";
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
import { onClick } from "../onClick";
import {
  columnWidths,
  context,
  context2d,
  currentDataRow,
  drawingColumnIndex,
  recordId,
  rowHeight,
  rowIndex,
  tableColumnIds,
  tablePanelView,
} from "../renderingValues";
import {
  applyScrollTranslation,
  cellPaddingLeft,
  cellPaddingLeftFirstCell,
  checkBoxCharacterFontSize,
  clipCell,
  fontSize,
  numberCellPaddingLeft,
  topTextOffset,
} from "./cellsCommon";

export function dataColumnsWidths() {
  return tableColumnIds().map((id) => columnWidths().get(id) || 100);
}

export function dataColumnsDraws() {
  return tableColumnIds().map((id) => () => {
    applyScrollTranslation();
    clipCell();
    drawDataCellBackground();
    drawCellValue();
    registerClickHandler(id);
  });
}

function registerClickHandler(columnId: string) {
  const ctx = context();
  const row = currentDataRow();

  const thisCellRectangle = {
    columnLeft: currentColumnLeft(),
    columnWidth: currentColumnWidth(),
    rowTop: currentRowTop(),
    rowHeight: currentRowHeight(),
  };
  getTablePanelView(ctx).setCellRectangle(rowIndex(), drawingColumnIndex(), thisCellRectangle);

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
            const defaultAction = getDataView(ctx).defaultAction;
            if (defaultAction && defaultAction.isEnabled) {
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
            const defaultAction = getDataView(ctx).defaultAction;
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
            const defaultAction = getDataView(ctx).defaultAction;
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

function xCenter() {
  return currentColumnLeft() + currentColumnWidth() / 2;
}

function yCenter() {
  return currentRowTop() + rowHeight() / 2;
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
    y: yCenter() - fontSize / 2,
    width: fontSize,
    height: fontSize,
  };
}

export function drawDataCellBackground() {
  const ctx2d = context2d();

  ctx2d.fillStyle = getUnderLineColor();
  ctx2d.fillRect(0, 0, currentColumnWidth() * CPR(), rowHeight() * CPR());

  ctx2d.fillStyle = getBackGroundColor();
  ctx2d.fillRect(
    CPR() * currentColumnLeft(),
    CPR() * currentRowTop(),
    CPR() * currentColumnWidth(),
    CPR() * currentRowHeight()
  );
}

function drawCellValue() {
  const ctx2d = context2d();
  const isHidden = !getRowStateAllowRead(tablePanelView(), recordId(), currentProperty().id);
  const foregroundColor = getRowStateForegroundColor(tablePanelView(), recordId(), "");
  const type = currentProperty().column;

  let isLink = false;
  let isLoading = false;
  let isInvalid = !!currentCellErrorMessage();

  const property = currentProperty();
  if (property.isLookup && property.lookupEngine) {
    isLoading = isCurrentCellLoading();
    isLink = selectors.column.isLinkToForm(currentProperty());
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
    switch (type) {
      case "CheckBox":
        ctx2d.font = `${checkBoxCharacterFontSize * CPR()}px "Font Awesome 5 Free"`;
        ctx2d.textAlign = "center";
        ctx2d.textBaseline = "middle";

        ctx2d.fillText(
          !!currentCellText() ? "\uf14a" : "\uf0c8",
          CPR() * xCenter(),
          CPR() * yCenter()
        );
        break;
      case "Date":
        if (currentCellText() !== null && currentCellText() !== "") {
          let momentValue = moment(currentCellText());
          if (!momentValue.isValid()) {
            break;
          }
          ctx2d.fillText(
            momentValue.format(currentProperty().formatterPattern),
            CPR() * (currentColumnLeft() + getPaddingLeft()),
            CPR() * (currentRowTop() + topTextOffset)
          );
        }
        break;
      case "ComboBox":
      case "TagInput":
      case "Checklist":
        if (isLink) {
          ctx2d.save();
          ctx2d.fillStyle = "#4c84ff";
        }
        if (currentCellText() !== null) {
          ctx2d.fillText(
            "" + currentCellText()!,
            CPR() * (currentColumnLeft() + getPaddingLeft()),
            CPR() * (currentRowTop() + topTextOffset)
          );
        }
        if (isLink) {
          ctx2d.restore();
        }
        break;
      case "Number":
        if (currentCellText() !== null) {
          ctx2d.save();
          ctx2d.textAlign = "right";
          ctx2d.fillText(
            "" + currentCellText()!,
            CPR() * (currentColumnLeft() + currentColumnWidth() - numberCellPaddingLeft()),
            CPR() * (currentRowTop() + topTextOffset)
          );
          ctx2d.restore();
        }
        break;
      default:
        if (currentCellText() !== null) {
          if (!currentProperty().isPassword) {
            ctx2d.fillText(
              "" + currentCellText()!,
              CPR() * (currentColumnLeft() + getPaddingLeft()),
              CPR() * (currentRowTop() + topTextOffset)
            );
          } else {
            ctx2d.fillText("*******", numberCellPaddingLeft() * CPR(), 15 * CPR());
          }
        }
    }
  }
}

function getPaddingLeft() {
  return drawingColumnIndex() === 0 ? cellPaddingLeftFirstCell : cellPaddingLeft;
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

  const backgroundColor =
    getRowStateColumnBgColor(tablePanelView(), recordId(), "") ||
    getRowStateRowBgColor(tablePanelView(), recordId());

  if (isColumnOrderChangeSource) {
    return "#eeeeff";
    //} else if(cell.isColumnOrderChangeTarget){
  } else if (isCellCursor) {
    return "#eaf0f9";
  } else if (isRowCursor) {
    return "#ebf3ff";
  } else {
    if (backgroundColor) {
      return backgroundColor;
    } else {
      return rowIndex() % 2 === 1 ? "#f7f6fa" : "#ffffff";
    }
  }
}
