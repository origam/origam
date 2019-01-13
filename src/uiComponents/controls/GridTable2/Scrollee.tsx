import { observer } from "mobx-react";
import * as React from "react";
import { IScrolleeProps } from "./types";

/*
  Component translating its content according to scrollOffsetSource.
*/

// TODO: Add hideOverflow property to disable content clipping (or allow some custom class?)
@observer
export default class Scrollee extends React.Component<IScrolleeProps> {
  public render() {
    return (
      <div
        style={{
          width: this.props.width,
          height: this.props.height,
          overflow: "hidden"
        }}
      >
        <div
          style={{
            position: "relative",
            top: this.props.fixedVert
              ? 0
              : -this.props.scrollOffsetSource.scrollTop,
            left: this.props.fixedHoriz
              ? 0
              : -this.props.scrollOffsetSource.scrollLeft
          }}
        >
          {this.props.children}
        </div>
      </div>
    );
  }
}