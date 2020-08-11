import React from "react";
import { FilterSettingsComboBox, FilterSettingsComboBoxItem } from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.scss";
import { action, observable } from "mobx";
import { observer } from "mobx-react";
import produce from "immer";
import { FilterSetting } from "./FilterSetting";
import _ from "lodash";

export interface IStringFilterOp {}

const OPERATORS: any[] = [
  { human: <>=</>, type: "eq" },
  { human: <>&ne;</>, type: "neq" },
  { human: <>begins with</>, type: "starts" },
  { human: <>not begins with</>, type: "nstarts" },
  { human: <>ends with</>, type: "ends" },
  { human: <>not ends with</>, type: "nends" },
  { human: <>contain</>, type: "contains" },
  { human: <>not contain</>, type: "ncontains" },
  { human: <>is null</>, type: "null" },
  { human: <>is not null</>, type: "nnull" },
];

const OpCombo: React.FC<{
  setting: any;
  onChange: (newSetting: any) => void;
}> = (props) => {
  return (
    <FilterSettingsComboBox
      trigger={<>{(OPERATORS.find((op) => op.type === props.setting.type) || {}).human}</>}
    >
      {OPERATORS.map((op) => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() =>
            props.onChange(
              produce(props.setting, (draft: any) => {
                draft.type = op.type;
              })
            )
          }
        >
          {op.human}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
};

const OpEditors: React.FC<{
  setting: any;
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
          value={setting.val1}
          onChange={(event: any) =>
            props.onChange &&
            props.onChange(
              produce(setting, (draft: any) => {
                draft.val1 = event.target.value === "" ? undefined : event.target.value;
              })
            )
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
  @observable.ref setting: FilterSetting = new FilterSetting(OPERATORS[0].type, OPERATORS[0].human);

  componentDidMount() {
    // this.takeSettingFromProps();
  }

  componentDidUpdate() {
    // this.takeSettingFromProps();
  }

  @action.bound takeSettingFromProps() {
    if (this.props.setting) {
      this.setting = this.props.setting;
    }
  }

  @action.bound
  handleBlur() {
    this.handleSettingChange();
  }

  @action.bound
  handleChange(newSetting: any) {
    this.setting = newSetting;
    this.handleSettingChangeDebounced();
  }

  handleSettingChangeDebounced = _.debounce(() => this.handleSettingChange(), 1000);

  @action.bound
  handleSettingChange() {
    this.setting.isComplete = this.setting.val1 !== undefined;
    this.setting.val2 = undefined;
    this.props.onTriggerApplySetting && this.props.onTriggerApplySetting(this.setting);
  }

  render() {
    return (
      <>
        <OpCombo setting={this.setting} onChange={this.handleChange} />
        <OpEditors
          setting={this.setting}
          onChange={this.handleChange}
          onBlur={this.handleBlur}
        />

        {/*<input className={CS.input} />*/}
      </>
    );
  }
}
