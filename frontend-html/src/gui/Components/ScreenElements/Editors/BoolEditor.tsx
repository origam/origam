import * as React from "react";
import { observer } from "mobx-react";
import S from "./BoolEditor.module.scss";
import cx from "classnames";
import { IFocusAble } from "../../../../model/entities/FocusManager";
import CS from "gui/Components/ScreenElements/Editors/CommonStyle.module.css";

@observer
export class BoolEditor extends React.Component<{
  value: boolean;
  isReadOnly: boolean;
  readOnlyNoGrey?: boolean;
  onChange?(event: any, value: boolean): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onBlur?: () => void;
  onFocus?: () => void;
  isInvalid: boolean;
  invalidMessage?: string;
  id?: string;
  forceTakeFocus?: boolean;
  subscribeToFocusManager?: (obj: IFocusAble) => void;
}> {
  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };

  componentDidMount() {
    if (this.props.forceTakeFocus) {
      this.elmInput?.focus();
    }
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  render() {
    return (
      <div className={cx(S.editorContainer)}>
        <input
          id={this.props.id ? this.props.id : undefined}
          className="editor"
          type="checkbox"
          checked={this.props.value}
          readOnly={!this.props.readOnlyNoGrey && this.props.isReadOnly}
          disabled={!this.props.readOnlyNoGrey && this.props.isReadOnly}
          onChange={(event: any) => {
            this.props.onChange &&
              !this.props.isReadOnly &&
              this.props.onChange(event, event.target.checked);
          }}
          onKeyDown={this.props.onKeyDown}
          onClick={this.props.onClick}
          onBlur={this.props.onBlur}
          onFocus={this.props.onFocus}
          ref={this.refInput}
          tabIndex={0}
        />
        {this.props.isInvalid && (
          <div className={CS.notification} title={this.props.invalidMessage}>
            <i className="fas fa-exclamation-circle red" />
          </div>
        )}
      </div>
    );
  }
}
