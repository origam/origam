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
  val1: number;
}

interface IOp2 {
  type: "between" | "nbetween";
  human: React.ReactNode;
  val1: number;
  val2: number;
}

interface IOp0 {
  type: "null" | "nnull";
  human: React.ReactNode;
}

type ISetting = IOp2 | IOp1 | IOp0;

const OPERATORS: ISetting[] = [
  { human: <>=</>, type: "eq", val1: 0 },
  { human: <>&ne;</>, type: "neq", val1: 0 },
  { human: <>&le;</>, type: "lt", val1: 0 },
  { human: <>&ge;</>, type: "gt", val1: 0 },
  { human: <>&#60;</>, type: "lte", val1: 0 },
  { human: <>&#62;</>, type: "gte", val1: 0 },
  { human: <>between</>, type: "between", val1: 0, val2: 0 },
  { human: <>not between</>, type: "nbetween", val1: 0, val2: 0 },
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
          />
          <input
            type="number"
            className={CS.input}
            value={setting.val2}
            onChange={(event: any) =>
              props.onChange({ ...setting, val2: event.target.value })
            }
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
export class FilterSettingsNumber extends React.Component {
  @observable val1: string = "";

  @observable selectedOperator: ISetting = OPERATORS[0];

  render() {
    return (
      <>
        <OpCombo
          setting={this.selectedOperator}
          onChange={newSetting => (this.selectedOperator = newSetting)}
        />
        <OpEditors
          setting={this.selectedOperator}
          onChange={newSetting => (this.selectedOperator = newSetting)}
        />

        {/*<input className={CS.input} />*/}
      </>
    );
  }
}
