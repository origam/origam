import React from "react";
import S from "./Canvas.module.css";
import { computed, action } from "mobx";
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
