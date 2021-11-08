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
  cellPaddingLeft,
  cellPaddingRight,
  numberCellPaddingRight
} from "gui/Components/ScreenElements/Table/TableRendering/cells/cellsCommon";
import { getPaddingLeft } from "gui/Components/ScreenElements/Table/TableRendering/cells/dataCellRenderer";

export class CellAlignment {

  paddingRight: number;
  paddingLeft: number;
  alignment: string;

  constructor(isFirstColumn: boolean, type: string, style: ({ [key: string]: string }) | undefined) {

    if (style?.["textAlign"]) {
      if (style["textAlign"] === "center") {
        this.paddingRight = 0;
        this.paddingLeft = 0;
        this.alignment = "center";
      } else if (style["textAlign"] === "left") {
        this.paddingRight = cellPaddingRight;
        this.paddingLeft = getPaddingLeft(isFirstColumn);
        this.alignment = "left";
      } else if (style["textAlign"] === "right") {
        this.paddingRight = numberCellPaddingRight;
        this.paddingLeft = cellPaddingLeft;
        this.alignment = "right";
      } else {
        throw new Error(`textAlign ${style["textAlign"]} not implemented`);
      }
      return;
    }

    switch (type) {
      case "Number":
        this.paddingRight = numberCellPaddingRight;
        this.paddingLeft = cellPaddingLeft;
        this.alignment = "right";
        break;
      default:
        this.paddingRight = cellPaddingRight;
        this.paddingLeft = getPaddingLeft(isFirstColumn);
        this.alignment = "left";
    }
  }
}