import {observer} from "mobx-react";
import * as React from "react";
import {IScrolleeProps} from "./types";
import S from "./Scrollee.module.css";

/*
  Component translating its content according to scrollOffsetSource.
*/

// TODO: Maybe add hideOverflow property to disable content clipping? (or allow some custom class?)
@observer
export default class Scrollee extends React.Component<IScrolleeProps> {
  public render() {
    return (
      <div
        className={S.scrollee}
        style={{
          width: this.props.width,
          height: this.props.height,
          zIndex: this.props.zIndex || 0,
        }}
      >
        <div
          className={S.scrolleeShifted}
          style={{
            top:
              (this.props.fixedVert ? 0 : -this.props.scrollOffsetSource.scrollTop) +
              (this.props.offsetTop || 0),
            left:
              (this.props.fixedHoriz ? 0 : -this.props.scrollOffsetSource.scrollLeft) +
              (this.props.offsetLeft || 0),
          }}
        >
          {this.props.children}
        </div>
      </div>
    );
  }
}
