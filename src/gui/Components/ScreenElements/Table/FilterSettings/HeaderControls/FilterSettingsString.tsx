import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.scss";
import {action, observable, runInAction} from "mobx";
import { observer } from "mobx-react";
import { FilterSetting } from "./FilterSetting";
import { Operator } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import {LookupFilterSetting} from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsLookup";

const OPERATORS = [
    Operator.startsWith,
    Operator.notStartsWith,
    Operator.contains,
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
      trigger={<>{(OPERATORS.find((op) => op.type === props.setting.type) || {}).human}</>}
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
          {op.human}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
});

const OpEditors: React.FC<{
  setting: FilterSetting;
  onChange: () => void;
}> = observer((props) => {

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
            runInAction(()=> {
                setting.val1 = event.target.value === "" ? undefined : event.target.value;
                props.onChange()
              })
            }}
          onBlur={props.onChange}
        />
      );
    case "null":
    case "nnull":
    default:
      return null;
  }
});

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

  render() {
    return (
      <>
        <OpCombo setting={this.props.setting} onChange={this.handleChange} />
        <OpEditors setting={this.props.setting} onChange={this.handleChange}/>
      </>
    );
  }
}
