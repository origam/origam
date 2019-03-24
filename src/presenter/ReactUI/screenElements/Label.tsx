import * as React from "react";
import { observer } from "mobx-react";

@observer
export class Label extends React.Component<{ text: string; height: number | undefined }> {
  render() {
    return (
      <div className="label" style={{ height: this.props.height }}>
        {this.props.text}
      </div>
    );
  }
}