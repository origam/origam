import React, { useEffect, useState } from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.scss";
import {action, observable, runInAction} from "mobx";
import { observer } from "mobx-react";
import { EDITOR_DALEY_MS, FilterSetting } from "./FilterSetting";
import { T } from "utils/translation";
import { LookupFilterSetting } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsLookup";
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
  onChange: (newSetting: any) => void;
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
            props.onChange(props.setting);}
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
  onChange: (newSetting: any) => void;
  onBlur?: (event: any) => void;
}> {

  @observable
  currentValue1 = this.props.setting.val1;

  @observable
  currentValue2 = this.props.setting.val2;

  componentDidUpdate(prevProps: any){
    if(prevProps.setting?.val1 !== this.props.setting?.val1){
      this.currentValue1 = this.props.setting?.val1;
    }
    if(prevProps.setting?.val2 !== this.props.setting?.val2){
      this.currentValue2 = this.props.setting?.val2;
    }
  }

  onCurrentValue1Changed(newValue: string){
    this.currentValue1 = newValue;

    const timeOutId = setTimeout(() => {
      runInAction(() => {
        this.props.setting.val1 = this.currentValue1 === "" ? undefined : this.currentValue1;
        this.props.onChange(this.props.setting);
      })
    }, EDITOR_DALEY_MS);
    return () => {
      clearTimeout(timeOutId);
    }
  }

  onCurrentValue2Changed(newValue: string){
    this.currentValue2 = newValue;

    const timeOutId = setTimeout(() => {
      runInAction(() => {
        this.props.setting.val2 = this.currentValue2 === "" ? undefined : this.currentValue2;
        this.props.onChange(this.props.setting);
      })
    }, EDITOR_DALEY_MS);
    return () => {
      clearTimeout(timeOutId);
    }
  }

  render(){
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
            value={this.currentValue1 ?? ""}
            onChange={(event: any) => this.onCurrentValue1Changed(event.target.value)}
            onBlur={this.props.onBlur}
          />
        );
  
      case "between":
      case "nbetween":
        return (
          <>
            <input
              type="number"
              className={CS.input}
              value={this.currentValue1 ?? ""}
              onChange={(event: any) => this.onCurrentValue1Changed(event.target.value)}
              onBlur={this.props.onBlur}
            />
            <input
              type="number"
              className={CS.input}
              value={this.currentValue2 ?? ""}
              onChange={(event: any) => this.onCurrentValue2Changed(event.target.value)}
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
}> {

  static get defaultSettings(){
    return new FilterSetting(OPERATORS[0].type)
  }

  @action.bound
  handleBlur() {
    this.handleSettingChange();
  }

  @action.bound
  handleChange(newSetting: any) {
    this.handleSettingChange();
  }

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

  render() {
    return (
      <>
        <OpCombo setting={this.props.setting} onChange={this.handleChange} />
        <OpEditors setting={this.props.setting} onChange={this.handleChange} onBlur={this.handleBlur} />
      </>
    );
  }
}
