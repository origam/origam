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

import React from "react";
import S from "./Canvas.module.css";
import { action, computed } from "mobx";
import { CPR } from "utils/canvas";

export class Canvas extends React.Component<{ width: number; height: number, refCanvasElement: any }> {
  firstVisibleRowIndex = 0;
  lastVisibleRowIndex = 0;

  ctxCanvas: CanvasRenderingContext2D | null = null;

  @action.bound
  refCanvas(elm: HTMLCanvasElement | null) {
    this.props.refCanvasElement(elm);
    if (elm) {
      this.ctxCanvas = elm.getContext("2d");
    } else {
      this.ctxCanvas = null;
    }
  }

  @computed
  public get canvasWidthPX() {
    return Math.ceil(this.props.width * CPR()) || 0;
  }

  @computed
  public get canvasHeightPX() {
    return Math.ceil(this.props.height * CPR()) || 0;
  }

  @computed
  public get canvasWidthCSS() {
    return Math.ceil(this.props.width * CPR()) / CPR() || 0;
  }

  @computed
  public get canvasHeightCSS() {
    return Math.ceil(this.props.height * CPR()) / CPR() || 0;
  }

  @computed
  public get canvasProps() {
    return {
      width: this.canvasWidthPX,
      height: this.canvasHeightPX,
      style: {
        // +1 because added 1px border makes canvas resizing and blurry.
        // Has to by synchronized with stylesheet :/
        minWidth: this.canvasWidthCSS + 1,
        maxWidth: this.canvasWidthCSS + 1,
        minHeight: this.canvasHeightCSS,
        maxHeight: this.canvasHeightCSS,
      },
    };
  }

  render() {
    return (
      <canvas
        className={S.canvas}
        {...this.canvasProps}
        /*width={0}
        height={0}*/
        ref={this.refCanvas}
      />
    );
  }
}
