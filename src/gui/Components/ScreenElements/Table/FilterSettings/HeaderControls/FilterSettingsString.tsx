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

type ISetting = IOp1 | IOp0;

const OPERATORS: ISetting[] = [
  { human: <>=</>, type: "eq", val1: "" },
  { human: <>&ne;</>, type: "neq", val1: "" },
  { human: <>begins with</>, type: "starts", val1: "" },
  { human: <>not begins with</>, type: "nstarts", val1: "" },
  { human: <>ends with</>, type: "ends", val1: "" },
  { human: <>not ends with</>, type: "nends", val1: "" },
  { human: <>contain</>, type: "contains", val1: "" },
  { human: <>not contain</>, type: "ncontains", val1: "" },
  { human: <>is null</>, type: "null" },
  { human: <>is not null</>, type: "nnull" }
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
          onClick={() => props.onChange(op)}
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
            props.onChange({ ...setting, val1: event.target.value })
          }
        />
      );
    case "null":
    case "nnull":
    default:
      return null;
  }
};

@observer
export class FilterSettingsString extends React.Component {
  @observable selectedOperator: ISetting = OPERATORS[0];

  render() {
    return (
      <>
        <OpCombo
          setting={this.selectedOperator}
          onChange={(newSetting) => this.selectedOperator = newSetting}
        />
        <OpEditors 
          setting={this.selectedOperator}
          onChange={(newSetting) => this.selectedOperator = newSetting}
        />

        {/*<input className={CS.input} />*/}
      </>
    );
  }
}
