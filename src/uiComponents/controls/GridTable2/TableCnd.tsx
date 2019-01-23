import * as React from "react";
import GridTable from "./Table";
import { computed, action, observable } from "mobx";
import { IGridDimensions } from "src/uiComponents/controls/GridTable2/types";
import { bind } from "bind-decorator";
import GridCursor from "./Cursor";
import { IGridCursorPos, ITableView } from "./types";

import { IDataCursorState, IGridTableEvents } from "../../../Grid/types2";
import { CPR } from "src/utils/canvas";
import { IDataTableSelectors } from "src/Grid/types2";
import { inject, Observer, observer } from "mobx-react";
import { GridTableEvents } from "src/Grid/GridTableEvents";
import DataCursorState from "../../../Grid/DataCursorState";
import { EventObserver, IEventSubscriber } from "src/utils/events";
import { Rect } from "react-measure";
import { StringGridEditor } from "src/cells/string/GridEditor";
import { IDataViewState, IDataViewType } from "../../skeleton/types";

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
  dataViewState?: IDataViewState;
}

@inject("dataTableSelectors", "dataCursorState", "dataViewState")
@observer
export default class TableCnd extends React.Component<ITableCndProps>
  implements ITableView {
  constructor(props: any) {
    super(props);
  }

  private gridCursorPos: IGridCursorPos = new GridCursorPos(
    this.props.dataTableSelectors!,
    this.props.dataCursorState!
  );
  private gridTableEvents: IGridTableEvents = new GridTableEvents(
    this.props.dataCursorState!,
    this.props.dataTableSelectors!,
    this
  );
  private renderCell = renderCell(this.gridTableEvents);

  @observable.ref private elmGridCursor: GridCursor | null;

  @action.bound private refGridCursor(elm: GridCursor | null) {
    this.elmGridCursor = elm;
  }

  private refGridTable_disposeHandlers: Array<() => void> = [];
  @action.bound
  private refGridTable(elm: GridTable | null) {
    if (elm) {
      this.refGridTable_disposeHandlers.push(
        this.gridTableEvents.onCursorMovementFinished(() => {
          elm.scrollToColumnShortest(this.gridCursorPos.selectedColumnIndex!);
          elm.scrollToRowShortest(this.gridCursorPos.selectedRowIndex!);
        }),
        this.props.dataCursorState!.onEditingEnded(() => {
          elm.focusGridScroller();
        })
      );
    } else {
      this.refGridTable_disposeHandlers.forEach(h => h());
    }
  }

  @computed public get editorContainerStyle() {
    return (this.elmGridCursor
      ? {
          position: "fixed",
          top: this.elmGridCursor.top,
          left: this.elmGridCursor.left,
          width: this.elmGridCursor.width,
          height: this.elmGridCursor.height
        }
      : {}) as any;
  }

  @computed public get isActiveView() {
    return this.props.dataViewState!.activeView === IDataViewType.Table;
  }

  public render() {
    return (
      <div
        style={{
          width: "100%",
          height: "100%",
          display: !this.isActiveView ? "none" : undefined
        }}
      >
        <GridTable
          ref={this.refGridTable}
          gridCursorPos={this.gridCursorPos}
          gridDimensions={gridDimensions}
          fixedColumnSettings={{ fixedColumnCount: 2 }}
          showScroller={true}
          renderGridCursor={(gDim, gCursorPos, shouldRenderCellCursor) => {
            return (
              <GridCursor
                ref={shouldRenderCellCursor ? this.refGridCursor : undefined}
                gridDimensions={gDim}
                gridCursorPos={gCursorPos}
                showCellCursor={shouldRenderCellCursor}
                pollCellRect={this.props.dataCursorState!.isEditing}
                renderCellContent={(cursorRect: Rect) => null}
              />
            );
          }}
          editor={null}
          renderHeader={renderHeader}
          renderCell={this.renderCell}
          onKeyDown={this.gridTableEvents.handleGridKeyDown}
          onOutsideClick={this.gridTableEvents.handleOutsideClick}
        />
      </div>
    );
  }
}

