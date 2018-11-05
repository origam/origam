import * as React from "react";
import { observer } from "mobx-react";

@observer
export class FormSection extends React.Component<any> {
  public render() {
    return (
      <>
        <div
          className="oui-panel"
          style={{
            top: this.props.y,
            left: this.props.x,
            width: this.props.w,
            height: this.props.h
          }}
        >
          {this.props.children}
        </div>
        <div
          className="oui-panel-label"
          style={{
            top: this.props.y + 5,
            left: this.props.x + 5
          }}
        >
          {this.props.name}
        </div>
      </>
    );
  }
}
