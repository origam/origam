import * as React from "react";
import { observer } from "mobx-react";

@observer
export class Window extends React.Component<any> {
  public render() {
    return (
      <div
        className="oui-window"
        style={{ display: !this.props.active ? "none" : undefined }}
      >
        {this.props.children}
      </div>
    );
  }
}
