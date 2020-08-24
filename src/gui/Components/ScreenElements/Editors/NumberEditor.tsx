import {action, computed, observable} from "mobx";
import {observer} from "mobx-react";
import * as React from "react";
import {Tooltip} from "react-tippy";
import S from "./NumberEditor.module.scss";
import cx from "classnames";
import {
  formatNumber,
  getCurrentDecimalSeparator,
  getCurrentGroupSeparator
} from "../../../../model/entities/NumberFormating";
import {IFocusable} from "../../../../model/entities/FocusManager";

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
  customStyle?: any;
  refocuser?: (cb: () => void) => () => void;
  onChange?(event: any, value: string | null): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onEditorBlur?(event: any): void;
  subscribeToFocusManager?: (obj: IFocusable) => (()=>void);
  tabIndex?: number;
}> {
  disposers: any[] = [];

  @observable hasFocus = false;
  @observable editingValue: null | string = "";
  @observable wasChanged = false;
  unsubscribeFromFocusManager?: () => void;

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
    this.props.refocuser && this.disposers.push(this.props.refocuser(this.makeFocusedIfNeeded));
    this.makeFocusedIfNeeded();
    if(this.elmInput && this.props.subscribeToFocusManager){
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
    this.hasFocus = true;
    this.editingValue = this.numeralFormattedValue;
    setTimeout(() => {
      if (this.elmInput) {
        this.elmInput.select();
        this.elmInput.scrollLeft = 0;
      }
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
      const value =  ""+Number(this.editValue);
      this.hasFocus = false;
      this.props.onChange && this.props.onChange(null, value);
      this.props.onEditorBlur && this.props.onEditorBlur(event);
    }
  }

  @action.bound handleChange(event: any) {
    this.wasChanged = true;
    const invalidChars = new RegExp("[^\\d"+getCurrentDecimalSeparator()+getCurrentGroupSeparator()+"]", "g");
    this.editingValue = (event.target.value || "").replace(invalidChars, "");
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

  getStyle(){
    if(this.props.customStyle){
      return this.props.customStyle;
    }else{
      return {
        color: this.props.foregroundColor,
        backgroundColor: this.props.backgroundColor,
      }
    }
  }

  render() {
    return (
      <div className={S.editorContainer}>
        {!this.props.isMultiline ? (
          <input
            style={this.getStyle()}
            title={this.props.customNumberFormat || ""}
            className={cx(S.input, "isRightAligned")}
            type={this.props.isPassword ? "password" : "text"}
            autoComplete={this.props.isPassword ? "new-password" : undefined}
            value={this.editValue !== undefined && this.editValue !== null ? this.editValue : ""}
            readOnly={this.props.isReadOnly}
            ref={this.refInput}
            onChange={this.handleChange}
            onKeyDown={this.props.onKeyDown}
            onClick={this.props.onClick}
            onBlur={this.handleBlur}
            onFocus={this.handleFocus}
            tabIndex={this.props.tabIndex ? this.props.tabIndex : undefined}
          />
        ) : (
          <textarea
            style={this.getStyle()}
            className={S.input}
            value={this.props.value || ""}
            readOnly={this.props.isReadOnly}
            ref={this.refInput}
            onChange={this.handleChange}
            onKeyDown={this.props.onKeyDown}
            onClick={this.props.onClick}
            onBlur={this.handleBlur}
            onFocus={this.handleFocus}
            tabIndex={this.props.tabIndex ? this.props.tabIndex : undefined}
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
