import * as React from "react";
import { IGridDimensions, IGridCursorPos, IGridCursorProps } from "./types";
import { observer } from "mobx-react";
import {
  action,
  observable,
  computed,
  IReactionDisposer,
  reaction,
  trace
} from "mobx";
import { Rect } from "react-measure";

@observer
export default class GridCursor extends React.Component<IGridCursorProps>
  implements Rect {
  @observable public sTop: number = 0;
  @observable public sLeft: number = 0;
  @observable public sWidth: number = 0;
  @observable public sHeight: number = 0;

  @computed public get top() {
    return this.sTop;
  }

  @computed public get left() {
    return this.sLeft;
  }

  @computed public get width() {
    return this.sWidth;
  }

  @computed public get height() {
    return this.sHeight;
  }

  private elmCellDiv: HTMLDivElement | null;

  @action.bound private updateCellRect() {
    if (this.elmCellDiv) {
      const rect = this.elmCellDiv!.getBoundingClientRect();
      this.sTop = rect.top;
      this.sLeft = rect.left;
      this.sWidth = rect.width;
      this.sHeight = rect.height;
    }
  }

  private hRectPoller: any;
  private updatePollerRunning() {
    if (this.elmCellDiv && this.props.pollCellRect) {
      this.updateCellRect();
      // this.hRectPoller = setInterval(this.updateCellRect, 250);
    } else {
      clearInterval(this.hRectPoller);
    }
  }

  public componentDidUpdate(prevProps: IGridCursorProps) {
    if (this.props.pollCellRect !== prevProps.pollCellRect) {
      this.updatePollerRunning();
    }
    this.updateCellRect();
  }

  @action.bound private refCellDiv(elm: HTMLDivElement) {
    this.elmCellDiv = elm;
    this.updatePollerRunning();
  }


  public render() {
    const gcp = this.props.gridCursorPos;
    if (
      gcp.selectedColumnIndex === undefined ||
      gcp.selectedRowIndex === undefined
    ) {
      return null;
    }

    const gDim = this.props.gridDimensions;
    const height = gDim.getRowHeight(gcp.selectedRowIndex);
    const top = gDim.getRowTop(gcp.selectedRowIndex);
    if (this.props.showCellCursor) {
      const width = gDim.getColumnWidth(gcp.selectedColumnIndex);
      const left = gDim.getColumnLeft(gcp.selectedColumnIndex);
      return (
        <>
          <div
            style={{
              position: "absolute",
              width: Math.max(0, left),
              height,
              left: 0,
              top,
              backgroundColor: "#00000022"
            }}
          />
          <div
            ref={this.refCellDiv}
            style={{
              position: "absolute",
              width,
              height,
              left,
              top,
              backgroundColor: "#00000055"
            }}
          >
            {this.props.renderCellContent(this)}
          </div>
          <div
            style={{
              position: "absolute",
              width: gDim.contentWidth - left + width,
              height,
              left: left + width,
              top,
              backgroundColor: "#00000022"
            }}
          />
        </>
      );
    } else {
      return (
        <div
          style={{
            position: "absolute",
            width: gDim.contentWidth,
            height,
            top,
            left: 0,
            backgroundColor: "#00000022"
          }}
        />
      );
    }
  }
}
