import * as React from "react";
import { observer } from "mobx-react";


@observer
export class BoolEditor extends React.Component<{
  value: boolean;
  isReadOnly: boolean;
  onChange?(event: any, value: boolean): void;
}> {
  render() {
    return (
      <div className="editor-container">
        <input
          className="editor"
          type="checkbox"
          checked={this.props.value}
          readOnly={this.props.isReadOnly}
          onChange={(event: any) => {
            this.props.onChange &&
              this.props.onChange(event, event.target.checked);
          }}
        />
      </div>
    );
  }
}
