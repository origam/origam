import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.scss";
import { action, observable } from "mobx";
import { observer } from "mobx-react";
import { FilterSetting } from "./FilterSetting";
import { Operator } from "./Operatots";

const OPERATORS = () =>
  [
    Operator.contains,
    Operator.notContains,
    Operator.startsWith,
    Operator.notStartsWith,
    Operator.endsWith,
    Operator.notEndsWith,
    Operator.equals,
    Operator.notEquals,
    Operator.isNull,
    Operator.isNotNull,
  ] as Operator[];

const OpCombo: React.FC<{
  setting: any;
  onChange: (newSetting: any) => void;
}> = (props) => {
  return (
    <FilterSettingsComboBox
      trigger={<>{(OPERATORS().find((op) => op.type === props.setting.type) || {}).human}</>}
    >
      {OPERATORS().map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() =>{
            props.setting.type = op.type;
            props.onChange(props.setting)}
          }
        >
          {op.human}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
};

const OpEditors: React.FC<{
  setting: FilterSetting;
  onChange?: (newSetting: any) => void;
  onBlur?: (event: any) => void;
}> = (props) => {
  const { setting } = props;
  switch (setting.type) {
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
          value={setting.val1 ?? ""}
          onChange={(event: any) =>{
            setting.val1 = event.target.value === "" ? undefined : event.target.value;
            props.onChange &&
            props.onChange(setting)}
          }
          onBlur={props.onBlur}
        />
      );
    case "null":
    case "nnull":
    default:
      return null;
  }
};

@observer
export class FilterSettingsString extends React.Component<{
  setting?: any;
  onTriggerApplySetting?: (setting: any) => void;
}> {
  @observable.ref setting: FilterSetting = this.props.setting
    ? this.props.setting
    : new FilterSetting(OPERATORS()[0].type);

  @action.bound
  handleBlur() {
    this.handleSettingChange();
  }

  @action.bound
  handleChange(newSetting: any) {
    this.setting = newSetting;
    this.handleSettingChange();
  }

  @action.bound
  handleSettingChange() {
    this.setting.isComplete =
    this.setting.type === "null" || this.setting.type === "nnull" || this.setting.val1 !== undefined;
    this.setting.val2 = undefined;
    this.props.onTriggerApplySetting && this.props.onTriggerApplySetting(this.setting);
  }

  render() {
    return (
      <>
        <OpCombo setting={this.setting} onChange={this.handleChange} />
        <OpEditors setting={this.setting} onChange={this.handleChange} onBlur={this.handleBlur} />
      </>
    );
  }
}
