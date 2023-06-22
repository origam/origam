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
import { Operator } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import { ClearableInput } from "gui/connections/MobileComponents/Grid/ClearableInput";
import { requestFocus } from "utils/focus";

const OPERATORS = [
  Operator.contains,
  Operator.startsWith,
  Operator.notStartsWith,
  Operator.notContains,
  Operator.endsWith,
  Operator.notEndsWith,
  Operator.equals,
  Operator.notEquals,
  Operator.isNull,
  Operator.isNotNull,
];

const OpCombo: React.FC<{
  setting: any;
  id: string;
  onChange: () => void;
}> = observer((props) => {
  return (
    <FilterSettingsComboBox
      id={props.id}
      trigger={<>{(OPERATORS.find((op) => op.type === props.setting.type) || {}).caption}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() =>
            runInAction(() => {
                props.setting.type = op.type;
                props.onChange()
              }
            )}
        >
          {op.caption}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
});


@observer
class OpEditors extends React.Component<{
  id: string;
  setting?: any;
  onCurrentValueChanged: (currentValue: string) => void;
  currentValue: string;
  onChange: () => void;
  autoFocus: boolean;
}> {

  inputRef = (elm: any) => (this.inputTag = elm);
  inputTag: any;

  componentDidMount() {
    if (this.props.autoFocus) {
      setTimeout(() => {
        requestFocus(this.inputTag);
      });
    }
  }

  render() {
    switch (this.props.setting.type) {
      case "eq":
      case "neq":
      case "starts":
      case "nstarts":
      case "ends":
      case "nends":
      case "contains":
      case "ncontains":
        return (
          <ClearableInput
            id={this.props.id}
            className={CS.input}
            value={this.props.currentValue ?? ""}
            onChange={(event: any) => this.props.onCurrentValueChanged(event.target.value)}
            onBlur={this.props.onChange}
            ref={this.inputRef}
          />
        );
      case "null":
      case "nnull":
      default:
        return null;
    }

  }
}

@observer
export class FilterSettingsString extends React.Component<{
  setting?: any;
  autoFocus: boolean;
  onChange: ()=>void;
  id:string;
}> {

  static get defaultSettings() {
    return new FilterSetting(OPERATORS[0].type)
  }

  @action.bound
  handleChange() {
    const setting = this.props.setting;
    setting.isComplete = setting.type === "null" || setting.type === "nnull" || setting.val1 !== undefined;
    setting.val2 = undefined;
  }

  @action.bound
  handleFilterTypeChange() {
    if (this.props.setting.type === "null" || this.props.setting.type === "nnull") {
      this.currentValue = "";
      this.props.setting.val1 = undefined;
      this.props.setting.val2 = undefined;
    }
    this.handleChange();
  }

  componentDidUpdate(prevProps: any) {
    if (prevProps.setting.val1 !== this.props.setting.val1) {
      this.currentValue = this.props.setting.val1;
    }
  }

  @observable
  currentValue = this.props.setting.val1;

  onCurrentValueChanged(newValue: string) {
    this.currentValue = newValue;

    const timeOutId = setTimeout(() => {
      runInAction(() => {
        this.props.setting.val1 = this.currentValue === "" ? undefined : this.currentValue;
        this.handleChange();
        this.props.onChange();
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
          id={"combo_"+this.props.id}
          setting={this.props.setting}
          onChange={this.handleFilterTypeChange} />
        <OpEditors
          id={"input_"+this.props.id}
          setting={this.props.setting} 
          onChange={this.handleChange}
          currentValue={this.currentValue}
          onCurrentValueChanged={value => this.onCurrentValueChanged(value)}
          autoFocus={this.props.autoFocus}/>
      </>
    );
  }
}
