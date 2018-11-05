import * as React from "react";
import { observer } from "mobx-react";

@observer
export class GridForm extends React.Component<any> {
  public render() {
    return <div className="oui-form-root">{this.props.children}</div>;
  }
}
