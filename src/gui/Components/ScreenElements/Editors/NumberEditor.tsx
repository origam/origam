import { action, computed, observable } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import { Tooltip } from "react-tippy";
import S from "./NumberEditor.module.scss";
import cx from "classnames";
import {
  formatNumber,
  getCurrentDecimalSeparator,
} from "../../../../model/entities/NumberFormating";
import { IFocusable } from "../../../../model/entities/FocusManager";
import { getLocaleFromCookie } from "../../../../utils/cookies";
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
  maxLength?: number;
  customStyle?: any;
  reFocuser?: (cb: () => void) => () => void;
  onChange?(event: any, value: string | null): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onDoubleClick?(event: any): void;
  onEditorBlur?(event: any): void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
}> {
  disposers: any[] = [];

  @observable hasFocus = false;
  @observable editingValue: null | string = "";
  @observable wasChanged = false;

  @computed get numeralFormattedValue() {
    if (this.props.value === null) {
      return "";
    }
    return formatNumber(this.props.customNumberFormat, "", Number(this.props.value));
  }

  @computed get editValue() {
    if (this.hasFocus) {
      return this.editingValue;
    } else {
      return this.numeralFormattedValue;
    }
  }

  componentDidMount() {
    this.props.reFocuser && this.disposers.push(this.props.reFocuser(this.makeFocusedIfNeeded));
    this.makeFocusedIfNeeded();
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
  }

  componentWillUnmount() {
    this.disposers.forEach((d) => d());
  }

  componentDidUpdate(prevProps: { isFocused: boolean, value: any }) {
    if (!prevProps.isFocused && this.props.isFocused) {
      this.makeFocusedIfNeeded();
    }
    if(this.props.value !== prevProps.value) {
      this.wasChanged = false;
    }
    if(!this.props.isFocused && this.elmInput !== document.activeElement){
      this.editingValue = this.numeralFormattedValue;
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
    this.hasFocus = true;
    this.editingValue = this.numeralFormattedValue;
    if (this.elmInput) {
      this.elmInput.select();
      this.elmInput.scrollLeft = 0;
    }
  }

  @action.bound
  handleBlur(event: any) {
    if (!this.wasChanged || this.props.value === this.editValue) {
      this.props.onEditorBlur?.(event);
      return;
    }
    if (this.editValue === "") {
      this.props.onEditorBlur?.(event);
    } else {
      this.hasFocus = false;
      this.props.onEditorBlur?.(event);
    }
  }

  @computed
  private get numericValue() {
    if (this.editValue === null || this.editValue === "") {
      return null;
    }
    let valueToParse = this.editValue.endsWith(getCurrentDecimalSeparator())
      ? this.editValue + "0"
      : this.editValue;
    valueToParse = valueToParse.replace(getCurrentDecimalSeparator(), ".");
    return "" + Number(valueToParse);
  }

  @action.bound handleChange(event: any) {
    this.wasChanged = true;
    const invalidChars = new RegExp("[^\\d" + getCurrentDecimalSeparator() + "]", "g");
    this.editingValue = (event.target.value || "").replace(invalidChars, "");
    this.props.onChange && this.props.onChange(null, this.numericValue);
  }

  @action.bound handleKeyDown(event: any) {
    if (event.key === "Escape") {
      this.wasChanged = false;
    }
    this.props.onKeyDown && this.props.onKeyDown(event);
  }

  elmInput: HTMLInputElement | HTMLTextAreaElement | null = null;
  refInput = (elm: HTMLInputElement | HTMLTextAreaElement | null) => {
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
    const maxLength = this.props.maxLength === 0
      ? undefined
      : this.props.maxLength;
    return (
      <div className={S.editorContainer}>
        {!this.props.isMultiline ? (
          <input
            style={this.getStyle()}
            title={this.props.customNumberFormat || ""}
            className={cx(S.input, "isRightAligned")}
            type={this.props.isPassword ? "password" : "text"}
            autoComplete={this.props.isPassword ? "new-password" : undefined}
            value={this.editValue !== undefined && this.editValue !== "NaN" && this.editValue !== null
              ? this.editValue
              : ""}
            maxLength={maxLength}
            readOnly={this.props.isReadOnly}
            ref={this.refInput}
            onChange={this.handleChange}
            onKeyDown={this.props.onKeyDown}
            onClick={this.props.onClick}
            onDoubleClick={this.props.onDoubleClick}
            onBlur={this.handleBlur}
            onFocus={this.handleFocus}
          />
        ) : (
          <textarea
            style={this.getStyle()}
            className={S.input}
            value={this.props.value || ""}
            maxLength={maxLength}
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
