import * as React from "react";
import { observer } from "mobx-react";

@observer
export class VBox extends React.Component<{ height: number | undefined }> {
  getVBoxStyle() {
    if (this.props.height !== undefined) {
      return {
        flexShrink: 0,
        height: this.props.height
      };
    } else {
      return {
        flexGrow: 1
      };
    }
  }

  render() {
    return (
      <div className="vbox" style={this.getVBoxStyle()}>
        {this.props.children}
      </div>
    );
  }
}