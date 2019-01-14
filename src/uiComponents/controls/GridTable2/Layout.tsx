import * as React from "react";
import { IGridTableLayoutProps } from "./types";
import Measure from "react-measure";
import { observer, Observer } from "mobx-react";

@observer
export default class GridLayout extends React.Component<IGridTableLayoutProps> {
  public render() {
    return (
      <div
        style={{
          width: this.props.width,
          height: this.props.height,
          display: "flex",
          overflow: "hidden"
        }}
      >
        <div style={{ flexGrow: 1, display: "flex", flexDirection: "column" }}>
          <div style={{ display: "flex", flexDirection: "row" }}>
            <div
              style={{
                width: this.props.fixedColumnsTotalWidth
              }}
            >
              {this.props.fixedColumnsHeaders}
            </div>
            <div style={{ flexGrow: 1 }}>{this.props.movingColumnsHeaders}</div>
          </div>

          <div
            style={{
              flexGrow: 1,
              display: "flex",
              flexDirection: "row",
              position: "relative"
            }}
          >
            <div
              style={{
                position: "absolute",
                top: 0,
                left: 0,
                width: "100%",
                height: "100%",
                display: "flex",
                flexDirection: "row"
              }}
            >
              <div
                style={{
                  width: this.props.fixedColumnsTotalWidth
                }}
              >
                {this.props.fixedColumnsCanvas}
              </div>
              <div style={{ flexGrow: 1 }}>
                {this.props.movingColumnsCanvas}
              </div>
            </div>
            <div
              style={{
                position: "absolute",
                top: 0,
                left: 0,
                width: "100%",
                height: "100%",
                display: "flex",
                flexDirection: "row"
              }}
            >
              <div
                style={{
                  width: this.props.fixedColumnsTotalWidth
                }}
              >
                {this.props.fixedColumnsCursor}
              </div>
              <div style={{ flexGrow: 1 }}>
                {this.props.movingColumnsCursor}
              </div>
            </div>
            <div
              style={{
                position: "absolute",
                top: 0,
                left: 0,
                width: "100%",
                height: "100%"
              }}
            >
              {this.props.scroller}
            </div>
          </div>
        </div>
      </div>
    );
  }
}
