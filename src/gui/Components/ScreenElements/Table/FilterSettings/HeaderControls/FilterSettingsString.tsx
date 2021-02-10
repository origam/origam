import React, { useEffect, useState } from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.scss";
import {action, observable, runInAction} from "mobx";
import { observer } from "mobx-react";
import { EDITOR_DALEY_MS, FilterSetting } from "./FilterSetting";
import { Operator } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";

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
  ] ;

const OpCombo: React.FC<{
  setting: any;
  onChange: () => void;
}> = observer((props) => {
  return (
    <FilterSettingsComboBox
      trigger={<>{(OPERATORS.find((op) => op.type === props.setting.type) || {}).caption}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() =>
            runInAction(() => {
              props.setting.type = op.type;
              props.onChange()}
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
  setting?: any;
  onCurrentValueChanged: (currentValue: string) => void;
  currentValue: string;
  onChange: () => void;
}> {

  render(){
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
          <input
            className={CS.input}
            value={this.props.currentValue ?? ""}
            onChange={(event: any) => this.props.onCurrentValueChanged(event.target.value)}
            onBlur={this.props.onChange}
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
}> {
  
  static get defaultSettings(){
    return new FilterSetting(OPERATORS[0].type)
  }

  @action.bound
  handleChange() {
    const setting = this.props.setting;
    setting.isComplete =
    setting.type === "null" || setting.type === "nnull" || setting.val1 !== undefined;
    setting.val2 = undefined;
  }

  @action.bound
  handleFilterTypeChange() {
    this.currentValue = "";
    this.props.setting.val1 = undefined;
    this.props.setting.val2 = undefined;
    this.handleChange();
  }

  @observable
  currentValue = this.props.setting.val1;

  onCurrentValueChanged(newValue: string){
    this.currentValue = newValue;

    const timeOutId = setTimeout(() => {
      runInAction(() => {
        this.props.setting.val1 = this.currentValue === "" ? undefined : this.currentValue;
        this.handleChange();
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
          onChange={this.handleFilterTypeChange} />
        <OpEditors 
          setting={this.props.setting} 
          onChange={this.handleChange}
          currentValue={this.currentValue}
          onCurrentValueChanged={value => this.onCurrentValueChanged(value)}/>
      </>
    );
  }
}
