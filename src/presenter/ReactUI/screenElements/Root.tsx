import * as React from "react";
import { observer } from "mobx-react";

@observer
export class Root extends React.Component {
  render() {
    return <div className="root">{this.props.children}</div>;
  }
}