import { action, observable, computed } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { Tooltip } from "react-tippy";
import S from "./NumberEditor.module.scss";
import numeral from "numeral";

@observer
export class NumberEditor extends React.Component<{
  value: string | null;
  isMultiline?: boolean;
  isReadOnly: boolean;
  isPassword?: boolean;
  isInvalid: boolean;
  invalidMessage?: string;
  isFocused: boolean;
  backgroundColor?: string;
  foregroundColor?: string;
  customNumberFormat?: string | undefined;
  refocuser?: (cb: () => void) => () => void;
  onChange?(event: any, value: string | null): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onEditorBlur?(event: any): void;
}> {
  disposers: any[] = [];

  @observable hasFocus = false;
  @observable editingValue: null | string = "";
  @observable wasChanged = false;

  get numeralFormat() {
    return (
      (this.props.customNumberFormat
        ? this.props.customNumberFormat.replace("#", "0")
        : "") || "0"
    );
  }

  @computed get numeralFormattedValue() {
    if (this.props.value === null) {
      return "";
    }
    return numeral(this.props.value).format(this.numeralFormat);
  }

  @computed get editValue() {
    if (this.hasFocus) {
      return this.editingValue;
    } else {
      return this.numeralFormattedValue;
    }
  }

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

  @action.bound
  handleFocus(event: any) {
    this.hasFocus = true;
    this.editingValue = this.numeralFormattedValue;
    setTimeout(() => {
      this.elmInput && this.elmInput.select();
    }, 10);
  }

  @action.bound
  handleBlur(event: any) {
    console.log(this.props.value, this.editValue);
    if (!this.wasChanged || this.props.value === this.editValue) {
      this.props.onEditorBlur && this.props.onEditorBlur(event);
      return;
    }
    if (this.editValue === "") {
      this.props.onChange && this.props.onChange(null, null);
      this.props.onEditorBlur && this.props.onEditorBlur(event);
    } else {
      const value = numeral(this.editValue).format(this.numeralFormat);
      this.hasFocus = false;
      this.props.onChange && this.props.onChange(null, value);
      this.props.onEditorBlur && this.props.onEditorBlur(event);
    }
  }

  @action.bound handleChange(event: any) {
    this.wasChanged = true;
    this.editingValue = (event.target.value || "").replace(/[^\d.\-,'\s]/g, "");
    // this.props.onChange && this.props.onChange(event, event.target.value);
  }

  @action.bound handleKeyDown(event: any) {
    if (event.key === "Escape") {
      this.wasChanged = false;
    }
    this.props.onKeyDown && this.props.onKeyDown(event);
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
              backgroundColor: this.props.backgroundColor
            }}
            title={this.props.customNumberFormat || ""}
            className={S.input}
            type={this.props.isPassword ? "password" : "text"}
            autoComplete={this.props.isPassword ? "new-password" : undefined}
            value={
              this.editValue !== undefined && this.editValue !== null
                ? this.editValue
                : ""
            }
            readOnly={this.props.isReadOnly}
            ref={this.refInput}
            onChange={this.handleChange}
            onKeyDown={this.props.onKeyDown}
            onClick={this.props.onClick}
            onBlur={this.handleBlur}
            onFocus={this.handleFocus}
          />
        ) : (
          <textarea
            style={{
              color: this.props.foregroundColor,
              backgroundColor: this.props.backgroundColor
            }}
            className={S.input}
            value={this.props.value || ""}
            readOnly={this.props.isReadOnly}
            ref={this.refInput}
            onChange={this.handleChange}
            onKeyDown={this.props.onKeyDown}
            onClick={this.props.onClick}
            onBlur={this.handleBlur}
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
