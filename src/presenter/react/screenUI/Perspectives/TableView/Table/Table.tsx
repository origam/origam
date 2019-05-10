import * as React from "react";
import ReactDOM from 'react-dom';
import Canvas from "./Canvas";

import Measure, { BoundingRect, ContentRect } from "react-measure";
import { PositionedField } from "./Form";
import Scroller from "./Scroller";
import { observer, Observer } from "mobx-react";
import { observable, action, computed } from "mobx";
import Scrollee from "./Scrollee";
import { HeaderRow } from "./Headers";
import moment from "moment";
import { Editor } from "../../Editor";
import {
  ICell,
  IFormField,
  ITable
} from "../../../../../view/Perspectives/TableView/types";
import { PubSub } from "../../../../../../utils/events";
import { CPR } from "../../../../../../utils/canvas";
import { isType } from "ts-action";
import * as TableViewActions from "../../../../../../DataView/TableView/TableViewActions";

const renderCell = (table: ITable) => (
  rowIndex: number,
  columnIndex: number,
  topOffset: number,
  leftOffset: number,
  columnLeft: number,
  columnWidth: number,
  columnRight: number,
  rowTop: number,
  rowHeight: number,
  rowBottom: number,
  ctx: CanvasRenderingContext2D,
  cell: ICell,
  cursor: IFormField,
  onCellClick: PubSub<any>
) => {
  onCellClick.subscribe((event: any) => {
    console.log(rowIndex, columnIndex);
    table.onCellClick && table.onCellClick(event, rowIndex, columnIndex);
  });

  /* BACKGROUND FILL */
  if (cell.isCellCursor) {
    ctx.fillStyle = "#bbbbbb";
  } else if (cell.isRowCursor) {
    ctx.fillStyle = "#dddddd";
  } else {
    ctx.fillStyle = rowIndex % 2 === 0 ? "#ffffff" : "#efefef";
  }
  ctx.fillRect(0, 0, columnWidth * CPR, rowHeight * CPR);

  /* TEXTUAL CONTENT */
  ctx.font = `${12 * CPR}px sans-serif`;
  if (cell.isLoading) {
    ctx.fillStyle = "#888888";
    ctx.fillText("Loading...", 15 * CPR, 15 * CPR);
  } else {
    ctx.fillStyle = "black";
    switch (cell.type) {
      case "BoolCell":
        ctx.font = `${14 * CPR}px "Font Awesome 5 Free"`;
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText(
          cell.value ? "\uf14a" : "\uf0c8",
          (columnWidth / 2) * CPR,
          (rowHeight / 2) * CPR
        );
        break;
      case "DateTimeCell":
        ctx.fillText(
          moment(cell.value, cell.inputFormat).format(cell.outputFormat),
          15 * CPR,
          15 * CPR
        );
        break;
      default:
        ctx.fillText("" + cell.value!, 15 * CPR, 15 * CPR);
    }
  }

  if (cell.isInvalid) {
    ctx.save();
    ctx.fillStyle = "red";
    ctx.beginPath();
    ctx.moveTo(0, 0);
    ctx.lineTo(0, rowHeight);
    ctx.lineTo(5, rowHeight / 2);
    ctx.closePath();
    ctx.fill();
    /*ctx.fillRect(0, 0, 5, 5);
    ctx.fillRect(0, rowHeight - 5, 5, 5);
    ctx.fillRect(0, 0, 3, rowHeight);*/
    ctx.restore();
  }
};

@observer
export class Table extends React.Component<{ controller: ITable }> {
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
      this.props.controller.onOutsideTableClick &&
        this.props.controller.onOutsideTableClick(event);
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

  disposer: (() => void) | undefined;

  componentDidMount() {
    this.disposer = this.props.controller.listenMediator((action, sender) => {
      if (isType(action, TableViewActions.makeCellVisibleByIdx)) {
        this.scrollToCellShortest(
          action.payload.rowIdx,
          action.payload.columnIdx
        );
      }
    });
  }

  @action.bound scrollToCellShortest(rowIdx: number, columnIdx: number) {
    // TODO: Refactor to take real scrollbar sizes
    const SCROLLBAR_SIZE = 20;
    if (this.elmScroller) {
      const { cells } = this.props.controller;
      const top = cells.getRowTop(rowIdx);
      const bottom = cells.getRowBottom(rowIdx);
      if (columnIdx >= cells.fixedColumnCount) {
        const left = cells.getColumnLeft(columnIdx);
        const right = cells.getColumnRight(columnIdx);

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

  get scrollState() {
    return this.props.controller.scrollState;
  }

  get gridDimensions() {
    return this.props.controller.cells;
  }

  get fixedColumnsCount() {
    return this.props.controller.cells.fixedColumnCount;
  }

  get hasFixedColumns() {
    return this.fixedColumnsCount !== 0;
  }

  @computed get fixedColumnsWidth() {
    if (!this.hasFixedColumns) {
      return 0;
    }
    return (
      this.gridDimensions.getColumnRight(this.fixedColumnsCount - 1) -
      this.gridDimensions.getColumnLeft(0)
    );
  }

  @action.bound handleScrollerClick(event: any) {
    if (event.clientX > this.fixedColumnsWidth) {
      this.elmCanvasMoving &&
        this.elmCanvasMoving.triggerCellClick(
          event,
          event.clientX -
          this.contentBounds.left +
          this.scrollState.scrollLeft -
          this.fixedColumnsWidth,
          event.clientY - this.contentBounds.top + this.scrollState.scrollTop
        );
    } else {
      this.elmCanvasFixed &&
        this.elmCanvasFixed.triggerCellClick(
          event,
          event.clientX - this.contentBounds.left,
          event.clientY - this.contentBounds.top + this.scrollState.scrollTop
        );
    }
  }

  @action.bound handleResize(contentRect: { bounds: BoundingRect }) {
    this.contentBounds = contentRect.bounds;
  }

  render() {
    return (
      <div className="table">
        {this.props.controller.isLoading && (
          <div className="loading-overlay">
            <div className="loading-icon">
              <i className="far fa-clock fa-7x blink" />
            </div>
          </div>
        )}
        <Measure bounds={true} onResize={this.handleResize}>
          {({ measureRef, contentRect }) => (
            <Observer>
              {() => (
                <>
                  {contentRect.bounds!.width ? (
                    <div className="headers">
                      {this.hasFixedColumns ? (
                        <>
                          <Scrollee
                            scrollOffsetSource={this.scrollState}
                            fixedHoriz={true}
                            fixedVert={true}
                            width={this.fixedColumnsWidth}
                          >
                            <HeaderRow
                              gridDimensions={this.gridDimensions}
                              headers={this.props.controller.cells}
                              columnStartIndex={0}
                              columnEndIndex={this.fixedColumnsCount}
                            />
                          </Scrollee>
                          <Scrollee
                            scrollOffsetSource={this.scrollState}
                            fixedVert={true}
                            width={
                              contentRect.bounds!.width -
                              10 -
                              this.fixedColumnsWidth
                            }
                          >
                            <HeaderRow
                              gridDimensions={this.gridDimensions}
                              headers={this.props.controller.cells}
                              columnStartIndex={this.fixedColumnsCount}
                              columnEndIndex={this.gridDimensions.columnCount}
                            />
                          </Scrollee>
                        </>
                      ) : (
                          <Scrollee
                            scrollOffsetSource={this.scrollState}
                            fixedVert={true}
                            width={contentRect.bounds!.width - 10}
                          >
                            <HeaderRow
                              gridDimensions={this.gridDimensions}
                              headers={this.props.controller.cells}
                              columnStartIndex={0}
                              columnEndIndex={this.gridDimensions.columnCount}
                            />
                          </Scrollee>
                        )}
                    </div>
                  ) : null}

                  <div ref={measureRef} className="cell-area-container">
                    {contentRect.bounds!.height ? (
                      <>
                        <div className="canvas-row">
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
                                contentHeight={this.props.controller.cells.contentHeight}
                                scrollOffsetSource={this.scrollState}
                                gridDimensions={this.gridDimensions}
                                cells={this.props.controller.cells}
                                cursor={this.props.controller.cursor}
                                renderCell={renderCell(this.props.controller)}
                                onNoCellClick={this.props.controller.onNoCellClick}
                              />
                              <Canvas
                                ref={this.refCanvasMoving}
                                columnStartIndex={this.fixedColumnsCount}
                                leftOffset={-this.fixedColumnsWidth}
                                isHorizontalScroll={true}
                                width={
                                  contentRect.bounds!.width -
                                  10 -
                                  this.fixedColumnsWidth
                                }
                                contentWidth={
                                  this.props.controller.cells.contentWidth
                                }
                                contentHeight={
                                  this.props.controller.cells.contentHeight
                                }
                                height={contentRect.bounds!.height - 10}
                                scrollOffsetSource={this.scrollState}
                                gridDimensions={this.gridDimensions}
                                cells={this.props.controller.cells}
                                cursor={this.props.controller.cursor}
                                renderCell={renderCell(this.props.controller)}
                                onNoCellClick={this.props.controller.onNoCellClick}
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
                                this.props.controller.cells.contentWidth
                              }
                              contentHeight={
                                this.props.controller.cells.contentHeight
                              }
                              height={contentRect.bounds!.height - 10}
                              scrollOffsetSource={this.scrollState}
                              gridDimensions={this.gridDimensions}
                              cells={this.props.controller.cells}
                              cursor={this.props.controller.cursor}
                              renderCell={renderCell(this.props.controller)}
                              onVisibleDataChanged={(
                                fvci,
                                lvci,
                                fvri,
                                lvri
                              ) => {
                                console.log("VDC", fvci, lvci, fvri, lvri);
                              }}
                              onBeforeRender={
                                this.props.controller.onBeforeRender
                              }
                              onAfterRender={
                                this.props.controller.onAfterRender
                              }
                              onNoCellClick={this.props.controller.onNoCellClick}
                            />
                          )}
                        </div>
                        {this.props.controller.cursor.isEditing && (
                          <PositionedField
                            fixedColumnsCount={this.fixedColumnsCount}
                            rowIndex={this.props.controller.cursor.rowIndex}
                            columnIndex={
                              this.props.controller.cursor.columnIndex
                            }
                            scrollOffsetSource={this.scrollState}
                            gridDimensions={this.gridDimensions}
                            worldBounds={contentRect.bounds!}
                          >
                            {this.props.controller.cursor.field && (
                              <Editor
                                key={`${
                                  this.props.controller.cursor.rowIndex
                                  }@${this.props.controller.cursor.columnIndex}`}
                                field={this.props.controller.cursor.field}
                              />
                            )}
                          </PositionedField>
                        )}
                        <Scroller
                          ref={this.refScroller}
                          width={contentRect.bounds!.width}
                          height={contentRect.bounds!.height}
                          isVisible={true}
                          contentWidth={this.gridDimensions.contentWidth}
                          contentHeight={this.gridDimensions.contentHeight}
                          scrollOffsetTarget={this.scrollState}
                          onClick={this.handleScrollerClick}
                          onKeyDown={this.props.controller.onKeyDown}
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
