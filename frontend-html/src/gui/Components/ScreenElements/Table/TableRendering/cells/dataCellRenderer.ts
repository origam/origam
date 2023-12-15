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
  currentCellText,
  currentCellValue,
  currentColumnId,
  currentColumnLeft,
  currentColumnWidth,
  currentProperty,
  currentRowHeight,
  currentRowTop,
} from "gui/Components/ScreenElements/Table/TableRendering/currentCell";
import {
  cellPaddingLeft,
  cellPaddingLeftFirstCell,
  cellPaddingRight,
  checkBoxCharacterFontSize,
  numberCellPaddingRight,
  topTextOffset,
} from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import { CPR } from "utils/canvas";
import moment from "moment";
import selectors from "model/selectors-tree";
import {
  context,
  drawingColumnIndex,
  formScreen,
  rowHeight,
  rowIndex,
} from "gui/Components/ScreenElements/Table/TableRendering/renderingValues";
import { setTableDebugValue } from "gui/Components/ScreenElements/Table/TableRendering/DebugTableMonitor";
import { CellAlignment } from "gui/Components/ScreenElements/Table/TableRendering/cells/cellAlignment";
import { flashColor2htmlColor } from "utils/flashColorFormat";

interface IDataCellRenderer {
  drawCellText(): void;
  cellText: string | undefined;
}

export function currentDataCellRenderer(ctx2d: CanvasRenderingContext2D) {
  let property = currentProperty();
  const type = property.column;
  const cellAlignment = new CellAlignment( drawingColumnIndex() === 0, type, property.style);
  switch (type) {
    case "CheckBox":
      return new CheckBoxCellRenderer(ctx2d);
    case "Date":
      return new DateCellRenderer(ctx2d);
    case "TagInput":
      return new TagInputCellRenderer(ctx2d);
    case "ComboBox":
    case "Checklist":
      return new CheckListCellRenderer(ctx2d);
    case "Number":
      return new GenericCellRenderer(ctx2d, property.style, cellAlignment);
    case "Image":
      return new ImageCellRenderer(ctx2d);
    case "Color":
      return new ColorCellRenderer(ctx2d);
    default:
      return new GenericCellRenderer(ctx2d, property.style, cellAlignment);
  }
}

class ColorCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {
  }

  cellText = "";

  drawCellText(): void {
    this.ctx2d.fillStyle = flashColor2htmlColor(currentCellValue()) || "black";
    this.ctx2d.fillRect(
      (currentColumnLeft() + 3) * CPR(),
      (currentRowTop() + 3) * CPR(),
      (currentColumnWidth() - 2 * 3) * CPR(),
      (currentRowHeight() - 2 * 3) * CPR()
    );
  }
}

class CheckBoxCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {
  }

  get cellText() {
    return currentCellText();
  }

  drawCellText(): void {
    this.ctx2d.font = `${checkBoxCharacterFontSize * CPR()}px "Font Awesome 5 Free"`;
    this.ctx2d.textAlign = "center";
    this.ctx2d.textBaseline = "middle";

    this.ctx2d.fillText(
      !!this.cellText ? "\uf14a" : "\uf0c8",
      CPR() * xCenter(),
      CPR() * (currentRowTop() + topTextOffset - 5)
    );
    setTableDebugValue(context(), currentColumnId(), rowIndex(), this.cellText);
  }
}

class DateCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {
  }

  get cellText() {
    if (currentCellText() !== null && currentCellText() !== "") {
      let momentValue = moment(currentCellText());
      if (!momentValue.isValid()) {
        return undefined;
      }
      return momentValue.format(currentProperty().formatterPattern);
    } else {
      return undefined;
    }
  }

  drawCellText(): void {
    const dateTimeText = this.cellText;
    if (dateTimeText) {
      this.ctx2d.fillText(
        dateTimeText,
        CPR() * (currentColumnLeft() + getPaddingLeft()),
        CPR() * (currentRowTop() + topTextOffset)
      );
    }
    setTableDebugValue(context(), currentColumnId(), rowIndex(), dateTimeText);
  }
}

class TagInputCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {
  }

  get cellText() {
    return currentCellText();
  }

  drawCellText(): void {
    if (this.cellText !== null) {
      this.ctx2d.fillText(
        "" + this.cellText!,
        CPR() * (currentColumnLeft() + getPaddingLeft()),
        CPR() * (currentRowTop() + topTextOffset)
      );
    }
    setTableDebugValue(context(), currentColumnId(), rowIndex(), this.cellText);
  }
}

class CheckListCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {
  }

  get cellText() {
    return currentCellText();
  }

  drawCellText(): void {
    let isLink = false;
    const property = currentProperty();
    if (property.isLookup && property.lookupEngine) {
      isLink = selectors.column.isLinkToForm(currentProperty());
    }

    if (isLink) {
      this.ctx2d.save();
      this.ctx2d.fillStyle = getComputedStyle(document.documentElement).getPropertyValue('--foreground1');
    }
    if (currentCellText() !== null) {
      this.ctx2d.fillText(
        "" + currentCellText()!,
        CPR() * (currentColumnLeft() + getPaddingLeft()),
        CPR() * (currentRowTop() + topTextOffset)
      );
    }
    setTableDebugValue(context(), currentColumnId(), rowIndex(), currentCellText());
    if (isLink) {
      this.ctx2d.restore();
    }
  }
}

class ImageCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {
  }

  get cellText() {
    return currentCellText();
  }

  drawCellText(): void {
    const value = currentCellValue();
    if (!value) {
      return;
    }
    const {pictureCache} = formScreen();
    const img = pictureCache.getImage(value);

    if (!img || !img.complete) {
      return;
    }
    const property = currentProperty();
    const iw = img.width;
    const ih = img.height;
    const cw = property.columnWidth;
    const ch = property.height;
    const WR = cw / iw;
    const HR = ch / ih;
    let ratio = 0;
    if (Math.abs(1 - WR) < Math.abs(1 - HR)) {
      ratio = WR;
    } else {
      ratio = HR;
    }
    const finIW = ratio * iw;
    const finIH = ratio * ih;
    this.ctx2d.drawImage(
      img,
      CPR() * (xCenter() - finIW * 0.5),
      CPR() * (yCenter() - finIH * 0.5),
      CPR() * finIW,
      CPR() * finIH
    );
  }
}

class GenericCellRenderer implements IDataCellRenderer {
  constructor(
    private ctx2d: CanvasRenderingContext2D,
    private style: ({[key: string]: string}) | undefined,
    private cellAlignment: CellAlignment) {
  }

  get cellText() {
    return currentCellText();
  }

  drawCellText(): void {
    if (currentCellText() !== null) {
      if (!currentProperty().isPassword) {
        this.ctx2d.font = getCustomTextStyle(this.style, this.ctx2d);
        let x = getCellXCoordinate(this.cellAlignment);
        this.ctx2d.textAlign = this.cellAlignment.alignment as CanvasTextAlign;
        this.ctx2d.fillText(
          "" + currentCellText()!,
          CPR() * x,
          CPR() * (currentRowTop() + topTextOffset)
        );
      } else {
        this.ctx2d.fillText("*******", numberCellPaddingRight * CPR(), 15 * CPR());
      }
    }
    setTableDebugValue(context(), currentColumnId(), rowIndex(), currentCellText())
  }
}

export function getPaddingLeft(isFirstColumn?: boolean) {
  if(isFirstColumn === undefined){
    isFirstColumn = drawingColumnIndex() === 0
  }
  return isFirstColumn ? cellPaddingLeftFirstCell : cellPaddingLeft;
}

export function getPaddingRight() {
  return cellPaddingRight;
}

export function xCenter() {
  return currentColumnLeft() + currentColumnWidth() / 2;
}

export function yCenter() {
  return currentRowTop() + rowHeight() / 2;
}

function getCustomTextStyle(style: ({[key: string]: string}) | undefined, ctx2d: CanvasRenderingContext2D){
  if(style?.["fontWeight"] === "bold"){
   return "bold " + ctx2d.font
  }
  return ctx2d.font;
}

function getCellXCoordinate(cellAlignment: CellAlignment){

  if(cellAlignment.alignment === "center") {
    return xCenter() ;
  }
  else if(cellAlignment.alignment === "right"){
    return currentColumnLeft() + currentColumnWidth() - cellAlignment.paddingRight;
  }
  else {
    return currentColumnLeft()  + cellAlignment.paddingLeft;
  }
}