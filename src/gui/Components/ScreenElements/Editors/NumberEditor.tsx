/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { action, computed, observable } from "mobx";
import { observer } from "mobx-react";
import * as React from "react";
import S from "./NumberEditor.module.scss";
import cx from "classnames";
import {
  formatNumber,
  getCurrentDecimalSeparator,
} from "../../../../model/entities/NumberFormating";
import { IFocusable } from "../../../../model/entities/FormFocusManager";
import { IProperty } from "model/entities/types/IProperty";
@observer
export class NumberEditor extends React.Component<{
  value: string | null;
  isReadOnly: boolean;
  isPassword?: boolean;
  isInvalid: boolean;
  invalidMessage?: string;
  property?: IProperty;
  backgroundColor?: string;
  foregroundColor?: string;
  customNumberFormat?: string | undefined;
  maxLength?: number;
  customStyle?: any;
  onChange?(event: any, value: string | null): void;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onDoubleClick?(event: any): void;
  onEditorBlur?(event: any): void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onTextOverflowChanged?: (toolTip: string | null | undefined) => void;
}> {
  disposers: any[] = [];

  @observable hasFocus = false;
  @observable editingValue: null | string = "";
  @observable wasChanged = false;

  @computed get numeralFormattedValue() {
    if (this.props.value === null) {
      return "";
    }
    return formatNumber(
      this.props.customNumberFormat,
      this.props.property?.entity ?? "",
      Number(this.props.value)
    );
  }

  @computed get editValue() {
    if (this.hasFocus) {
      return this.editingValue;
    } else {
      return this.numeralFormattedValue;
    }
  }

  componentDidMount() {
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
    this.updateTextOverflowState();
  }

  componentWillUnmount() {
    this.disposers.forEach((d) => d());
  }

  componentDidUpdate(prevProps: { value: any }) {
    if (this.props.value !== prevProps.value && !this.wasChanged) {
      this.editingValue = this.numeralFormattedValue;
    }
    this.updateTextOverflowState();
  }
  @action.bound
  handleFocus(event: any) {
    this.hasFocus = true;
    this.wasChanged = false;
    this.editingValue = this.numeralFormattedValue;
    if (this.elmInput) {
      this.elmInput.select();
      this.elmInput.scrollLeft = 0;
    }
  }

  private updateTextOverflowState() {
    if(!this.elmInput){
      return;
    }
    const textOverflow = this.elmInput.offsetWidth < this.elmInput.scrollWidth
    this.props.onTextOverflowChanged?.(textOverflow ? this.numeralFormattedValue : undefined);
  }

  @action.bound
  handleBlur(event: any) {
    this.hasFocus = false;
    this.wasChanged = false;
    if (!this.wasChanged || this.props.value === this.editValue) {
      this.props.onEditorBlur?.(event);
      return;
    }
    if (this.editValue === "") {
      this.props.onEditorBlur?.(event);
    } else {
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
    const invalidChars = new RegExp("[^\\d\\-" + getCurrentDecimalSeparator() + "]", "g");
    this.editingValue = (event.target.value || "").replace(invalidChars, "");
    this.props.onChange && this.props.onChange(null, this.numericValue);
    this.updateTextOverflowState();
  }

  @action.bound handleKeyDown(event: any) {
    if (event.key === "Escape") {
      this.wasChanged = false;
    } else if (event.key === "Enter") {
      this.editingValue = this.numeralFormattedValue;
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
    const maxLength = (this.props.maxLength === 0 ? undefined : this.props.maxLength) || undefined;
    return (
      <div className={S.editorContainer}>
        <input
          style={this.getStyle()}
          title={this.props.customNumberFormat || undefined}
          className={cx(S.input, "isRightAligned")}
          type={this.props.isPassword ? "password" : "text"}
          autoComplete={this.props.isPassword ? "new-password" : undefined}
          value={
            this.editValue !== undefined && this.editValue !== "NaN" && this.editValue !== null
              ? this.editValue
              : ""
          }
          maxLength={maxLength}
          readOnly={this.props.isReadOnly}
          ref={this.refInput}
          onChange={this.handleChange}
          onKeyDown={this.handleKeyDown}
          onClick={this.props.onClick}
          onDoubleClick={this.props.onDoubleClick}
          onBlur={this.handleBlur}
          onFocus={this.handleFocus}
        />
        {this.props.isInvalid && (
          <div className={S.notification} title={this.props.invalidMessage}>
            <i className="fas fa-exclamation-circle red" />
          </div>
        )}
      </div>
    );
  }
}
