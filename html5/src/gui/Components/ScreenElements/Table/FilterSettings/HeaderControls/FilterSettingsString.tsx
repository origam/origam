import React, { useImperativeHandle } from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.css";
import { observable, computed, action } from "mobx";
import { observer, PropTypes } from "mobx-react";

export interface IStringFilterOp {}

interface IOp1 {
  type:
    | "eq"
    | "neq"
    | "starts"
    | "nstarts"
    | "ends"
    | "nends"
    | "contains"
    | "ncontains";
  human: React.ReactNode;
  val1: string;
}

interface IOp0 {
  type: "null" | "nnull";
  human: React.ReactNode;
}

export type ISetting = (IOp1 | IOp0) & { dataType: "string" };

const OPERATORS: ISetting[] = [
  { dataType: "string", human: <>=</>, type: "eq", val1: "" },
  { dataType: "string", human: <>&ne;</>, type: "neq", val1: "" },
  { dataType: "string", human: <>begins with</>, type: "starts", val1: "" },
  {
    dataType: "string",
    human: <>not begins with</>,
    type: "nstarts",
    val1: ""
  },
  { dataType: "string", human: <>ends with</>, type: "ends", val1: "" },
  { dataType: "string", human: <>not ends with</>, type: "nends", val1: "" },
  { dataType: "string", human: <>contain</>, type: "contains", val1: "" },
  { dataType: "string", human: <>not contain</>, type: "ncontains", val1: "" },
  { dataType: "string", human: <>is null</>, type: "null" },
  { dataType: "string", human: <>is not null</>, type: "nnull" }
];

const OpCombo: React.FC<{
  setting: ISetting;
  onChange: (newSetting: ISetting) => void;
}> = props => {
  return (
    <FilterSettingsComboBox trigger={<>{props.setting.human}</>}>
      {OPERATORS.map(op => (
        <FilterSettingsComboBoxItem
          key={op.type}
          onClick={() =>
            props.onChange({
              ...props.setting,
              type: op.type,
              human: op.human
            } as any)
          }
        >
          {op.human}
        </FilterSettingsComboBoxItem>
      ))}
    </FilterSettingsComboBox>
  );
};

const OpEditors: React.FC<{
  setting: ISetting;
  onChange?: (newSetting: ISetting) => void;
  onBlur?: (event: any) => void;
}> = props => {
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
            props.onChange({ ...setting, val1: event.target.value })
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
  onTriggerApplySetting?: (setting: ISetting) => void;
}> {
  @observable setting: ISetting = OPERATORS[0];

  @action.bound
  handleBlur() {
    switch (this.setting.type) {
      case "eq":
      case "neq":
      case "starts":
      case "nstarts":
      case "ends":
      case "nends":
      case "contains":
      case "ncontains":
        if (this.setting.val1) {
          this.props.onTriggerApplySetting &&
            this.props.onTriggerApplySetting(this.setting);
        }
        break;
      default:
        this.props.onTriggerApplySetting &&
          this.props.onTriggerApplySetting(this.setting);
    }
  }

  @action.bound
  handleChange(newSetting: ISetting) {
    this.setting = newSetting;
    switch (this.setting.type) {
      case "eq":
      case "neq":
      case "starts":
      case "nstarts":
      case "ends":
      case "nends":
      case "contains":
      case "ncontains":
        if (this.setting.val1) {
          this.props.onTriggerApplySetting &&
            this.props.onTriggerApplySetting(this.setting);
        }
      default:
        this.props.onTriggerApplySetting &&
          this.props.onTriggerApplySetting(this.setting);
    }
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
