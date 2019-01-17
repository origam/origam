import * as React from "react";
import { observable, action, when, IReactionDisposer, computed } from "mobx";
import { AutoSizer } from "react-virtualized";
import GridLayout from "./Layout";
import {
  IGridDimensions,
  IGridTableProps,
  IRenderHeader,
  IGridCursorPos
} from "./types";
import Scrollee from "./Scrollee";
import GridCanvas from "./Canvas";
import Scroller from "./Scroller";
import { observer, Observer } from "mobx-react";
import GridCursor from "./Cursor";
import { GridDimensions, GridCursorPos } from "./GridSplitter";
import { IFixedColumnSettings } from "./types";
import bind from "bind-decorator";

const Headers = observer(
  ({
    gridDimensions,
    startIndex,
    renderHeader
  }: {
    gridDimensions: IGridDimensions;
    startIndex: number;
    renderHeader: IRenderHeader;
  }) => {
    const headers: React.ReactNode[] = [];
    for (let i = startIndex; i < gridDimensions.columnCount; i++) {
      headers.push(
        <div
          key={i}
          className="grid-column-header"
          style={{ minWidth: gridDimensions.getColumnWidth(i) }}
        >
          {renderHeader(i)}
        </div>
      );
    }
    return <div className="grid-column-headers">{headers}</div>;
  }
);

@observer
export default class GridTable extends React.Component<IGridTableProps> {
  constructor(props: IGridTableProps) {
    super(props);

    this.gridDimensionsFixed = new GridDimensions(
      props.gridDimensions,
      props.fixedColumnSettings,
      true
    );
    this.gridDimensionsMoving = new GridDimensions(
      props.gridDimensions,
      props.fixedColumnSettings,
      false
    );
    this.gridCursorPosFixed = new GridCursorPos(
      props.gridCursorPos,
      props.fixedColumnSettings,
      true
    );
    this.gridCursorPosMoving = new GridCursorPos(
      props.gridCursorPos,
      props.fixedColumnSettings,
      false
    );
  }

  private gridDimensionsFixed: IGridDimensions;
  private gridDimensionsMoving: IGridDimensions;
  private gridCursorPosFixed: IGridCursorPos;
  private gridCursorPosMoving: IGridCursorPos;

  private elmGridCanvasMoving: GridCanvas | null;
  private elmGridCanvasFixed: GridCanvas | null;
  private elmGridScroller: Scroller | null;

  @observable public scrollTop: number = 0;
  @observable public scrollLeft: number = 0;

  @computed private get isFixedCursor(): boolean {
    return !!(
      this.props.gridCursorPos.selectedColumnIndex !== undefined &&
      this.props.gridCursorPos.selectedColumnIndex <
        this.props.fixedColumnSettings.fixedColumnCount
    );
  }

  @action.bound public setScrollOffset(
    scrollTop: number,
    scrollLeft: number
  ): void {
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }

  private scrollToRowShortestWhenDisposer: IReactionDisposer | undefined;
  @action.bound public scrollToRowShortest(rowIndex: number) {
    this.scrollToRowShortestWhenDisposer &&
      this.scrollToRowShortestWhenDisposer();
    this.scrollToRowShortestWhenDisposer = when(
      () => Boolean(this.elmGridCanvasMoving && this.elmGridScroller),
      () => {
        this.scrollToRowShortestWhenDisposer = undefined;
        const top = this.props.gridDimensions.getRowTop(rowIndex);
        const bottom = this.props.gridDimensions.getRowBottom(rowIndex);
        if (top < this.elmGridCanvasMoving!.rectTop) {
          this.elmGridScroller!.scrollTop =
            this.scrollTop - (this.elmGridCanvasMoving!.rectTop - top);
        } else if (bottom > this.elmGridCanvasMoving!.rectBottom) {
          this.elmGridScroller!.scrollTop =
            this.scrollTop +
            (bottom - this.elmGridCanvasMoving!.rectBottom) +
            this.elmGridScroller!.horizontalScrollbarSize;
        }
      }
    );
  }

  private scrollToColumnShortestWhenDisposer: IReactionDisposer | undefined;
  @action.bound public scrollToColumnShortest(columnIndex: number) {
    this.scrollToColumnShortestWhenDisposer &&
      this.scrollToColumnShortestWhenDisposer();
    this.scrollToColumnShortestWhenDisposer = when(
      () => Boolean(this.elmGridCanvasMoving && this.elmGridScroller),
      () => {
        this.scrollToColumnShortestWhenDisposer = undefined;
        if (columnIndex >= this.props.fixedColumnSettings.fixedColumnCount) {
          const left = this.gridDimensionsMoving.getColumnLeft(columnIndex);
          const right = this.gridDimensionsMoving.getColumnRight(columnIndex);
          if (left < this.elmGridCanvasMoving!.rectLeft) {
            this.elmGridScroller!.scrollLeft =
              this.scrollLeft - (this.elmGridCanvasMoving!.rectLeft - left);
          } else if (right > this.elmGridCanvasMoving!.rectRight) {
            this.elmGridScroller!.scrollLeft =
              this.scrollLeft +
              (right - this.elmGridCanvasMoving!.rectRight) +
              this.elmGridScroller!.verticalScrollbarSize;
          }
        }
      }
    );
  }

  @action.bound private handleScrollerClick(
    event: any,
    scrollerX: number,
    scrollerY: number
  ) {
    if (scrollerX <= this.gridDimensionsFixed.contentWidth) {
      // Clicked to the fixed area...
      console.log(scrollerX, scrollerY + this.scrollTop);
      this.elmGridCanvasFixed!.triggerCellClick(
        event,
        scrollerX,
        scrollerY + this.scrollTop
      );
    } else {
      console.log(
        scrollerX - this.gridDimensionsFixed.contentWidth + this.scrollLeft,
        scrollerY + this.scrollTop
      );
      this.elmGridCanvasMoving!.triggerCellClick(
        event,
        scrollerX - this.gridDimensionsFixed.contentWidth + this.scrollLeft,
        scrollerY + this.scrollTop
      );
    }
  }

  @action.bound private refGridCanvasMoving(elm: GridCanvas) {
    this.elmGridCanvasMoving = elm;
  }

  @action.bound private refGridCanvasFixed(elm: GridCanvas) {
    this.elmGridCanvasFixed = elm;
  }

  @action.bound private refGridScroller(elm: Scroller) {
    this.elmGridScroller = elm;
  }

  @bind
  public focusGridScroller() {
    this.elmGridScroller && this.elmGridScroller.focus();
  }

  public render() {
    const fixGDim = this.gridDimensionsFixed;
    const movGDim = this.gridDimensionsMoving;
    return (
      <div style={{ width: "100%", height: "100%" }}>
        <AutoSizer>
          {({ width: tableWidth, height: tableHeight }) => (
            <Observer>
              {() => (
                <GridLayout
                  width={tableWidth}
                  height={tableHeight}
                  fixedColumnsTotalWidth={fixGDim.contentWidth}
                  fixedColumnsHeaders={
                    <Scrollee
                      width={fixGDim.contentWidth}
                      height={undefined}
                      fixedHoriz={true}
                      fixedVert={true}
                      scrollOffsetSource={this}
                    >
                      <Headers
                        startIndex={0}
                        gridDimensions={fixGDim}
                        renderHeader={this.props.renderHeader}
                      />
                    </Scrollee>
                  }
                  movingColumnsHeaders={
                    <Scrollee
                      width={tableWidth - fixGDim.contentWidth}
                      height={undefined}
                      fixedVert={true}
                      scrollOffsetSource={this}
                    >
                      <Headers
                        startIndex={
                          this.props.fixedColumnSettings.fixedColumnCount
                        }
                        gridDimensions={movGDim}
                        renderHeader={this.props.renderHeader}
                      />
                    </Scrollee>
                  }
                  fixedColumnsCanvas={
                    <AutoSizer>
                      {({ width, height }) => (
                        <Observer>
                          {() => (
                            <GridCanvas
                              ref={this.refGridCanvasFixed}
                              renderCell={this.props.renderCell}
                              width={width}
                              height={height}
                              gridDimensions={fixGDim}
                              scrollOffsetSource={this}
                              fixedHoriz={true}
                            />
                          )}
                        </Observer>
                      )}
                    </AutoSizer>
                  }
                  movingColumnsCanvas={
                    <AutoSizer>
                      {({ width, height }) => (
                        <Observer>
                          {() => (
                            <GridCanvas
                              ref={this.refGridCanvasMoving}
                              renderCell={this.props.renderCell}
                              width={width}
                              height={height}
                              gridDimensions={movGDim}
                              scrollOffsetSource={this}
                            />
                          )}
                        </Observer>
                      )}
                    </AutoSizer>
                  }
                  fixedColumnsCursor={
                    <Scrollee
                      width={fixGDim.contentWidth}
                      height={"100%"}
                      fixedHoriz={true}
                      fixedVert={false}
                      scrollOffsetSource={this}
                    >
                      <Observer>
                        {() =>
                          this.props.renderGridCursor(
                            fixGDim,
                            this.gridCursorPosFixed,
                            this.isFixedCursor
                          )
                        }
                      </Observer>
                    </Scrollee>
                  }
                  movingColumnsCursor={
                    <Scrollee
                      width={movGDim.contentWidth}
                      height={"100%"}
                      fixedHoriz={false}
                      fixedVert={false}
                      scrollOffsetSource={this}
                    >
                      <Observer>
                        {() =>
                          this.props.renderGridCursor(
                            movGDim,
                            this.gridCursorPosMoving,
                            !this.isFixedCursor
                          )
                        }
                      </Observer>
                    </Scrollee>
                  }
                  scroller={
                    <Scroller
                      isVisible={this.props.showScroller}
                      ref={this.refGridScroller}
                      width={"100%"}
                      height={"100%"}
                      contentWidth={movGDim.contentWidth}
                      contentHeight={movGDim.contentHeight}
                      scrollOffsetTarget={this}
                      onClick={this.handleScrollerClick}
                      onOutsideClick={this.props.onOutsideClick}
                      onKeyDown={this.props.onKeyDown}
                    />
                  }
                  editor={this.props.editor}
                />
              )}
            </Observer>
          )}
        </AutoSizer>
      </div>
    );
  }
}
