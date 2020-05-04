import { bind } from "bind-decorator";
import {
  action,
  computed,
  IReactionDisposer,
  observable,
  reaction,
  runInAction,
  autorun,
} from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { IGridCanvasProps } from "./types";
import { PubSub } from "../../../../utils/events";
import { CPR } from "../../../../utils/canvas";
import { rangeQuery } from "../../../../utils/arrays";
import S from "./Canvas.module.css";
import _ from "lodash";
import { SearchResultsPanel } from "gui/Workbench/SearchResults";

/* eslint-disable no-unused-expressions */

/*
  Canvas element, which just draws table-like data shifted by scroll offset 
  given by scrollOffsetSource.
*/

@observer
export default class Canvas extends React.Component<IGridCanvasProps> {
  @observable.ref public refCanvas: HTMLCanvasElement | null = null;
  @observable.ref public ctxCanvas: CanvasRenderingContext2D | null = null;

  // Sometimes the repainting reaction needs to be kicked "artificially",
  // but we do not want to call the painting effect directly, because it would
  // bypss animationFrame scheduling so we just change a value of this field.
  @observable private repaintTrigger = 0;

  private reactionDisposers: IReactionDisposer[] = [];

  private onClickSubscriptions: Array<{
    left: number;
    top: number;
    right: number;
    bottom: number;
    onClick: PubSub<any>;
  }> = [];

  @computed public get scrollTop() {
    return this.props.scrollOffsetSource.scrollTop;
  }

  @computed public get scrollLeft() {
    return this.props.isHorizontalScroll ? this.props.scrollOffsetSource.scrollLeft : 0;
  }

  @computed public get rectTop() {
    return this.scrollTop;
  }

  @computed public get rectLeft() {
    return this.scrollLeft;
  }

  @computed public get rectBottom() {
    return this.scrollTop + this.props.height;
  }

  @computed public get rectRight() {
    return this.scrollLeft + this.props.width;
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
        // Has to by synchronized with stylesheet :/
        minWidth: this.canvasWidthCSS + 1,
        maxWidth: this.canvasWidthCSS + 1,
        minHeight: this.canvasHeightCSS,
        maxHeight: this.canvasHeightCSS,
      },
    };
  }

  @computed
  get visibleRowsRange() {
    return rangeQuery(
      (i) => this.props.gridDimensions.getRowBottom(i),
      (i) => this.props.gridDimensions.getRowTop(i),
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
      (i) => this.props.gridDimensions.getColumnRight(i),
      (i) => this.props.gridDimensions.getColumnLeft(i),
      this.props.gridDimensions.columnCount,
      this.scrollLeft + this.props.leftOffset,
      this.scrollLeft + this.props.width - this.props.leftOffset
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
    this.props.onBeforeRender && this.props.onBeforeRender();
    this.repaintTrigger;
    const { width, height } = this.props;
    const {
      firstVisibleColumnIndex,
      lastVisibleColumnIndex,
      firstVisibleRowIndex,
      lastVisibleRowIndex,
    } = this;

    ctx.fillStyle = "white";
    ctx.fillRect(0, 0, width * CPR, height * CPR);

    this.onClickSubscriptions = [];
    ctx.save();
    /*ctx.beginPath();
    ctx.rect(
      0,
      0,
      Math.min(this.props.contentWidth - this.scrollLeft) * CPR,
      this.props.contentHeight * CPR
    );
    ctx.clip();*/
    for (
      let columnIndex = firstVisibleColumnIndex; // + this.props.columnStartIndex;
      columnIndex <= lastVisibleColumnIndex; // + this.props.columnStartIndex;
      columnIndex++
    ) {
      for (let rowIndex = firstVisibleRowIndex; rowIndex <= lastVisibleRowIndex; rowIndex++) {
        this.renderCell(columnIndex, rowIndex, ctx);
      }
    }
    const dim = this.props.gridDimensions;
    const columnRight = dim.getColumnRight(lastVisibleColumnIndex);
    if (columnRight < this.canvasWidthPX) {
      for (let rowIndex = firstVisibleRowIndex; rowIndex <= lastVisibleRowIndex; rowIndex++) {
        const rowTop = dim.getRowTop(rowIndex);
        const rowBottom = dim.getRowBottom(rowIndex);
        const rowHeight = dim.getRowHeight(rowIndex);
        ctx.fillStyle = rowIndex % 2 === 0 ? "#f7f6fa" : "#ffffff";
        ctx.fillRect(
          columnRight * CPR,
          (rowTop - this.scrollTop) * CPR,
          (this.canvasWidthPX - columnRight) * CPR,
          rowHeight * CPR
        );
      }
    }
    ctx.restore();
    this.props.onAfterRender && this.props.onAfterRender();
  }

  private renderCell(columnIndex: number, rowIndex: number, ctx: CanvasRenderingContext2D) {
    const dim = this.props.gridDimensions;
    const columnLeft = dim.getColumnLeft(columnIndex) + this.props.leftOffset;
    const columnRight = dim.getColumnRight(columnIndex) + this.props.leftOffset;
    const columnWidth = 20//dim.getColumnWidth(columnIndex);
    const rowTop = dim.getRowTop(rowIndex);
    const rowBottom = dim.getRowBottom(rowIndex);
    const rowHeight = dim.getRowHeight(rowIndex);

    const onClickPubSub = new PubSub<any>();
    this.onClickSubscriptions.push({
      left: columnLeft,
      top: rowTop,
      right: columnRight,
      bottom: rowBottom,
      onClick: onClickPubSub,
    });

    ctx.save();

    // Move origin to top left corner of the cell being drawn.
    ctx.translate((columnLeft - this.scrollLeft) * CPR, (rowTop - this.scrollTop) * CPR);

    this.props.renderCell({
      rowIndex,
      columnIndex,
      topOffset: -this.scrollTop,
      leftOffset: -this.scrollLeft,
      columnLeft,
      columnWidth,
      columnRight,
      rowTop,
      rowHeight,
      rowBottom,
      ctx,
      onCellClick: onClickPubSub,
    });

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
        },
      }),
      reaction(
        () =>
          [
            this.firstVisibleColumnIndex,
            this.lastVisibleColumnIndex,
            this.firstVisibleRowIndex,
            this.lastVisibleRowIndex,
          ] as [number, number, number, number],
        (data) => {
          this.props.onVisibleDataChanged && this.props.onVisibleDataChanged(...data);
        }
      )
    );
  }

  public componentDidUpdate() {
    runInAction(() => {
      this.repaintTrigger++;
    });
  }

  public componentWillUnmount() {
    this.reactionDisposers.forEach((d) => d());
  }

  @action.bound public triggerCellClick(event: any, x: number, y: number) {
    let triggered = false;
    for (let subs of this.onClickSubscriptions) {
      if (subs.left <= x && x < subs.right && subs.top <= y && y < subs.bottom) {
        triggered = true;
        subs.onClick.trigger(event);
      }
    }
    if (!triggered) {
      this.props.onNoCellClick && this.props.onNoCellClick(event);
    }
  }

  public render() {
    return (
      <canvas
        className={S.canvas}
        {...this.canvasProps}
        /*width={0}
        height={0}*/
        ref={this.handleRefCanvas}
      />
    );
  }
}

