import React, { useEffect, useState } from "react";
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

const EDITOR_DALEY_MS = 500;

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

const OpEditors: React.FC<{
  setting: any;
  onChange: (newSetting: any) => void;
  onBlur?: (event: any) => void;
}> = observer((props) => {

  const { setting } = props;
  const [currentValue1, setCurrentValue1] = useState(setting.val1);
  const [currentValue2, setCurrentValue2] = useState(setting.val2);
  
  useEffect(() => {
    const timeOutId = setTimeout(() => {
      runInAction(() => {
        setting.val1 = currentValue1 === "" ? undefined : currentValue1;
        props.onChange(setting);
      })
    }, EDITOR_DALEY_MS);
    return () => {
      clearTimeout(timeOutId);
    }
  }, [currentValue1]);
  
  useEffect(() => {
    const timeOutId = setTimeout(() => {
      runInAction(() => {
        setting.val2 = currentValue2 === "" ? undefined : currentValue2;
        props.onChange(setting);
      })
    }, EDITOR_DALEY_MS);
    return () => {
      clearTimeout(timeOutId);
    }
  }, [currentValue2]);
  
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
          value={currentValue1 ?? ""}
          onChange={(event: any) => setCurrentValue1(event.target.value)}
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
            value={currentValue1 ?? ""}
            onChange={(event: any) => setCurrentValue1(event.target.value)}
            onBlur={props.onBlur}
          />
          <input
            type="number"
            className={CS.input}
            value={currentValue2 ?? ""}
            onChange={(event: any) => setCurrentValue2(event.target.value)}
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
