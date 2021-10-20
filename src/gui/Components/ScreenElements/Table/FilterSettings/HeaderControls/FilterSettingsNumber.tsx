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
}> = observer((props) => {
  return (
    <FilterSettingsComboBox
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
  currentValue1: any;
  currentValue2: any;
  onCurrentValue1Changed: ((value1: any) => void);
  onCurrentValue2Changed: ((value2: any) => void);
  autoFocus: boolean;
}> {

  inputRef = (elm: any) => (this.inputTag = elm);
  inputTag: any;

  componentDidMount() {
    if (this.props.autoFocus) {
      setTimeout(() => {
        this.inputTag?.focus();
      });
    }
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
          <input
            type="number"
            className={CS.input}
            value={this.props.currentValue1 ?? ""}
            onChange={(event: any) => this.props.onCurrentValue1Changed(event.target.value)}
            onBlur={this.props.onBlur}
            ref={this.inputRef}
          />
        );

      case "between":
      case "nbetween":
        return (
          <>
            <input
              type="number"
              className={CS.input}
              value={this.props.currentValue1 ?? ""}
              onChange={(event: any) => this.props.onCurrentValue1Changed(event.target.value)}
              onBlur={this.props.onBlur}
              ref={this.inputRef}
            />
            <input
              type="number"
              className={CS.input}
              value={this.props.currentValue2 ?? ""}
              onChange={(event: any) => this.props.onCurrentValue2Changed(event.target.value)}
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
  autoFocus: boolean;
  onChange: () => void;
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
        this.props.setting.val1 = this.currentValue1 === "" ? undefined : this.currentValue1;
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
        this.props.setting.val2 = this.currentValue2 === "" ? undefined : this.currentValue2;
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
          setting={this.props.setting}
          onChange={this.handleFilterTypeChange}
        />
        <OpEditors
          setting={this.props.setting}
          onBlur={this.handleBlur}
          currentValue1={this.currentValue1}
          currentValue2={this.currentValue2}
          onCurrentValue1Changed={val1 => this.onCurrentValue1Changed(val1)}
          onCurrentValue2Changed={val2 => this.onCurrentValue2Changed(val2)}
          autoFocus={this.props.autoFocus}
        />
      </>
    );
  }
}
