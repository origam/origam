import * as React from "react";
import { observer, inject } from "mobx-react";
import { IGridProps, IGridView, IColumnHeaderRenderer } from "./types";
import { IGridPanelBacking } from "../GridPanel/types";
import { action, observable } from "mobx";

function alter<T>(arr1: T[], alterItemFn: (idx: number) => T): T[] {
  const result: T[] = [];
  for (let i = 0; i < arr1.length; i++) {
    result.push(arr1[i]);
    if (i < arr1.length - 1) {
      result.push(alterItemFn(i));
    }
  }
  return result;
}

@observer
export class GridComponent extends React.Component<IGridProps> {
  public componentDidMount() {
    this.view.componentDidMount(this.props, this);
  }

  public componentDidUpdate(prevProps: IGridProps) {
    this.view.componentDidUpdate(prevProps, this.props, this);
  }

  public componentWillUnmount() {
    this.view.componentWillUnmount();
  }

  private get view() {
    return this.props.view;
  }

  private get canvasProps() {
    return this.view.canvasProps;
  }

  public render() {
    const {
      width,
      height,

      overlayElements
    } = this.props;

    const {
      handleGridScroll,
      handleGridKeyDown,
      handleGridClick,
      refRoot,
      refScroller,
      refCanvas
    } = this.view;

    const { contentWidth, contentHeight, isScrollingEnabled } = this.view;

    const { canvasProps } = this;
    // console.log(width, height)
    return (
      <div
        className="grid-view-container"
        style={{ flex: "1 1 auto" }}
        onKeyDown={handleGridKeyDown}
        // style={{display: this.props.isHidden ? "none" : undefined}}
      >
        <div
          className="grid-view-root"
          ref={refRoot}
          tabIndex={-1}
          onClick={handleGridClick}
        >
          <canvas {...canvasProps} ref={refCanvas} />
          <div
            className="grid-view-scroller"
            style={{
              width,
              height,
              overflow: !isScrollingEnabled ? "hidden" : undefined
            }}
            onScroll={handleGridScroll}
            ref={refScroller}
          >
            <div
              style={{
                width: contentWidth + 100,
                height: contentHeight + 100
              }}
            />
          </div>
          {overlayElements}
        </div>
        <div className="grid-view-editor-portal" /*ref={refEditorPortal}*/ />
      </div>
    );
  }
}

@inject("gridPaneBacking")
@observer
export class ColumnWidthHandle extends React.Component<any> {
  @observable private isMoving = false;
  private startMousePosition: number;
  private startLeftColumnWidth: number;
  private startRightColumnWidth: number;

  @action.bound
  private handleMouseDown(event: any) {
    const { gridInteractionSelectors } = this.props
      .gridPaneBacking as IGridPanelBacking;
    this.isMoving = true;
    console.log("MD");
    this.startMousePosition = event.screenX;
    this.startLeftColumnWidth = gridInteractionSelectors.getColumnWidth(
      this.props.leftColumnId
    );
    this.startRightColumnWidth = gridInteractionSelectors.getColumnWidth(
      this.props.rightColumnId
    );
    window.addEventListener("mousemove", this.handleWindowMouseMove);
    window.addEventListener("mouseup", this.handleWindowMouseUp);
  }

  @action.bound
  private handleWindowMouseUp(event: any) {
    this.isMoving = false;
    window.removeEventListener("mousemove", this.handleWindowMouseMove);
    window.removeEventListener("mouseup", this.handleWindowMouseUp);
  }

  @action.bound
  private handleWindowMouseMove(event: any) {
    if (this.isMoving) {
      const { gridInteractionActions } = this.props
        .gridPaneBacking as IGridPanelBacking;
      if (this.props.leftColumnId) {
        gridInteractionActions.setColumnWidth(
          this.props.leftColumnId,
          this.startLeftColumnWidth + event.screenX - this.startMousePosition
        );
      }
      /*if (this.props.rightColumnId) {
        gridInteractionActions.setColumnWidth(
          this.props.rightColumnId,
          this.startRightColumnWidth - event.screenX + this.startMousePosition
        );
      }*/
    }
  }

  public render() {
    const { gridInteractionActions } = this.props
      .gridPaneBacking as IGridPanelBacking;
    return (
      <div
        className="column-header-width-handle"
        onMouseDown={this.handleMouseDown}
      />
    );
  }
}

@observer
export class ColumnHeaders extends React.Component<{
  view: IGridView;
  columnHeaderRenderer: IColumnHeaderRenderer;
}> {
  public render() {
    const { view, columnHeaderRenderer } = this.props;
    return (
      <div
        style={{ display: "flex", flexDirection: "row", overflow: "hidden", userSelect: "none" }}
      >
        <FixedHeaders view={view} columnHeaderRenderer={columnHeaderRenderer} />
        <div
          style={{
            display: "flex",
            flexDirection: "row",
            width: view.movingColumnsTotalWidth,
            position: "relative",
            left: view.columnHeadersOffsetLeft
          }}
        >
          <MovingHeaders
            view={view}
            columnHeaderRenderer={columnHeaderRenderer}
          />
        </div>
      </div>
    );
  }
}

@observer
export class FixedHeaders extends React.Component<{
  view: IGridView;
  columnHeaderRenderer: IColumnHeaderRenderer;
}> {
  public render() {
    const { view } = this.props;
    const headers = [];
    for (let i = 0; i < view.fixedColumnCount; i++) {
      headers.push(
        <div
          key={view.getColumnId(i)}
          style={{
            minWidth: view.getColumnRight(i) - view.getColumnLeft(i),
            maxWidth: view.getColumnRight(i) - view.getColumnLeft(i),
            zIndex: 1000,
            boxSizing: "border-box"
          }}
          className="table-column-header-container"
        >
          {this.props.columnHeaderRenderer({ columnIndex: i })}
        </div>
      );
      headers.push(
        <ColumnWidthHandle
          leftColumnId={view.getColumnId(i)}
          rightColumnId={view.getColumnId(i + 1)}
          key={i}
        />
      );
    }
    return headers;
  }
}

@observer
export class MovingHeaders extends React.Component<{
  view: IGridView;
  columnHeaderRenderer: IColumnHeaderRenderer;
}> {
  public render() {
    const { view } = this.props;
    const headers = [];
    const columnCount = view.columnCount;
    for (let i = view.fixedColumnCount; i < columnCount; i++) {
      headers.push(
        <div
          key={view.getColumnId(i)}
          style={{
            minWidth: view.getColumnRight(i) - view.getColumnLeft(i),
            maxWidth: view.getColumnRight(i) - view.getColumnLeft(i)
          }}
          className="table-column-header-container"
        >
          {this.props.columnHeaderRenderer({ columnIndex: i })}
        </div>
      );
      headers.push(
        <ColumnWidthHandle
          leftColumnId={view.getColumnId(i)}
          rightColumnId={view.getColumnId(i + 1)}
          key={i}
        />
      );
    }
    return headers;
  }
}
