import { tableColumnIds, columnWidths, context2d, columnIndex, rowHeight, rowIndex, tablePanelView, recordId, property, context } from "../renderingValues";
import {
  currentColumnLeft,
  currentRowTop,
  currentCellText,
  currentColumnWidth,
  currentRowHeight,
} from "../currentCell";
import { applyScrollTranslation, clipCell } from "./cellsCommon";
import { CPR } from "utils/canvas";
import { getSelectedRowId } from "model/selectors/TablePanelView/getSelectedRowId";
import { getRowStateColumnBgColor } from "model/selectors/RowState/getRowStateColumnBgColor";
import { getRowStateRowBgColor } from "model/selectors/RowState/getRowStateRowBgColor";
import { getRowStateAllowRead } from "model/selectors/RowState/getRowStateAllowRead";
import { getRowStateForegroundColor } from "model/selectors/RowState/getRowStateForegroundColor";
import selectors from "model/selectors-tree";
import moment from "moment";
import { onClick } from "../onClick";
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { onPossibleSelectedRowChange } from "model/actions-ui/onPossibleSelectedRowChange";
import { getMenuItemId } from "model/selectors/getMenuItemId";
import { getDataStructureEntityId } from "model/selectors/DataView/getDataStructureEntityId";
import { flow } from "mobx";
import { getIsSelectionCheckboxesShown } from "model/selectors/DataView/getIsSelectionCheckboxesShown";

export function dataColumnsWidths() {
  return tableColumnIds().map((id) => columnWidths().get(id) || 100);
}

export function dataColumnsDraws() {
  return tableColumnIds().map((id) => () => {
    applyScrollTranslation();
    clipCell();
    drawDataCellBackground();
    drawCellValue();
    registerClickHandler();
  });
}

function registerClickHandler(){
  const ctx = context();
  const cellRowIndex = rowIndex();
  const selectionColumnShown = getIsSelectionCheckboxesShown(ctx);
  const cellColumnIndex = selectionColumnShown
    ? columnIndex() - 1
    : columnIndex() ;

  onClick({
    x: currentColumnLeft(),
    y: currentRowTop(),
    w: currentColumnWidth(),
    h: currentRowHeight(),
    handler(event: any) { flow(function* (){
      console.log("click");

      yield* getTablePanelView(ctx).onCellClick(event, cellRowIndex, cellColumnIndex);
      yield onPossibleSelectedRowChange(ctx)(
        getMenuItemId(ctx),
        getDataStructureEntityId(ctx),
        getSelectedRowId(ctx));
    })();
    },
  });
}

export function drawDataCellBackground() {

  const ctx2d = context2d();

  ctx2d.fillStyle = getUnderLineColor();
  ctx2d.fillRect(0, 0, currentColumnWidth() * CPR, rowHeight() * CPR);

  ctx2d.fillStyle = getBackGroundColor();
  ctx2d.fillRect(
    CPR * currentColumnLeft(),
    CPR * currentRowTop(),
    CPR * currentColumnWidth(),
    CPR * currentRowHeight()
  );
}

function drawCellValue(){
  const ctx2d = context2d();
  const isHidden = !getRowStateAllowRead(tablePanelView(), recordId(), property().id)
  const foregroundColor = getRowStateForegroundColor(tablePanelView(), recordId(), "")
  const type = property().column;
  const cellPaddingLeft = columnIndex() === 0 ? 25 : 15;

  let isLink = false;
  let isLoading = false;
  if (property().isLookup) {
    isLoading = property().lookup!.isLoading(currentCellText());
    isLink = selectors.column.isLinkToForm(property());
  }

  ctx2d.font = `${12 * CPR}px "IBM Plex Sans", sans-serif`;
  if (isHidden) {
    return;
  }
  if (isLoading) {
    ctx2d.fillStyle = "#888888";
    ctx2d.fillText("Loading...", cellPaddingLeft * CPR, 15 * CPR);
  } else {
    ctx2d.fillStyle = foregroundColor || "black";
    switch (type) {
      case "CheckBox":
        ctx2d.font = `${14 * CPR}px "Font Awesome 5 Free"`;
        ctx2d.textAlign = "center";
        ctx2d.textBaseline = "middle";

        ctx2d.fillText(
          !!currentCellText() ? "\uf14a" : "\uf0c8",
          CPR * (currentColumnLeft() + (currentColumnWidth() / 2)),
          CPR * (currentRowTop() + (rowHeight() / 2)));
        break;
      case "Date":
        if (currentCellText() !== null) {
          ctx2d.fillText(
            moment(currentCellText()).format(property().formatterPattern),
            CPR * (currentColumnLeft() + 2),
            CPR * (currentRowTop() + 17));
        }
        break;
      case "ComboBox":
      case "TagInput":
        if (isLink) {
          ctx2d.save();
          ctx2d.fillStyle = "blue";
        }
        if (currentCellText() !== null) {
          ctx2d.fillText("" + currentCellText()!,                   
            CPR * (currentColumnLeft() + 2),
            CPR * (currentRowTop() + 17));
        }
        if (isLink) {
          ctx2d.restore();
        }
        break;
      case "Number":
        if (currentCellText() !== null) {
          ctx2d.save();
          ctx2d.textAlign = "right";
          ctx2d.fillText("" + currentCellText()!,                
            CPR * (currentColumnLeft() + currentColumnWidth() - cellPaddingLeft),
            CPR * (currentRowTop() + 17));
          ctx2d.restore();
        }
        break;
      default:
        if (currentCellText() !== null) {
          if (!property().isPassword) {
            ctx2d.fillText(
              "" + currentCellText()!,
              CPR * (currentColumnLeft() + 2),
              CPR * (currentRowTop() + 17));
          } else {
            ctx2d.fillText("*******", cellPaddingLeft * CPR, 15 * CPR);
          }
        }
    }
  }
}

function getUnderLineColor() { return "#e5e5e5"; }

function getBackGroundColor() {

  const isColumnOrderChangeSource = tablePanelView().columnOrderChangingSourceId === property().id;
  const selectedColumnId = tableColumnIds()[columnIndex()];
  const selectedRowId = getSelectedRowId(tablePanelView());

  const isCellCursor = property().id === selectedColumnId && recordId() === selectedRowId;
  const isRowCursor = recordId() === selectedRowId;

  const backgroundColor =
    getRowStateColumnBgColor(tablePanelView(), recordId(), "") ||
    getRowStateRowBgColor(tablePanelView(), recordId());

  if (isColumnOrderChangeSource) {
    return "#eeeeff";
    //} else if(cell.isColumnOrderChangeTarget){
  } else if (isCellCursor) {
    return "#bbbbbb";
  } else if (isRowCursor) {
    return "#dddddd";
  } else {
    if (backgroundColor) {
      return backgroundColor;
    } else {
      return rowIndex() % 2 === 0 ? "#f7f6fa" : "#ffffff";
    }
  }
}
