import { decorate, action, autorun, IReactionDisposer } from "mobx";
import { trcpr } from "../utils/canvas";
import {
  IGridState,
  IGridSelectors,
  IGridActions,
  IGridSetup,
  IClickSubscription,
  T$2,
  T$4,
  T$1,
  IGridProps
} from "./types";
import { getElementPosition } from "../utils/elements";
// import { getElementPosition } from "../utils/elements";

class AnimationFrameScheduler {
  private scheduled: boolean = false;

  public schedule(fn: () => void) {
    if (!this.scheduled) {
      this.scheduled = true;
      requestAnimationFrame(() => {
        this.scheduled = false;
        fn();
      });
    }
  }
}

decorate(AnimationFrameScheduler, {
  schedule: action.bound
});

export class GridActions implements IGridActions {
  constructor(
    public state: IGridState,
    public selectors: IGridSelectors,
    public setup: IGridSetup
  ) {
    this.repaintScheduler = new AnimationFrameScheduler();
  }

  private repaintScheduler: AnimationFrameScheduler;
  private rePainter: IReactionDisposer | undefined;

  private clickSubscriptions: IClickSubscription[] = [];

  get fixedColumnCount(): number {
    return this.selectors.fixedColumnCount;
  }

  public handleResize(width: number, height: number): void {
    this.state.setSize(width, height);
  }

  public handleScroll(event: any): void {
    if (!this.setup.isScrollingEnabled) {
      event.target.scrollTop = this.selectors.scrollTop;
      event.target.scrollLeft = this.selectors.scrollLeft;
      return;
    }
    const { scrollLeft, scrollTop } = event.target;
    this.state.setScroll(scrollTop, scrollLeft);
    if (this.selectors.onScroll) {
      const {
        visibleColumnsFirstIndex,
        visibleColumnsLastIndex,
        visibleRowsFirstIndex,
        visibleRowsLastIndex
      } = this.selectors;
      this.selectors.onScroll({
        scrollLeft,
        scrollTop,
        visibleColumnsFirstIndex,
        visibleColumnsLastIndex,
        visibleRowsFirstIndex,
        visibleRowsLastIndex
      });
    }
  }

  public handleGridClick(event: any): void {
    const { clientX, clientY } = event;
    const { x: elmLeft, y: elmTop } = getElementPosition(
      this.selectors.elmScroller!
    );
    const leftInWorld = clientX - elmLeft; // + this.scrollLeft;
    const topInWorld = clientY - elmTop + this.selectors.scrollTop;
    // console.log(leftInWorld, topInWorld, clientX, clientY, elmLeft, elmTop, this.elmScroller)
    const clickSubscriptions = [...this.clickSubscriptions];
    clickSubscriptions.sort(
      (a, b) => a.cellInfo.columnIndex - b.cellInfo.columnIndex
    );
    for (const clickSub of clickSubscriptions) {
      const { cellRect, cellRealRect, cellInfo, handler } = clickSub;
      const { left, right, top, bottom } = cellRealRect;
      if (
        leftInWorld >= left &&
        leftInWorld < right &&
        topInWorld >= top &&
        topInWorld < bottom
      ) {
        handler(event, cellRect, cellInfo);
        return;
      }
    }

    // TODO...
    // onNoCellClick && onNoCellClick(event);
    // TODO...
  }

  public handleKeyDown(event: any): void {
    throw new Error("Method not implemented.");
  }

  public refRoot(element: HTMLDivElement): void {
    this.state.setRefRoot(element);
  }

  public refScroller(element: HTMLDivElement): void {
    this.state.setRefScroller(element);
  }

  public refCanvas(element: HTMLCanvasElement): void {
    this.state.setRefCanvas(element);
    if (element) {
      this.state.setCanvasContext(element.getContext("2d")!);
    } else {
      this.state.setCanvasContext(null);
    }
  }

  private handleWindowClick(event: any) {
    if (
      !this.selectors.elmRoot ||
      !this.selectors.elmRoot.contains(event.target)
    ) {
      this.selectors.onOutsideClick && this.selectors.onOutsideClick(event);
    }
  }

  public componentDidMount(props: IGridProps) {
    this.updateByComponentProps(props);
    window.addEventListener("click", this.handleWindowClick);
    this.rePainter = autorun(
      () => {
        this.repaint();
      },
      {
        scheduler: this.repaintScheduler.schedule
      }
    );
  }

  public componentDidUpdate(prevProps: IGridProps, nextProps: IGridProps) {
    this.updateByComponentProps(nextProps);
  }

  public updateByComponentProps(props: IGridProps): void {
    this.state.setCellRenderer(props.cellRenderer);
    this.state.setOnScroll(props.onScroll);
    this.state.setOnOutsideClick(props.onOutsideClick);
  }

  public componentWillUnmount(): void {
    window.removeEventListener("click", this.handleWindowClick);
    this.rePainter && this.rePainter();
  }

  public performScrollTo({
    scrollTop,
    scrollLeft
  }: {
    scrollTop?: number;
    scrollLeft?: number;
  }) {
    if (scrollTop !== undefined) {
      this.state.setScrollTop(scrollTop);
      this.selectors.elmScroller!.scrollTop = scrollTop;
    }
    if (scrollLeft !== undefined) {
      this.state.setScrollLeft(scrollLeft);
      this.selectors.elmScroller!.scrollLeft = scrollLeft;
    }
  }

  public performIncrementScroll({
    scrollTop,
    scrollLeft
  }: {
    scrollTop?: number;
    scrollLeft?: number;
  }) {
    if (scrollTop !== undefined) {
      this.performScrollTo({ scrollTop: scrollTop + this.selectors.scrollTop });
    }
    if (scrollLeft !== undefined) {
      this.performScrollTo({
        scrollLeft: scrollLeft + this.selectors.scrollLeft
      });
    }
  }

  private fixedShiftedColumnDimensions({
    left,
    width,
    columnIndex
  }: {
    left: number;
    width: number;
    columnIndex: number;
  }) {
    return this.selectors.fixedShiftedColumnDimensions(
      left,
      width,
      columnIndex
    );
  }

  private paintCell(
    ctx: CanvasRenderingContext2D,
    shiftLeft: number,
    shiftTop: number,
    i: number,
    j: number
  ) {
    const { selectors } = this;
    const { cellRenderer } = selectors;
    ctx.save();
    const columnLeft = selectors.getColumnLeft(j);
    const columnRight = selectors.getColumnRight(j);
    const rowTop = selectors.getRowTop(i);
    const rowBottom = selectors.getRowBottom(i);
    const cellWidth = columnRight - columnLeft;
    const cellHeight = rowBottom - rowTop;

    const cellRect = {
      left: columnLeft,
      top: rowTop,
      width: cellWidth,
      height: cellHeight,
      right: columnRight,
      bottom: rowBottom
    };

    const realColumnDimensions = this.fixedShiftedColumnDimensions({
      left: cellRect.left,
      width: cellRect.width,
      columnIndex: j
    });

    const cellRealRect = {
      ...cellRect,
      left: realColumnDimensions.left,
      width: realColumnDimensions.width,
      right: realColumnDimensions.left + realColumnDimensions.width
    };

    const cellInfo = {
      columnIndex: j,
      rowIndex: i
    };

    ctx.translate(...(trcpr(columnLeft + shiftLeft, rowTop + shiftTop) as T$2));
    cellRenderer({
      columnIndex: j,
      rowIndex: i,
      cellDimensions: {
        width: cellWidth,
        height: cellHeight
      },
      ctx,
      events: {
        onClick: handler => {
          this.clickSubscriptions.push({
            cellRect,
            cellRealRect,
            cellInfo,
            handler
          });
        }
      }
    });
    ctx.restore();
  }

  private repaint() {
    this.clickSubscriptions = [];
    const ctx = this.selectors.canvasContext;
    const {
      scrollLeft,
      scrollTop,
      width,
      height,
      visibleRowsFirstIndex,
      visibleRowsLastIndex,
      visibleColumnsFirstIndex,
      visibleColumnsLastIndex,
      fixedColumnsTotalWidth,
      movingColumnsTotalWidth
    } = this.selectors;
    const { fixedColumnCount } = this;
    if (!ctx) {
      return;
    }
    ctx.fillStyle = "white";
    ctx.fillRect(...(trcpr(0, 0, width, height) as T$4));
    ctx.fillStyle = "black";
    ctx.font = `${Math.round(...(trcpr(12) as T$1))}px sans-serif`;
    ctx.save();
    ctx.beginPath();
    ctx.rect(
      ...(trcpr(
        fixedColumnsTotalWidth,
        0,
        movingColumnsTotalWidth,
        height
      ) as T$4)
    );
    ctx.clip();
    for (let j = visibleColumnsFirstIndex; j <= visibleColumnsLastIndex; j++) {
      if (j < fixedColumnCount) {
        continue;
      }
      for (let i = visibleRowsFirstIndex; i <= visibleRowsLastIndex; i++) {
        this.paintCell(ctx, -scrollLeft, -scrollTop, i, j);
      }
    }
    ctx.restore();
    if (fixedColumnsTotalWidth > 0) {
      ctx.save();
      ctx.beginPath();
      ctx.rect(...(trcpr(0, 0, fixedColumnsTotalWidth, height) as T$4));
      ctx.clip();
      for (let j = 0; j < fixedColumnCount; j++) {
        for (let i = visibleRowsFirstIndex; i <= visibleRowsLastIndex; i++) {
          this.paintCell(ctx, 0, -scrollTop, i, j);
        }
      }
      ctx.restore();

      ctx.save();
      ctx.beginPath();
      ctx.moveTo(...(trcpr(fixedColumnsTotalWidth, 0) as T$2));
      ctx.lineTo(...(trcpr(fixedColumnsTotalWidth, height) as T$2));
      ctx.strokeStyle = "#e0e0e0";
      ctx.lineWidth = 3;
      ctx.stroke();
      ctx.restore();
    }
    this.setup.onRowsRendered &&
      this.setup.onRowsRendered(visibleRowsFirstIndex, visibleRowsLastIndex);
  }

  public scheduleRepaint() {
    this.repaintScheduler.schedule(() => this.repaint());
  }
}

decorate(GridActions, {
  performScrollTo: action.bound,
  componentDidMount: action.bound,
  componentWillUnmount: action.bound,
  componentDidUpdate: action.bound,
  handleResize: action.bound,
  refRoot: action.bound,
  refScroller: action.bound,
  refCanvas: action.bound,
  scheduleRepaint: action.bound
});
