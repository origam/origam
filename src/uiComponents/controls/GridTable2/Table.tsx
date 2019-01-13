import * as React from "react";
import { observable, action } from "mobx";
import { AutoSizer } from "react-virtualized";
import GridLayout from "./Layout";
import { IGridDimensions, IGridTableProps, IRenderHeader } from "./types";
import Scrollee from "./Scrollee";
import GridCanvas from "./Canvas";
import Scroller from "./Scroller";
import { observer, Observer } from "mobx-react";

const Headers = observer(
  ({
    gridDimensions,
    renderHeader
  }: {
    gridDimensions: IGridDimensions;
    renderHeader: IRenderHeader;
  }) => {
    const headers: React.ReactNode[] = [];
    for (let i = 0; i < gridDimensions.columnCount; i++) {
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
  @observable public scrollTop: number = 0;
  @observable public scrollLeft: number = 0;

  @action.bound public setScrollOffset(
    scrollTop: number,
    scrollLeft: number
  ): void {
    this.scrollTop = scrollTop;
    this.scrollLeft = scrollLeft;
  }

  public render() {
    const fixGDim = this.props.gridDimensionsFixed;
    const movGDim = this.props.gridDimensionsMoving;
    const fixedColumnsTotalWidth =
      fixGDim.columnCount > 0
        ? fixGDim.getColumnRight(fixGDim.columnCount - 1) -
          fixGDim.getColumnLeft(0)
        : 0;
    const movingColumnsTotalWidth =
      movGDim.getColumnRight(movGDim.columnCount - 1) -
      movGDim.getColumnLeft(0);
    const rowsTotalHeight =
      movGDim.getRowBottom(movGDim.rowCount - 1) - movGDim.getRowTop(0);
    return (
      <div style={{ width: "100%", height: "100%" }}>
        <AutoSizer>
          {({ width: tableWidth, height: tableHeight }) => (
            <Observer>
              {() => (
                <GridLayout
                  width={tableWidth}
                  height={tableHeight}
                  fixedColumnsTotalWidth={fixedColumnsTotalWidth}
                  fixedColumnsHeaders={
                    <Scrollee
                      width={fixedColumnsTotalWidth}
                      height={undefined}
                      fixedHoriz={true}
                      fixedVert={true}
                      scrollOffsetSource={this}
                    >
                      <Headers
                        gridDimensions={this.props.gridDimensionsFixed}
                        renderHeader={this.props.renderHeaderFixed}
                      />
                    </Scrollee>
                  }
                  movingColumnsHeaders={
                    <Scrollee
                      width={tableWidth - fixedColumnsTotalWidth}
                      height={undefined}
                      fixedVert={true}
                      scrollOffsetSource={this}
                    >
                      <Headers
                        gridDimensions={this.props.gridDimensionsMoving}
                        renderHeader={this.props.renderHeaderMoving}
                      />
                    </Scrollee>
                  }
                  fixedColumnsCanvas={
                    <AutoSizer>
                      {({ width, height }) => (
                        <Observer>
                          {() => (
                            <GridCanvas
                              renderCell={this.props.renderCellFixed}
                              width={width}
                              height={height}
                              gridDimensions={this.props.gridDimensionsFixed}
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
                              renderCell={this.props.renderCellMoving}
                              width={width}
                              height={height}
                              gridDimensions={this.props.gridDimensionsMoving}
                              scrollOffsetSource={this}
                            />
                          )}
                        </Observer>
                      )}
                    </AutoSizer>
                  }
                  scroller={
                    <Scroller
                      width={"100%"}
                      height={"100%"}
                      contentWidth={movingColumnsTotalWidth}
                      contentHeight={rowsTotalHeight}
                      scrollOffsetTarget={this}
                    />
                  }
                />
              )}
            </Observer>
          )}
        </AutoSizer>
      </div>
    );
  }
}
