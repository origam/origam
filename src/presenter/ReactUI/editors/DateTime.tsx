import * as React from "react";
import { observer } from "mobx-react";
import moment from 'moment';



@observer
export class DateTimeEditor extends React.Component<{
  value: string;
  inputFormat: string;
  outputFormat: string;
  isReadOnly: boolean;
  isInvalid: boolean;
}> {
  render() {
    return (
      <div className="editor-container">
        <input
          className="editor"
          type="text"
          value={moment(this.props.value, this.props.inputFormat).format(
            this.props.outputFormat
          )}
          readOnly={this.props.isReadOnly}
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
