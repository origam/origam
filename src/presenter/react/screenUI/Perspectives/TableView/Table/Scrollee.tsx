import { observer } from "mobx-react";
import * as React from "react";
import { IScrolleeProps } from "./types";

/*
  Component translating its content according to scrollOffsetSource.
*/

// TODO: Maybe add hideOverflow property to disable content clipping? (or allow some custom class?)
@observer
export default class Scrollee extends React.Component<IScrolleeProps> {
  public render() {
    return (
      <div
        className="scrollee"
        style={{
          width: this.props.width,
          height: this.props.height
        }}
      >
        <div
          className="scrollee-shifted"
          style={{
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
