import { action } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { Tooltip } from "react-tippy";
import S from "./TextEditor.module.scss";

@observer
export class TextEditor extends React.Component<{
  value: string | null;
  isMultiline?: boolean;
  isReadOnly: boolean;
  isPassword?: boolean;
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
    this.props.refocuser && this.disposers.push(this.props.refocuser(this.makeFocusedIfNeeded));
    this.makeFocusedIfNeeded();
  }

  componentWillUnmount() {
    this.disposers.forEach((d) => d());
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
        if (this.elmInput) {
          this.elmInput.select();
          this.elmInput.scrollLeft = 0;
        }
      }, 10);
    }
  }

  @action.bound
  handleFocus(event: any) {
    setTimeout(() => {
      if (this.elmInput) {
        this.elmInput.select();
        this.elmInput.scrollLeft = 0;
      }
    }, 10);
  }

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };

  render() {
    return (
      <div className={S.editorContainer}>
        {!this.props.isMultiline ? (
          <input
            style={{
              color: this.props.foregroundColor,
              backgroundColor: this.props.backgroundColor,
            }}
            className={S.input}
            type={this.props.isPassword ? "password" : "text"}
            autoComplete={this.props.isPassword ? "new-password" : undefined}
            value={this.props.value || ""}
            readOnly={this.props.isReadOnly}
            ref={this.refInput}
            onChange={(event: any) =>
              this.props.onChange && this.props.onChange(event, event.target.value)
            }
            onKeyDown={this.props.onKeyDown}
            onClick={this.props.onClick}
            onBlur={this.props.onEditorBlur}
            onFocus={this.handleFocus}
          />
        ) : (
          <textarea
            style={{
              color: this.props.foregroundColor,
              backgroundColor: this.props.backgroundColor,
            }}
            className={S.input}
            value={this.props.value || ""}
            readOnly={this.props.isReadOnly}
            ref={this.refInput}
            onChange={(event: any) =>
              this.props.onChange && this.props.onChange(event, event.target.value)
            }
            onKeyDown={this.props.onKeyDown}
            onClick={this.props.onClick}
            onBlur={this.props.onEditorBlur}
            onFocus={this.handleFocus}
          />
        )}
        {this.props.isInvalid && (
          <div className={S.notification}>
            <Tooltip html={this.props.invalidMessage} arrow={true}>
              <i className="fas fa-exclamation-circle red" />
            </Tooltip>
          </div>
        )}
      </div>
    );
  }
}
