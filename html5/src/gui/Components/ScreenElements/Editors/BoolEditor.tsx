import * as React from "react";
import { observer } from "mobx-react";
import style from './BoolEditor.module.css';
import { action } from "mobx";


@observer
export class BoolEditor extends React.Component<{
  value: boolean;
  isReadOnly: boolean;
  onChange?(event: any, value: boolean): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
}> {
  
  render() {
    return (
      <div className={`editor-container ${style.checkbox}`}>
        <input
          className="editor"
          type="checkbox"
          checked={this.props.value}
          readOnly={this.props.isReadOnly}
          onChange={(event: any) => {
            this.props.onChange && !this.props.isReadOnly &&
              this.props.onChange(event, event.target.checked);
          }}
          onKeyDown={this.props.onKeyDown}
          onClick={this.props.onClick}
          onBlur={() => 'bool blur'}
        />
      </div>
    );
  }
}
