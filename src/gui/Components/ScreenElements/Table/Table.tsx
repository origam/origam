import * as React from "react";
import ReactDOM from "react-dom";
import Canvas from "./Canvas";

import Measure, { BoundingRect, ContentRect } from "react-measure";
import { PositionedField } from "./PositionedField";
import Scroller from "./Scroller";
import { observer, Observer } from "mobx-react";
import { observable, action, computed } from "mobx";
import Scrollee from "./Scrollee";
import { HeaderRow } from "./HeaderRow";
import moment from "moment";
import { PubSub } from "../../../../utils/events";
import { CPR } from "../../../../utils/canvas";
import { ITableProps } from "./types";
import S from "./Table.module.css";

@observer
export class Table extends React.Component<ITableProps> {
  @observable contentBounds: BoundingRect = {
    top: 0,
    left: 0,
    bottom: 0,
    right: 0,
    width: 0,
    height: 0
  };
  @observable.ref elmCanvasFixed: Canvas | null = null;
  @observable.ref elmCanvasMoving: Canvas | null = null;
  @observable.ref elmScroller: Scroller | null = null;

  @action.bound handleWindowClick(event: any) {
    const domNode = ReactDOM.findDOMNode(this.elmScroller);
    if (domNode && !domNode.contains(event.target)) {
      this.props.onOutsideTableClick && this.props.onOutsideTableClick(event);
    }
  }

  @action.bound refCanvasFixed(elm: Canvas | null) {
    this.elmCanvasFixed = elm;
  }

  @action.bound refCanvasMoving(elm: Canvas | null) {
    this.elmCanvasMoving = elm;
  }

  @action.bound refScroller(elm: Scroller | null) {
    this.elmScroller = elm;
    if (elm) {
      window.addEventListener("click", this.handleWindowClick);
    } else {
      window.removeEventListener("click", this.handleWindowClick);
    }
  }

  disposers: Array<() => void> = [];

  componentDidMount() {
    this.props.listenForScrollToCell &&
      this.disposers.push(
        this.props.listenForScrollToCell((rowIdx, colIdx) => {
          this.scrollToCellShortest(rowIdx, colIdx);
        })
      );
  }

  componentWillUnmount() {
    this.disposers.forEach(d => d());
  }

  @action.bound scrollToCellShortest(rowIdx: number, columnIdx: number) {
    // TODO: Refactor to take real scrollbar sizes
    const { gridDimensions } = this.props;
    const SCROLLBAR_SIZE = 20;
    if (this.elmScroller) {
      const top = gridDimensions.getRowTop(rowIdx);
      const bottom = gridDimensions.getRowBottom(rowIdx);
      if (columnIdx >= this.fixedColumnCount) {
        const left = gridDimensions.getColumnLeft(columnIdx);
        const right = gridDimensions.getColumnRight(columnIdx);

        if (left - this.elmScroller.scrollLeft < this.fixedColumnsWidth) {
          this.elmScroller.scrollTo({
            scrollLeft: left - this.fixedColumnsWidth
          });
        }
        if (
          right - this.elmScroller.scrollLeft >
          this.contentBounds.width - SCROLLBAR_SIZE
        ) {
          this.elmScroller.scrollTo({
            scrollLeft: right - this.contentBounds.width + SCROLLBAR_SIZE
          });
        }
      }
      if (top - this.elmScroller.scrollTop < 0) {
        this.elmScroller.scrollTo({ scrollTop: top });
      }
      if (
        bottom - this.elmScroller.scrollTop >
        this.contentBounds.height - SCROLLBAR_SIZE
      ) {
        this.elmScroller.scrollTo({
          scrollTop: bottom - this.contentBounds.height + SCROLLBAR_SIZE
        });
      }
    }
  }

  get fixedColumnCount() {
    return this.props.fixedColumnCount || 0;
  }

  get hasFixedColumns() {
    return this.fixedColumnCount !== 0;
  }

  @computed get fixedColumnsWidth() {
    if (!this.hasFixedColumns) {
      return 0;
    }
    return (
      this.props.gridDimensions.getColumnRight(this.fixedColumnCount - 1) -
      this.props.gridDimensions.getColumnLeft(0)
    );
  }

  @action.bound handleScrollerClick(event: any) {
    if (event.clientX > this.fixedColumnsWidth) {
      this.elmCanvasMoving &&
        this.elmCanvasMoving.triggerCellClick(
          event,
          event.clientX -
            this.contentBounds.left +
            this.props.scrollState.scrollLeft -
            this.fixedColumnsWidth,
          event.clientY -
            this.contentBounds.top +
            this.props.scrollState.scrollTop
        );
    } else {
      this.elmCanvasFixed &&
        this.elmCanvasFixed.triggerCellClick(
          event,
          event.clientX - this.contentBounds.left,
          event.clientY -
            this.contentBounds.top +
            this.props.scrollState.scrollTop
        );
    }
  }

  @action.bound handleResize(contentRect: { bounds: BoundingRect }) {
    this.contentBounds = contentRect.bounds;
  }

  render() {
    return (
      <div className={S.table}>
        {this.props.isLoading && (
          <div className={S.loadingOverlay}>
            <div className={S.loadingIcon}>
              <i className="far fa-clock fa-7x blink" />
            </div>
          </div>
        )}
        <Measure bounds={true} onResize={this.handleResize}>
          {({ measureRef, contentRect }) => (
            <Observer>
              {() => (
                <>
                  {" "}
                  {this.props.renderHeader &&
                    (contentRect.bounds!.width ? (
                      <div className={S.headers}>
                        {this.hasFixedColumns ? (
                          <>
                            <Scrollee
                              scrollOffsetSource={this.props.scrollState}
                              fixedHoriz={true}
                              fixedVert={true}
                              width={this.fixedColumnsWidth}
                            >
                              <HeaderRow
                                gridDimensions={this.props.gridDimensions}
                                renderHeader={this.props.renderHeader}
                                columnStartIndex={0}
                                columnEndIndex={this.fixedColumnCount}
                              />
                            </Scrollee>
                            <Scrollee
                              scrollOffsetSource={this.props.scrollState}
                              fixedVert={true}
                              width={
                                contentRect.bounds!.width -
                                10 -
                                this.fixedColumnsWidth
                              }
                            >
                              <HeaderRow
                                gridDimensions={this.props.gridDimensions}
                                renderHeader={this.props.renderHeader}
                                columnStartIndex={this.fixedColumnCount}
                                columnEndIndex={
                                  this.props.gridDimensions.columnCount
                                }
                              />
                            </Scrollee>
                          </>
                        ) : (
                          <Scrollee
                            scrollOffsetSource={this.props.scrollState}
                            fixedVert={true}
                            width={contentRect.bounds!.width - 10}
                          >
                            <HeaderRow
                              gridDimensions={this.props.gridDimensions}
                              renderHeader={this.props.renderHeader}
                              columnStartIndex={0}
                              columnEndIndex={
                                this.props.gridDimensions.columnCount
                              }
                            />
                          </Scrollee>
                        )}
                      </div>
                    ) : null)}
                  <div ref={measureRef} className={S.cellAreaContainer}>
                    {contentRect.bounds!.height && this.props.renderCell ? (
                      <>
                        <div className={S.canvasRow}>
                          {this.hasFixedColumns && (
                            <>
                              <Canvas
                                ref={this.refCanvasFixed}
                                columnStartIndex={0}
                                leftOffset={0}
                                isHorizontalScroll={false}
                                width={this.fixedColumnsWidth}
                                contentWidth={this.fixedColumnsWidth}
                                height={contentRect.bounds!.height - 10}
                                contentHeight={
                                  this.props.gridDimensions.contentHeight
                                }
                                scrollOffsetSource={this.props.scrollState}
                                gridDimensions={this.props.gridDimensions}
                                renderCell={this.props.renderCell}
                                onNoCellClick={this.props.onNoCellClick}
                              />
                              <Canvas
                                ref={this.refCanvasMoving}
                                columnStartIndex={this.fixedColumnCount}
                                leftOffset={-this.fixedColumnsWidth}
                                isHorizontalScroll={true}
                                width={
                                  contentRect.bounds!.width -
                                  10 -
                                  this.fixedColumnsWidth
                                }
                                contentWidth={
                                  this.props.gridDimensions.contentWidth
                                }
                                contentHeight={
                                  this.props.gridDimensions.contentHeight
                                }
                                height={contentRect.bounds!.height - 10}
                                scrollOffsetSource={this.props.scrollState}
                                gridDimensions={this.props.gridDimensions}
                                renderCell={this.props.renderCell}
                                onNoCellClick={this.props.onNoCellClick}
                              />
                            </>
                          )}
                          {!this.hasFixedColumns && (
                            <Canvas
                              ref={this.refCanvasMoving}
                              columnStartIndex={0}
                              leftOffset={0}
                              isHorizontalScroll={true}
                              width={contentRect.bounds!.width - 10}
                              contentWidth={
                                this.props.gridDimensions.contentWidth
                              }
                              contentHeight={
                                this.props.gridDimensions.contentHeight
                              }
                              height={contentRect.bounds!.height - 10}
                              scrollOffsetSource={this.props.scrollState}
                              gridDimensions={this.props.gridDimensions}
                              renderCell={this.props.renderCell}
                              onVisibleDataChanged={(
                                fvci,
                                lvci,
                                fvri,
                                lvri
                              ) => {
                                // console.log("VDC", fvci, lvci, fvri, lvri);
                              }}
                              onBeforeRender={undefined}
                              onAfterRender={undefined}
                              onNoCellClick={this.props.onNoCellClick}
                            />
                          )}
                        </div>
                        {this.props.isEditorMounted &&
                          this.props.editingRowIndex &&
                          this.props.editingColumnIndex && (
                            <PositionedField
                              fixedColumnsCount={this.fixedColumnCount}
                              rowIndex={this.props.editingRowIndex}
                              columnIndex={this.props.editingColumnIndex}
                              scrollOffsetSource={this.props.scrollState}
                              gridDimensions={this.props.gridDimensions}
                              worldBounds={contentRect.bounds!}
                            >
                              {this.props.renderEditor &&
                                this.props.renderEditor()}
                            </PositionedField>
                          )}
                        <Scroller
                          ref={this.refScroller}
                          width={contentRect.bounds!.width}
                          height={contentRect.bounds!.height}
                          isVisible={true}
                          contentWidth={this.props.gridDimensions.contentWidth}
                          contentHeight={
                            this.props.gridDimensions.contentHeight
                          }
                          scrollOffsetTarget={this.props.scrollState}
                          onClick={this.handleScrollerClick}
                          onKeyDown={this.props.onKeyDown}
                        />
                      </>
                    ) : null}
                  </div>
                </>
              )}
            </Observer>
          )}
        </Measure>
      </div>
    );
  }
}
