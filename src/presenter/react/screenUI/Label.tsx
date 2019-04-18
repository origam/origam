import * as React from "react";
import { observer } from "mobx-react";
import { parseNumber } from "../../../utils/xml";

@observer
export class Label extends React.Component<{ Text: string; Height: string }> {
  render() {
    return (
      <div className="label" style={{ height: parseNumber(this.props.Height) }}>
        {this.props.Text}
      </div>
    );
  }
}
