import * as React from "react";
import { observer } from "mobx-react";
import { action } from "mobx";
import S from "./TextEditor.module.css";
import CS from "./CommonStyle.module.css";
import { Tooltip } from "react-tippy";

@observer
export class TextEditor extends React.Component<{
  value: string | null;
  isReadOnly: boolean;
  isInvalid: boolean;
  invalidMessage?: string;
  isFocused: boolean;
  backgroundColor?: string;
  foregroundColor?: string;
  refocuser?: (cb: () => void) => () => void;
  onChange?(event: any, value: string): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onEditorBlur?(event: any): void;
}> {
  disposers: any[] = [];

  componentDidMount() {
    this.props.refocuser &&
      this.disposers.push(this.props.refocuser(this.makeFocusedIfNeeded));
    this.makeFocusedIfNeeded();
  }

  componentWillUnmount() {
    this.disposers.forEach(d => d());
  }

  componentDidUpdate(prevProps: { isFocused: boolean }) {
    if (!prevProps.isFocused && this.props.isFocused) {
      this.makeFocusedIfNeeded();
    }
  }

  @action.bound
  makeFocusedIfNeeded() {
    if (this.props.isFocused) {
      console.log("--- MAKE FOCUSED ---");
      this.elmInput && this.elmInput.focus();
      setTimeout(() => {
        this.elmInput && this.elmInput.select();
      }, 10);
    }
  }

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };

  render() {
    return (
      <div className={CS.editorContainer}>
        <input
          style={{
            color: this.props.foregroundColor,
            backgroundColor: this.props.backgroundColor
          }}
          className={CS.editor}
          type="text"
          value={this.props.value || ""}
          readOnly={this.props.isReadOnly}
          ref={this.refInput}
          onChange={(event: any) =>
            this.props.onChange &&
            this.props.onChange(event, event.target.value)
          }
          onKeyDown={this.props.onKeyDown}
          onClick={this.props.onClick}
          onBlur={this.props.onEditorBlur}
        />
        {this.props.isInvalid && (
          <div className={CS.notification}>
            <Tooltip html={this.props.invalidMessage} arrow={true}>
              <i className="fas fa-exclamation-circle red" />
            </Tooltip>
          </div>
        )}
      </div>
    );
  }
}
