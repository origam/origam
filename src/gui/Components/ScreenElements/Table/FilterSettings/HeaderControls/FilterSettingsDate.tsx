import React, { useImperativeHandle } from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.css";
import { observable, computed, action } from "mobx";
import { observer, PropTypes } from "mobx-react";
import { DateTimeEditor } from "gui/Components/ScreenElements/Editors/DateTimeEditor";

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

type ISetting = IOp2 | IOp1 | IOp0;

const OPERATORS: ISetting[] = [
  { human: <>=</>, type: "eq", val1: "" },
  { human: <>&ne;</>, type: "neq", val1: "" },
  { human: <>&le;</>, type: "lt", val1: "" },
  { human: <>&ge;</>, type: "gt", val1: "" },
  { human: <>&#60;</>, type: "lte", val1: "" },
  { human: <>&#62;</>, type: "gte", val1: "" },
  { human: <>between</>, type: "between", val1: "", val2: "" },
  { human: <>not between</>, type: "nbetween", val1: "", val2: "" },
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
        <DateTimeEditor
          value={setting.val1}
          outputFormat="D.M.YYYY"
          onChange={(event, isoDay) =>
            props.onChange({ ...setting, val1: isoDay })
          }
        />
      );

    case "between":
    case "nbetween":
      return (
        <>
          <DateTimeEditor
            value={setting.val1}
            outputFormat="D.M.YYYY"
            onChange={(event, isoDay) =>
              props.onChange({ ...setting, val1: isoDay })
            }
          />
          <DateTimeEditor
            value={setting.val2}
            outputFormat="D.M.YYYY"
            onChange={(event, isoDay) =>
              props.onChange({ ...setting, val2: isoDay })
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
export class FilterSettingsDate extends React.Component {
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
