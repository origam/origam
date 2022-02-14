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
  getCurrentGroupSeparator,
} from "../../../../model/entities/NumberFormating";
import { IFocusable } from "../../../../model/entities/FormFocusManager";
import { IProperty } from "model/entities/types/IProperty";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";

@observer
export class NumberEditor extends React.Component<{
  value: string | number | null;
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
  onChange?(event: any, value: string | null): Promise<void>;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onDoubleClick?(event: any): void;
  onEditorBlur?(event: any): void;
  subscribeToFocusManager?: (obj: IFocusable) => void;
  onTextOverflowChanged?: (toolTip: string | null | undefined) => void;
}> {

  @observable value: string = this.formatForDisplay(this.props.value);

  formatForDisplay(value: string | number | null){
    if (value === null) {
        return "";
    }
    const rawValue = typeof value === "string"
      ? value.replaceAll(getCurrentGroupSeparator(), "")
      : value;
    return formatNumber(
      this.props.customNumberFormat,
      this.props.property?.entity ?? "",
      Number(rawValue)
    );
  }

  formatForOnChange(value: string | number | null){
    const displayValue = this.formatForDisplay(value);
    return displayValue.replaceAll(getCurrentGroupSeparator(), "");
  }

  componentDidMount() {
    if (this.elmInput && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(this.elmInput);
    }
    this.updateTextOverflowState();
  }

  componentDidUpdate(prevProps: { value: any }) {
    if (this.props.value !== prevProps.value) {
      this.value = this.formatForDisplay(this.props.value)
    }
    this.updateTextOverflowState();
  }

  componentWillUnmount() {
    this.handleBlur(null);
  }

  @action.bound
  handleFocus(event: any) {
    if (this.elmInput) {
      this.elmInput.select();
      this.elmInput.scrollLeft = 0;
    }
  }

  updateTextOverflowState() {
    if (!this.elmInput) {
      return;
    }
    const textOverflow = this.elmInput.offsetWidth < this.elmInput.scrollWidth
    this.props.onTextOverflowChanged?.(textOverflow ? this.value : undefined);
  }


  @action.bound
  async handleBlur(event: any) {
    await runInFlowWithHandler({
      ctx: this.props.property,
      action: async () => {
        let value = this.formatForOnChange(this.value);
        if(this.formatForOnChange(this.props.value) !== value){
          this.props.onChange && await this.props.onChange(null, value);
        }
        this.props.onEditorBlur?.(event);
        this.value = this.formatForDisplay(this.value);
      }})
  }


  isValidNumber(value: string){
    let formattedValue = value.replaceAll(getCurrentGroupSeparator(), "");
    return !isNaN(Number(formattedValue))
  }

  @action.bound handleChange(event: any) {
    const invalidChars = new RegExp("[^\\d\\-" + getCurrentDecimalSeparator() + getCurrentGroupSeparator() + "]", "g");
    const cleanValue = (event.target.value || "").replaceAll(invalidChars, "");
    if(this.isValidNumber(cleanValue)){
      this.value = cleanValue;
      this.updateTextOverflowState();
    }
  }

  isValidCharacter(char: string){
    if(
      char === getCurrentDecimalSeparator() ||
      char === getCurrentGroupSeparator()
    ) {
      return true;
    }
    return !isNaN(parseInt(char, 10))
  }

  @action.bound handleKeyDown(event: any) {
    this.props.onKeyDown && this.props.onKeyDown(event);
  }

  elmInput: HTMLInputElement | null = null;
  refInput = (elm: HTMLInputElement | null) => {
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
          className={cx(S.input)}
          type={this.props.isPassword ? "password" : "text"}
          autoComplete={this.props.isPassword ? "new-password" : undefined}
          value={this.value}
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
            <i className="fas fa-exclamation-circle red"/>
          </div>
        )}
      </div>
    );
  }
}
