import { bind } from "bind-decorator";
import {
  action,
  computed,
  IReactionDisposer,
  observable,
  reaction,
  runInAction,
  autorun
} from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { IGridCanvasProps } from "./types";
import { CPR } from "src/utils/canvas";
import { rangeQuery } from "src/utils/arrays";

/*
  Canvas element, which just draws table-like data shifted by scroll offset 
  given by scrollOffsetSource.
*/

@observer
export default class GridCanvas extends React.Component<IGridCanvasProps> {
  @observable.ref public refCanvas: HTMLCanvasElement | null = null;
  @observable.ref public ctxCanvas: CanvasRenderingContext2D | null = null;

  // Sometimes the repainting reaction needs to be kicked "artificially",
  // but we do not want to call the painting effect directly, because it would
  // bypss animationFrame scheduling so we just change a value of this field.
  @observable private repaintTrigger = 0;

  private reactionDisposers: IReactionDisposer[] = [];

  @computed public get scrollTop() {
    return this.props.fixedVert ? 0 : this.props.scrollOffsetSource.scrollTop;
  }

  @computed public get scrollLeft() {
    return this.props.fixedHoriz ? 0 : this.props.scrollOffsetSource.scrollLeft;
  }

  @computed public get contentHeight(): number {
    return (
      this.props.gridDimensions.getRowBottom(
        this.props.gridDimensions.rowCount - 1
      ) + this.props.gridDimensions.getRowTop(0)
    );
  }

  @computed public get contentWidth(): number {
    return (
      this.props.gridDimensions.getColumnRight(
        this.props.gridDimensions.columnCount - 1
      ) + this.props.gridDimensions.getColumnLeft(0)
    );
  }

  @computed
  public get canvasWidthPX() {
    return Math.ceil(this.props.width * CPR) || 0;
  }

  @computed
  public get canvasHeightPX() {
    return Math.ceil(this.props.height * CPR) || 0;
  }

  @computed
  public get canvasWidthCSS() {
    return Math.ceil(this.props.width * CPR) / CPR || 0;
  }

  @computed
  public get canvasHeightCSS() {
    return Math.ceil(this.props.height * CPR) / CPR || 0;
  }

  @computed
  public get canvasProps() {
    return {
      width: this.canvasWidthPX,
      height: this.canvasHeightPX,
      style: {
        // +1 because added 1px border makes canvas resizing and blurry.
        // Has to by synchronized with stylesheet :(
        minWidth: this.canvasWidthCSS + 1,
        maxWidth: this.canvasWidthCSS + 1,
        minHeight: this.canvasHeightCSS,
        maxHeight: this.canvasHeightCSS
      }
    };
  }

  @computed
  get visibleRowsRange() {
    return rangeQuery(
      i => this.props.gridDimensions.getRowBottom(i),
      i => this.props.gridDimensions.getRowTop(i),
      this.props.gridDimensions.rowCount,
      this.scrollTop,
      this.scrollTop + this.props.height
    );
  }

  @computed public get firstVisibleRowIndex(): number {
    return this.visibleRowsRange.fgte;
  }

  @computed public get lastVisibleRowIndex(): number {
    return this.visibleRowsRange.llte;
  }

  @computed
  public get visibleColumnsRange() {
    return rangeQuery(
      i => this.props.gridDimensions.getColumnRight(i),
      i => this.props.gridDimensions.getColumnLeft(i),
      this.props.gridDimensions.columnCount,
      this.scrollLeft,
      this.scrollLeft + this.props.width
    );
  }

  @computed public get firstVisibleColumnIndex(): number {
    return this.visibleColumnsRange.fgte;
  }

  @computed public get lastVisibleColumnIndex(): number {
    return this.visibleColumnsRange.llte;
  }

  @action.bound private handleRefCanvas(element: HTMLCanvasElement) {
    this.refCanvas = element;
    if (element) {
      this.ctxCanvas = element.getContext("2d");
    } else {
      this.ctxCanvas = null;
    }
  }

  @bind
  private renderingReactionEffect() {
    const ctx = this.ctxCanvas;
    if (!ctx) {
      return;
    }
    this.repaintTrigger;
    const { width, height } = this.props;
    const {
      firstVisibleColumnIndex,
      lastVisibleColumnIndex,
      firstVisibleRowIndex,
      lastVisibleRowIndex
    } = this;

    ctx.fillStyle = "white";
    ctx.fillRect(0, 0, width * CPR, height * CPR);

    for (
      let columnIndex = firstVisibleColumnIndex;
      columnIndex <= lastVisibleColumnIndex;
      columnIndex++
    ) {
      for (
        let rowIndex = firstVisibleRowIndex;
        rowIndex <= lastVisibleRowIndex;
        rowIndex++
      ) {
        this.renderCell(columnIndex, rowIndex, ctx);
      }
    }
  }

  private renderCell(
    columnIndex: number,
    rowIndex: number,
    ctx: CanvasRenderingContext2D
  ) {
    const dim = this.props.gridDimensions;
    const columnLeft = dim.getColumnLeft(columnIndex);
    const columnRight = dim.getColumnRight(columnIndex);
    const columnWidth = dim.getColumnWidth(columnIndex);
    const rowTop = dim.getRowTop(rowIndex);
    const rowBottom = dim.getRowBottom(rowIndex);
    const rowHeight = dim.getRowHeight(rowIndex);

    ctx.save();

    // Move origin to top left corner of the cell being drawn.
    ctx.translate(
      (columnLeft - this.scrollLeft) * CPR,
      (rowTop - this.scrollTop) * CPR
    );

    this.props.renderCell(
      rowIndex,
      columnIndex,
      -this.scrollTop,
      -this.scrollLeft,
      columnLeft,
      columnWidth,
      columnRight,
      rowTop,
      rowHeight,
      rowBottom,
      ctx
    );

    ctx.restore();
  }

  @bind
  private renderingReactionData() {
    this.repaintTrigger;
    this.ctxCanvas;
    this.props.width;
    this.props.height;
    this.scrollTop;
    this.scrollLeft;
    return [];
  }

  public componentDidMount() {
    this.reactionDisposers.push(
      autorun(this.renderingReactionEffect, {
        scheduler(fn) {
          requestAnimationFrame(fn);
        }
      })
    );
  }

  public componentDidUpdate() {
    runInAction(() => {
      this.repaintTrigger++;
    });
  }

  public componentWillUnmount() {
    this.reactionDisposers.forEach(d => d());
  }

  public render() {
    return (
      <canvas
        className="grid-table-canvas"
        {...this.canvasProps}
        ref={this.handleRefCanvas}
      />
    );
  }
}
