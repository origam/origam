import { action } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { Tooltip } from "react-tippy";
import S from "./TextEditor.module.scss";
import { IFocusable } from "../../../../model/entities/FocusManager";

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
  isRichText: boolean;
  customStyle?: any;
  subscribeToFocusManager?: (obj: IFocusable) => () => void;
  refocuser?: (cb: () => void) => () => void;
  onChange?(event: any, value: string): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onDoubleClick?(event: any): void;
  onEditorBlur?(event: any): void;
  tabIndex?: number;
}> {
  disposers: any[] = [];
  unsubscribeFromFocusManager?: () => void;

  componentDidMount() {
    this.props.refocuser && this.disposers.push(this.props.refocuser(this.makeFocusedIfNeeded));
    this.makeFocusedIfNeeded();
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.unsubscribeFromFocusManager = this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  componentWillUnmount() {
    this.disposers.forEach((d) => d());
    this.unsubscribeFromFocusManager && this.unsubscribeFromFocusManager();
  }

  componentDidUpdate(prevProps: { isFocused: boolean }) {
    if (!prevProps.isFocused && this.props.isFocused) {
      this.makeFocusedIfNeeded();
    }
  }

  @action.bound
  makeFocusedIfNeeded() {
    if (this.props.isFocused && this.elmInput) {
      this.elmInput.select();
      this.elmInput.scrollLeft = 0;
    }
  }

  @action.bound
  handleFocus(event: any) {
    if (this.elmInput) {
      this.elmInput.select();
      this.elmInput.scrollLeft = 0;
    }
  }

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | any) => {
    this.elmInput = elm;
  };

  getStyle() {
    if (this.props.customStyle) {
      return this.props.customStyle;
    } else {
      return {
        color: this.props.foregroundColor,
        backgroundColor: this.props.backgroundColor,
      };
    }
  }

  render() {
    return (
      <div className={S.editorContainer}>
        {this.renderValueTag()}
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

  private renderValueTag() {
    if (this.props.isRichText) {
      return (
        <div className={S.editorContainer}>
          <div
            style={this.getStyle()}
            className={S.input}
            dangerouslySetInnerHTML={{ __html: this.props.value ?? "" }}
            onKeyDown={this.props.onKeyDown}
            onClick={this.props.onClick}
            onDoubleClick={this.props.onDoubleClick}
            onBlur={this.props.onEditorBlur}
            onFocus={this.handleFocus}
            tabIndex={this.props.tabIndex ? this.props.tabIndex : undefined}
          />
        </div>
      );
    }
    if (!this.props.isMultiline) {
      return (
        <input
          style={this.getStyle()}
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
          onDoubleClick={this.props.onDoubleClick}
          onBlur={this.props.onEditorBlur}
          onFocus={this.handleFocus}
          tabIndex={this.props.tabIndex ? this.props.tabIndex : undefined}
        />
      );
    }
    if(this.props.isReadOnly){
      return (
        <div className={S.input}
             onClick={this.props.onClick}
             onDoubleClick={this.props.onDoubleClick}
             onBlur={this.props.onEditorBlur}
             onFocus={this.handleFocus}>
          <span style={this.getStyle()}>{this.props.value || ""}</span>
        </div>
      );
    }else{
      return (
        <textarea
          style={this.getStyle()}
          className={S.input}
          value={this.props.value || ""}
          readOnly={this.props.isReadOnly}
          ref={this.refInput}
          onChange={(event: any) =>
            this.props.onChange && this.props.onChange(event, event.target.value)
          }
          onKeyDown={this.props.onKeyDown}
          onDoubleClick={this.props.onDoubleClick}
          onClick={this.props.onClick}
          onBlur={this.props.onEditorBlur}
          onFocus={this.handleFocus}
          tabIndex={this.props.tabIndex ? this.props.tabIndex : undefined}
        />
      );
    }
  }
}
