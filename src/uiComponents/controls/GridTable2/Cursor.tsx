import * as React from "react";
import { IGridDimensions, IGridCursorPos, IGridCursorProps } from "./types";
import { observer } from "mobx-react";

@observer
export default class GridCursor extends React.Component<IGridCursorProps> {
  public render() {
    const gcp = this.props.gridCursorPos;
    const gDim = this.props.gridDimensions;
    const width = gDim.getColumnWidth(gcp.selectedColumn);
    const height = gDim.getRowHeight(gcp.selectedRow);
    const left = gDim.getColumnLeft(gcp.selectedColumn);
    const top = gDim.getRowTop(gcp.selectedRow);
    return (
      <>
        <div
          style={{
            position: "absolute",
            width: left,
            height,
            left: 0,
            top,
            backgroundColor: "#00000022"
          }}
        />
        <div
          style={{
            position: "absolute",
            width,
            height,
            left,
            top,
            backgroundColor: "#00000055"
          }}
        />
        <div
          style={{
            position: "absolute",
            width: gDim.contentWidth - left + width,
            height,
            left: left + width,
            top,
            backgroundColor: "#00000022"
          }}
        />
      </>
    );
  }
}
