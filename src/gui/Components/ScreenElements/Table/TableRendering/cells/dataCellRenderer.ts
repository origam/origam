import {
  currentCellText,
  currentCellTextMultiline,
  currentCellValue,
  currentColumnLeft,
  currentColumnWidth,
  currentProperty,
  currentRowHeight,
  currentRowTop,
} from "gui/Components/ScreenElements/Table/TableRendering/currentCell";
import {
  cellPaddingLeft,
  cellPaddingLeftFirstCell,
  checkBoxCharacterFontSize,
  numberCellPaddingRight,
  topTextOffset,
} from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import { CPR } from "utils/canvas";
import moment from "moment";
import selectors from "model/selectors-tree";
import {
  currentRow,
  drawingColumnIndex,
  formScreen,
  rowHeight,
} from "gui/Components/ScreenElements/Table/TableRendering/renderingValues";
import { flashColor2htmlColor } from "utils/flashColorFormat";

interface IDataCellRenderer {
  drawCellText(): void;
  cellText: string | undefined;
  cellTextMulitiline: string | undefined;
  paddingLeft: number;
}

export function currentDataCellRenderer(ctx2d: CanvasRenderingContext2D) {
  const type = currentProperty().column;
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
      return new NumberInputCellRenderer(ctx2d);
    case "Image":
      return new ImageCellRenderer(ctx2d);
    case "Color":
      return new ColorCellRenderer(ctx2d);
    default:
      return new GenericCellRenderer(ctx2d);
  }
}

class ColorCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {}

  cellText = "";
  cellTextMulitiline = "";
  paddingLeft = 0;

  drawCellText(): void {
    this.ctx2d.font = `${checkBoxCharacterFontSize * CPR()}px "Font Awesome 5 Free"`;
    this.ctx2d.textAlign = "center";
    this.ctx2d.textBaseline = "middle";

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
  constructor(private ctx2d: CanvasRenderingContext2D) {}

  get paddingLeft() {
    return getPaddingLeft();
  }

  get cellText() {
    return currentCellText();
  }

  get cellTextMulitiline() {
    return currentCellTextMultiline();
  }

  drawCellText(): void {
    this.ctx2d.font = `${checkBoxCharacterFontSize * CPR()}px "Font Awesome 5 Free"`;
    this.ctx2d.textAlign = "center";
    this.ctx2d.textBaseline = "middle";

    this.ctx2d.fillText(
      !!this.cellText ? "\uf14a" : "\uf0c8",
      CPR() * xCenter(),
      CPR() * (currentRowTop() + topTextOffset - 4)
    );
  }
}

class DateCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {}

  get paddingLeft() {
    return getPaddingLeft();
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

  get cellTextMulitiline() {
    return this.cellText;
  }

  drawCellText(): void {
    const dateTimeText = this.cellText;
    if (dateTimeText) {
      this.ctx2d.fillText(
        dateTimeText,
        CPR() * (currentColumnLeft() + this.paddingLeft),
        CPR() * (currentRowTop() + topTextOffset)
      );
    }
  }
}
class TagInputCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {}

  get paddingLeft() {
    return getPaddingLeft();
  }

  get cellText() {
    return currentCellText();
  }

  get cellTextMulitiline() {
    return currentCellTextMultiline();
  }

  drawCellText(): void {
    if (this.cellText !== null) {
      this.ctx2d.fillText(
        "" + this.cellText!,
        CPR() * (currentColumnLeft() + this.paddingLeft),
        CPR() * (currentRowTop() + topTextOffset)
      );
    }
  }
}

class CheckListCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {}

  get paddingLeft() {
    return getPaddingLeft();
  }

  get cellText() {
    return currentCellText();
  }

  get cellTextMulitiline() {
    return currentCellTextMultiline();
  }

  drawCellText(): void {
    let isLink = false;
    const property = currentProperty();
    if (property.isLookup && property.lookupEngine) {
      isLink = selectors.column.isLinkToForm(currentProperty());
    }

    if (isLink) {
      this.ctx2d.save();
      this.ctx2d.fillStyle = "#4c84ff";
    }
    if (currentCellText() !== null) {
      this.ctx2d.fillText(
        "" + currentCellText()!,
        CPR() * (currentColumnLeft() + this.paddingLeft),
        CPR() * (currentRowTop() + topTextOffset)
      );
    }
    if (isLink) {
      this.ctx2d.restore();
    }
  }
}

class NumberInputCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {}

  get paddingLeft() {
    return currentColumnWidth() - this.paddingRight;
  }

  get cellText() {
    return currentCellText();
  }

  get cellTextMulitiline() {
    return currentCellTextMultiline();
  }

  get paddingRight() {
    return currentProperty().column === "Number" ? numberCellPaddingRight() : 0;
  }

  drawCellText(): void {
    if (currentCellText() !== null) {
      this.ctx2d.save();
      this.ctx2d.textAlign = "right";
      this.ctx2d.fillText(
        "" + currentCellText()!,
        CPR() * (currentColumnLeft() + this.paddingLeft),
        CPR() * (currentRowTop() + topTextOffset)
      );
      this.ctx2d.restore();
    }
  }
}

class ImageCellRenderer implements IDataCellRenderer {
  constructor(private ctx2d: CanvasRenderingContext2D) {}

  get paddingLeft() {
    return getPaddingLeft();
  }

  get cellText() {
    return currentCellText();
  }

  get cellTextMulitiline() {
    return currentCellTextMultiline();
  }

  drawCellText(): void {
    const value = currentCellValue();
    if (!value) {
      return;
    }
    const { pictureCache } = formScreen();
    const img = pictureCache.getImage(value);

    if (!img || !img.complete) {
      return;
    }
    const property = currentProperty();
    const iw = img.width;
    const ih = img.height;
    const cw = property.width;
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
  constructor(private ctx2d: CanvasRenderingContext2D) {}

  get paddingLeft() {
    return getPaddingLeft();
  }

  get cellText() {
    return currentCellText();
  }

  get cellTextMulitiline() {
    return currentCellTextMultiline();
  }

  drawCellText(): void {
    if (currentCellText() !== null) {
      if (!currentProperty().isPassword) {
        this.ctx2d.fillText(
          "" + currentCellText()!,
          CPR() * (currentColumnLeft() + this.paddingLeft),
          CPR() * (currentRowTop() + topTextOffset)
        );
      } else {
        this.ctx2d.fillText("*******", numberCellPaddingRight() * CPR(), 15 * CPR());
      }
    }
  }
}

export function getPaddingLeft() {
  return drawingColumnIndex() === 0 ? cellPaddingLeftFirstCell : cellPaddingLeft;
}

export function xCenter() {
  return currentColumnLeft() + currentColumnWidth() / 2;
}

export function yCenter() {
  return currentRowTop() + rowHeight() / 2;
}
