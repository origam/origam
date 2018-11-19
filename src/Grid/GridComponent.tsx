import * as React from "react";
import { observer } from "mobx-react";
import { IGridProps, IGridView, IColumnHeaderRenderer } from "./types";

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
        style={{ width, height }}
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

@observer
export class ColumnWidthHandle extends React.Component {
  public render() {
    return <div className="column-header-width-handle" />;
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
        style={{ display: "flex", flexDirection: "row", overflow: "hidden" }}
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
    }
    return alter(headers, (idx: number) => <ColumnWidthHandle key={idx} />);
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
    }
    return alter(headers, (idx: number) => <ColumnWidthHandle key={idx} />);
  }
}
