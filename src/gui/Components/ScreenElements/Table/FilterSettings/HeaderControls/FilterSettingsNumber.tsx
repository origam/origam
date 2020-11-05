import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem,
} from "gui/Components/ScreenElements/Table/FilterSettings/FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.scss";
import {action, observable, runInAction} from "mobx";
import { observer } from "mobx-react";
import { FilterSetting } from "./FilterSetting";
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
      trigger={<>{(OPERATORS.find((item) => item.type === props.setting.type) || {}).human}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() => {
            props.setting.type = op.type;
            props.onChange(props.setting);}
          }
        >
          {op.human}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
});

const OpEditors: React.FC<{
  setting: any;
  onChange: (newSetting: any) => void;
  onBlur?: (event: any) => void;
}> = observer((props) => {
  const { setting } = props;
  switch (setting.type) {
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
          value={setting.val1 ?? ""}
          onChange={(event: any) => {
            runInAction(() => {
              setting.val1 = event.target.value === "" ? undefined : event.target.value;
              props.onChange(setting);
            })
          }}
          onBlur={props.onBlur}
        />
      );

    case "between":
    case "nbetween":
      return (
        <>
          <input
            type="number"
            className={CS.input}
            value={setting.val1}
            onChange={(event: any) => {
              runInAction(() => {
                setting.val1 = event.target.value === "" ? undefined : event.target.value;
                props.onChange(setting);
              });
            }}
            onBlur={props.onBlur}
          />
          <input
            type="number"
            className={CS.input}
            value={setting.val2}
            onChange={(event: any) => {
              runInAction(()=>{
                setting.val2 = event.target.value === "" ? undefined : event.target.value;
                props.onChange(setting);
              });
            }}
            onBlur={props.onBlur}
          />
        </>
      );
    case "null":
    case "nnull":
    default:
      return null;
  }
});

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
