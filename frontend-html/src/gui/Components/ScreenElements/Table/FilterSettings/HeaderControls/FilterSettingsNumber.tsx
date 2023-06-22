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

import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.scss";
import { action, observable, runInAction } from "mobx";
import { observer } from "mobx-react";
import { EDITOR_DALEY_MS, FilterSetting } from "./FilterSetting";
import { Operator } from "./Operator";
import { getCurrentDecimalSeparator } from "model/entities/NumberFormating";
import { ClearableInput } from "gui/connections/MobileComponents/Grid/ClearableInput";
import { requestFocus } from "utils/focus";

const OPERATORS =
  [
    Operator.equals,
    Operator.notEquals,
    Operator.lessThanOrEquals,
    Operator.greaterThanOrEquals,
    Operator.lessThan,
    Operator.greaterThan,
    Operator.between,
    Operator.notBetween,
    Operator.isNull,
    Operator.isNotNull
  ];

const OpCombo: React.FC<{
  setting: any;
  onChange: () => void;
  id: string;
}> = observer((props) => {
  return (
    <FilterSettingsComboBox
      id={props.id}
      trigger={<>{(OPERATORS.find((item) => item.type === props.setting.type) || {}).caption}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() => {
            props.setting.type = op.type;
            props.onChange();
          }
          }
        >
          {op.caption}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
});

@observer
class OpEditors extends React.Component<{
  setting?: any;
  onBlur?: (event: any) => void;
  allowDecimalSeparator: boolean;
  currentValue1: any;
  currentValue2: any;
  onCurrentValue1Changed: ((value1: any) => void);
  onCurrentValue2Changed: ((value2: any) => void);
  autoFocus: boolean;
  id: string;
}> {

  inputRef = React.createRef<HTMLInputElement>();

  componentDidMount() {
    if (this.props.autoFocus) {
      setTimeout(() => {
        requestFocus(this.inputRef.current);
      });
    }
  }

  onChange(event: any){
    const value = this.removeInvalidCharacters(event);
    this.props.onCurrentValue1Changed(value)
  }

  private removeInvalidCharacters(event: any) {
    let separator = getCurrentDecimalSeparator();
    const invalidChars = new RegExp("[^\\d\\-" + (this.props.allowDecimalSeparator ? separator : "") + "]", "g");
    let clearedValue = (event.target.value || "").replace(invalidChars, "");
    let firstSeparatorIndex = clearedValue.indexOf(separator);
    if(firstSeparatorIndex > -1){
      let lastSeparatorIndex = clearedValue.lastIndexOf(separator);
      for (let i = 0; i < 1000; i++) {
        if (firstSeparatorIndex === lastSeparatorIndex) break;
        clearedValue = clearedValue.slice(0, lastSeparatorIndex) + clearedValue.slice(lastSeparatorIndex + 1);
      }
    }
    if(clearedValue === separator){
      clearedValue = "0" + separator
    }
    return isNaN(parseFloat(clearedValue)) ? "" : clearedValue;
  }

  render() {
    switch (this.props.setting.type) {
      case "eq":
      case "neq":
      case "lt":
      case "gt":
      case "lte":
      case "gte":
        return (
          <ClearableInput
            id={this.props.id}
            className={CS.input}
            value={this.props.currentValue1 ?? ""}
            onChange={(event: any) => {
              const value = this.removeInvalidCharacters(event);
              this.props.onCurrentValue1Changed(value);
            }}
            onBlur={this.props.onBlur}
            ref={this.inputRef}
          />
        );

      case "between":
      case "nbetween":
        return (
          <>
            <ClearableInput
              id={"from_" + this.props.id}
              className={CS.input}
              value={this.props.currentValue1 ?? ""}
              onChange={(event: any) => {
                const value = this.removeInvalidCharacters(event);
                this.props.onCurrentValue1Changed(value);
              }}
              onBlur={this.props.onBlur}
              ref={this.inputRef}
            />
            <ClearableInput
              id={"to_" + this.props.id}
              className={CS.input}
              value={this.props.currentValue2 ?? ""}
              onChange={(event: any) => {
                const value = this.removeInvalidCharacters(event);
                this.props.onCurrentValue2Changed(value);
              }}
              onBlur={this.props.onBlur}
            />
          </>
        );
      case "null":
      case "nnull":
      default:
        return null;
    }
  }
}

@observer
export class FilterSettingsNumber extends React.Component<{
  setting?: any;
  allowDecimalSeparator: boolean;
  autoFocus: boolean;
  onChange: ()=>void;
  id: string;
}> {

  static get defaultSettings() {
    return new FilterSetting(OPERATORS[0].type)
  }

  @action.bound
  handleBlur() {
    this.handleSettingChange();
  }

  @action.bound
  handleFilterTypeChange() {
    if (this.props.setting.type === "null" || this.props.setting.type === "nnull") {
      this.currentValue1 = "";
      this.currentValue2 = "";
      this.props.setting.val1 = undefined;
      this.props.setting.val2 = undefined;
    }
    this.handleSettingChange();
  }

  componentDidUpdate(prevProps: any) {
    if (prevProps.setting.val1 !== this.props.setting.val1) {
      this.currentValue1 = this.props.setting.val1;
    }
    if (prevProps.setting.val2 !== this.props.setting.val2) {
      this.currentValue2 = this.props.setting.val2;
    }
  }

  @action.bound
  private handleSettingChange() {
    switch (this.props.setting.type) {
      case "eq":
      case "neq":
      case "lt":
      case "gt":
      case "lte":
      case "gte":
        this.props.setting.isComplete = this.props.setting.val1 !== undefined;
        this.props.setting.val2 = undefined;
        break;
      case "between":
      case "nbetween":
        this.props.setting.isComplete =
          this.props.setting.val1 !== undefined && this.props.setting.val2 !== undefined;
        break;
      default:
        this.props.setting.val1 = undefined;
        this.props.setting.val2 = undefined;
        this.props.setting.isComplete = this.props.setting.type === "null" || this.props.setting.type === "nnull";
    }
  }

  @observable
  currentValue1 = this.props.setting.val1;

  @observable
  currentValue2 = this.props.setting.val2;

  onCurrentValue1Changed(newValue: string) {
    this.currentValue1 = newValue;

    const timeOutId = setTimeout(() => {
      runInAction(() => {
        this.props.setting.val1 = this.currentValue1 === ""
          ? undefined
          : parseFloat(this.currentValue1.replace(getCurrentDecimalSeparator(), "."));
        this.handleSettingChange();
        this.props.onChange();
      })
    }, EDITOR_DALEY_MS);
    return () => {
      clearTimeout(timeOutId);
    }
  }

  onCurrentValue2Changed(newValue: string) {
    this.currentValue2 = newValue;

    const timeOutId = setTimeout(() => {
      runInAction(() => {
        this.props.setting.val2 = this.currentValue2 === ""
          ? undefined
          : parseFloat(this.currentValue2.replace(getCurrentDecimalSeparator(), "."));
        this.handleSettingChange();
      })
    }, EDITOR_DALEY_MS);
    return () => {
      clearTimeout(timeOutId);
    }
  }

  render() {
    return (
      <>
        <OpCombo
          id={"combo_" + this.props.id}
          setting={this.props.setting}
          onChange={this.handleFilterTypeChange}
        />
        <OpEditors
          id={"input_" + this.props.id}
          setting={this.props.setting} 
          onBlur={this.handleBlur}
          currentValue1={this.currentValue1}
          currentValue2={this.currentValue2}
          onCurrentValue1Changed={val1 => this.onCurrentValue1Changed(val1)}
          onCurrentValue2Changed={val2 => this.onCurrentValue2Changed(val2)}
          allowDecimalSeparator={this.props.allowDecimalSeparator}
          autoFocus={this.props.autoFocus}
        />
      </>
    );
  }
}
