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

import { action } from "mobx";
import * as React from "react";
import S from "./NumberEditor.module.scss";
import cx from "classnames";
import {
  formatNumber,
  getCurrentDecimalSeparator,
  getCurrentGroupSeparator,
} from "model/entities/NumberFormating";
import { IFocusable } from "model/entities/FormFocusManager";
import { IProperty } from "model/entities/types/IProperty";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";
import { isRefreshShortcut, isSaveShortcut } from "utils/keyShortcuts";
import { observer } from "mobx-react";
import { createNumberEditorModel } from "gui/Components/ScreenElements/Editors/NumberEditorModel";

export interface NumberEditorProps
{
  value: string | number | null;
  isReadOnly: boolean;
  isPassword?: boolean;
  property: IProperty;
  backgroundColor?: string;
  foregroundColor?: string;
  customNumberFormat?: string | undefined;
  maxLength?: number;
  customStyle?: any;
  onChange?(event: any, value: string | null): Promise<void>;
  onKeyDown?(event: any): void;
  onClick?(event: any): void;
  onDoubleClick?(event: any): void;
  onMount?(onChange?: (value: any) => void): void;
  onEditorBlur?(event: any): Promise<void>;
  subscribeToFocusManager?: (obj: IFocusable, onBlur: ()=> Promise<void>) => void;
  onTextOverflowChanged?: (tooltip: string | null | undefined) => void;
  id?: string
}

@observer
export class NumberEditor extends React.Component<NumberEditorProps, any> {
  model =  createNumberEditorModel({props: this.props, initValue: this.formatForDisplay(this.props.value)});

  formatForDisplay(value: string | number | null){
    if(value === null || value === ""){
      return ""
    }
    let rawValue = value;
    if(typeof value === "string"){
      rawValue = value
        .replaceAll(getCurrentGroupSeparator(), "")
        .replaceAll(getCurrentDecimalSeparator(), ".")
      if(value.trim() === "" || value.trim() === "-"){
        rawValue = "0";
      }
    }
    return formatNumber(
      this.props.customNumberFormat,
      this.props.property?.entity ?? "",
      Number(rawValue)
    );
  }

  formatForOnChange(value: string | number | null){
    if(value === null || value === ""){
      return null;
    }
    return this.formatForDisplay(value)
      .replaceAll(getCurrentGroupSeparator(), "")
      .replaceAll(getCurrentDecimalSeparator(), ".");
  }
  componentDidMount() {
    if (this.model.inputRef.current && this.props.subscribeToFocusManager) {
      this.props.subscribeToFocusManager(
        this.model.inputRef.current,
        async()=> await this.handleBlur(null));
    }
    this.model.updateTextOverflowState();
    this.props.onMount?.(value => {
      if (this.model.inputRef.current) {
        this.model.inputRef.current.value = value;
        const event = {target: this.model.inputRef.current};
        this.model.handleChange(event);
      }
    });
  }

  componentDidUpdate(prevProps: { value: any }) {
    if (this.props.value !== prevProps.value) {
      this.model.value = this.formatForDisplay(this.props.value)
    }
    this.model.updateTextOverflowState();
  }

  componentWillUnmount() {
    this.handleBlur({target: this.model.inputRef.current});
  }

  @action.bound
  handleFocus(event: any) {
    if (this.model.inputRef.current) {
      this.model.inputRef.current.select();
      this.model.inputRef.current.scrollLeft = 0;
    }
  }

  @action.bound
  async handleBlur(event: any) {
    await runInFlowWithHandler({
      ctx: this.props.property,
      action: async () => {
        await this.onChange();
        await this.props.onEditorBlur?.(event);
      }})
  }

  @action.bound
  async handleKeyDown(event: any) {
    await runInFlowWithHandler({
      ctx: this.props.property,
      action: async () => {
        if (
          event.key === "Enter" ||
          event.key === "Tab" ||
          isSaveShortcut(event) ||
          isRefreshShortcut(event)
        ){
          await this.onChange();
        }
        this.props.onKeyDown && this.props.onKeyDown(event);
      }})
  }

  private async onChange() {
    let value = this.formatForOnChange(this.model.value);
    if (this.formatForOnChange(this.props.value) !== value) {
      this.props.onChange && await this.props.onChange(null, value);
    }
  }

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
          id={this.props.id}
          style={this.getStyle()}
          title={this.props.customNumberFormat || undefined}
          className={cx(S.input)}
          type={this.model.getInputType()}
          inputMode={this.model.getInputMode()}
          autoComplete={"off"}
          value={this.model.value}
          maxLength={maxLength}
          readOnly={this.props.isReadOnly}
          ref={this.model.inputRef}
          onChange={this.model.handleChange}
          onKeyDown={this.handleKeyDown}
          onClick={this.props.onClick}
          onDoubleClick={this.props.onDoubleClick}
          onBlur={this.handleBlur}
          onFocus={this.handleFocus}
          onDragStart={(e: any) =>  e.preventDefault()}
        />
      </div>
    );
  }
}
