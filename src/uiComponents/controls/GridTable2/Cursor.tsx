import * as React from "react";
import { IGridDimensions, IGridCursorPos, IGridCursorProps } from "./types";
import { observer } from "mobx-react";

@observer
export default class GridCursor extends React.Component<IGridCursorProps> {
  public render() {
    const gcp = this.props.gridCursorPos;
    if(gcp.selectedColumnIndex === undefined || gcp.selectedRowIndex === undefined) {
      return null;
    }

    const gDim = this.props.gridDimensions;
    const width = gDim.getColumnWidth(gcp.selectedColumnIndex);
    const height = gDim.getRowHeight(gcp.selectedRowIndex);
    const left = gDim.getColumnLeft(gcp.selectedColumnIndex);
    const top = gDim.getRowTop(gcp.selectedRowIndex);

    return (
      <>
        <div
          style={{
            position: "absolute",
            width: Math.max(0, left),
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
        >{this.props.cellContent}</div>
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
