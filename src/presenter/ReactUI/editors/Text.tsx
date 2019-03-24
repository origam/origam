import * as React from "react";

export class TextEditor extends React.Component<{
  value: string;
  isReadOnly: boolean;
  isInvalid: boolean;
  onChange?(event: any, value: string): void;
}> {
  render() {
    return (
      <div className="editor-container">
        <input
          className="editor"
          type="text"
          value={this.props.value}
          readOnly={this.props.isReadOnly}
          onChange={(event: any) =>
            this.props.onChange &&
            this.props.onChange(event, event.target.value)
          }
        />
        {this.props.isInvalid && (
          <div className="notification">
            <i className="fas fa-exclamation-circle red" />
          </div>
        )}
      </div>
    );
  }
}
