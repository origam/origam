import * as React from "react";
import GridTable from "./Table";
import { computed, action } from "mobx";
import { IGridDimensions } from "src/uiComponents/controls/GridTable2/types";
import { bind } from "bind-decorator";
import GridCursor from "./Cursor";
import { IGridCursorPos } from "./types";

import { IDataCursorState, IGridTableEvents } from "../../../Grid/types2";
import { CPR } from "src/utils/canvas";
import { IDataTableSelectors } from "src/Grid/types2";
import { inject, Observer } from "mobx-react";
import { GridTableEvents } from "src/Grid/GridTableEvents";
import DataCursorState from "../../../Grid/DataCursorState";
import { EventObserver, IEventSubscriber } from "src/utils/events";

class GridDimensions implements IGridDimensions {
  @computed public get rowCount(): number {
    return 10000;
  }

  @computed public get columnCount(): number {
    return 100;
  }

  @computed public get contentWidth(): number {
    return this.getColumnRight(this.columnCount - 1) - this.getColumnLeft(0);
  }

  @computed public get contentHeight(): number {
    return this.getRowBottom(this.rowCount - 1) - this.getRowTop(0);
  }

  @bind
  public getColumnLeft(columnIndex: number): number {
    return columnIndex * 50;
  }

  @bind
  public getColumnWidth(columnIndex: number): number {
    return 50;
  }

  @bind
  public getColumnRight(columnIndex: number): number {
    return this.getColumnLeft(columnIndex) + this.getColumnWidth(columnIndex);
  }

  @bind
  public getRowTop(rowIndex: number): number {
    return rowIndex * 20;
  }

  @bind
  public getRowHeight(rowIndex: number): number {
    return 20;
  }

  @bind
  public getRowBottom(rowIndex: number): number {
    return this.getRowTop(rowIndex) + this.getRowHeight(rowIndex);
  }
}

const gridDimensions = new GridDimensions();

class GridCursorPos implements IGridCursorPos {
  constructor(
    private dataTableSelectors: IDataTableSelectors,
    private dataCursorState: IDataCursorState
  ) {}

  @computed get selectedRowIndex() {
    return this.dataCursorState.selectedRecordId
      ? this.dataTableSelectors.recordIndexById(
          this.dataCursorState.selectedRecordId
        )
      : undefined;
  }

  @computed public get selectedColumnIndex() {
    return this.dataCursorState.selectedFieldId
      ? this.dataTableSelectors.fieldIndexById(
          this.dataCursorState.selectedFieldId
        )
      : undefined;
  }
}

const renderCell = (gridTableEvents: IGridTableEvents) => (
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
  onCellClick: IEventSubscriber
) => {
  ctx.font = `${12 * CPR}px sans-serif`;
  ctx.fillStyle = rowIndex % 2 === 0 ? "#ffffff" : "#efefef";
  ctx.fillRect(0, 0, columnWidth * CPR, rowHeight * CPR);
  ctx.fillStyle = "black";

  const text = `${columnIndex}/${rowIndex}`;
  ctx.fillText("" + text!, 15 * CPR, 15 * CPR);

  onCellClick((event: any, x: number, y: number) => {
    // console.log(x, y)
    if (x >= columnLeft && x <= columnRight && y >= rowTop && y <= rowBottom) {
      gridTableEvents.handleCellClick(event, rowIndex, columnIndex);
    }
  });
};

function renderHeader(columnIndex: number): React.ReactNode {
  return <div className="column-header-label">{columnIndex}</div>;
}

interface ITableCndProps {
  dataTableSelectors?: IDataTableSelectors;
  dataCursorState?: IDataCursorState;
}

@inject("dataTableSelectors", "dataCursorState")
export default class TableCnd extends React.Component<ITableCndProps> {
  constructor(props: any) {
    super(props);
  }

  private gridCursorPos: IGridCursorPos = new GridCursorPos(
    this.props.dataTableSelectors!,
    this.props.dataCursorState!
  );
  private gridTableEvents: IGridTableEvents = new GridTableEvents(
    this.props.dataCursorState!,
    this.props.dataTableSelectors!
  );
  private renderCell = renderCell(this.gridTableEvents);

  private refGridTable_disposeHandler: () => void;
  @action.bound
  private refGridTable(elm: GridTable | null) {
    if (elm) {
      this.refGridTable_disposeHandler = this.gridTableEvents.onCursorMovementFinished(
        () => {
          elm.scrollToColumnShortest(this.gridCursorPos.selectedColumnIndex!);
          elm.scrollToRowShortest(this.gridCursorPos.selectedRowIndex!);
        }
      );
    } else {
      this.refGridTable_disposeHandler();
    }
  }

  public render() {
    return (
      <GridTable
        ref={this.refGridTable}
        gridCursorPos={this.gridCursorPos}
        gridDimensions={gridDimensions}
        fixedColumnSettings={{ fixedColumnCount: 2 }}
        renderGridCursor={(gDim, gCursorPos) => (
          <Observer>
            {() => (
              <GridCursor
                gridDimensions={gDim}
                gridCursorPos={gCursorPos}
                cellContent={
                  <Observer>
                    {() => this.props.dataCursorState!.isEditing && "EDITING"}
                  </Observer>
                }
              />
            )}
          </Observer>
        )}
        renderHeader={renderHeader}
        renderCell={this.renderCell}
        onKeyDown={this.gridTableEvents.handleGridKeyDown}
      />
    );
  }
}
