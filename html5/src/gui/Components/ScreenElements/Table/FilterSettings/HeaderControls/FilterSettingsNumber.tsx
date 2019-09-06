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
  type: "eq" | "neq" | "lt" | "gt" | "lte" | "gte";

  human: React.ReactNode;
  val1: string;
}

interface IOp2 {
  type: "between" | "nbetween";
  human: React.ReactNode;
  val1: string;
  val2: string;
}

interface IOp0 {
  type: "null" | "nnull";
  human: React.ReactNode;
}

export type ISetting = (IOp2 | IOp1 | IOp0) & { dataType: "number" };

const OPERATORS: ISetting[] = [
  { dataType: "number", human: <>=</>, type: "eq", val1: "0" },
  { dataType: "number", human: <>&ne;</>, type: "neq", val1: "0" },
  { dataType: "number", human: <>&le;</>, type: "lte", val1: "0" },
  { dataType: "number", human: <>&ge;</>, type: "gte", val1: "0" },
  { dataType: "number", human: <>&#60;</>, type: "lt", val1: "0" },
  { dataType: "number", human: <>&#62;</>, type: "gt", val1: "0" },
  {
    dataType: "number",
    human: <>between</>,
    type: "between",
    val1: "0",
    val2: "0"
  },
  {
    dataType: "number",
    human: <>not between</>,
    type: "nbetween",
    val1: "0",
    val2: "0"
  },
  { dataType: "number", human: <>is null</>, type: "null" },
  { dataType: "number", human: <>is not null</>, type: "nnull" }
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
  onChange: (newSetting: ISetting) => void;
  onBlur?: (event: any) => void;
}> = props => {
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
          value={setting.val1}
          onChange={(event: any) =>
            props.onChange({ ...setting, val1: event.target.value })
          }
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
            onChange={(event: any) =>
              props.onChange({ ...setting, val1: event.target.value })
            }
            onBlur={props.onBlur}
          />
          <input
            type="number"
            className={CS.input}
            value={setting.val2}
            onChange={(event: any) =>
              props.onChange({ ...setting, val2: event.target.value })
            }
            onBlur={props.onBlur}
          />
        </>
      );
    case "null":
    case "nnull":
    default:
      return null;
  }
};

@observer
export class FilterSettingsNumber extends React.Component<{
  onTriggerApplySetting?: (setting: ISetting) => void;
}> {
  @observable setting: ISetting = OPERATORS[0];

  @action.bound
  handleBlur() {
    switch (this.setting.type) {
      case "eq":
      case "neq":
      case "lt":
      case "gt":
      case "lte":
      case "gte":
        if (this.setting.val1) {
          this.props.onTriggerApplySetting &&
            this.props.onTriggerApplySetting(this.setting);
        }
        break;
      case "between":
      case "nbetween":
        if (this.setting.val1 && this.setting.val2) {
          this.props.onTriggerApplySetting &&
            this.props.onTriggerApplySetting(this.setting);
        }
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
      case "lt":
      case "gt":
      case "lte":
      case "gte":
        if (this.setting.val1) {
          this.props.onTriggerApplySetting &&
            this.props.onTriggerApplySetting(this.setting);
        }
        break;
      case "between":
      case "nbetween":
        if (this.setting.val1 && this.setting.val2) {
          this.props.onTriggerApplySetting &&
            this.props.onTriggerApplySetting(this.setting);
        }
        break;
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
