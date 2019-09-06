import { DateTimeEditor } from "gui/Components/ScreenElements/Editors/DateTimeEditor";
import { action, observable } from "mobx";
import { observer } from "mobx-react";
import moment from "moment";
import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

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

export type ISetting = (IOp2 | IOp1 | IOp0) & { dataType: "date" };

const OPERATORS: ISetting[] = [
  { dataType: "date", human: <>=</>, type: "eq", val1: "" },
  { dataType: "date", human: <>&ne;</>, type: "neq", val1: "" },
  { dataType: "date", human: <>&le;</>, type: "lte", val1: "" },
  { dataType: "date", human: <>&ge;</>, type: "gte", val1: "" },
  { dataType: "date", human: <>&#60;</>, type: "lt", val1: "" },
  { dataType: "date", human: <>&#62;</>, type: "gt", val1: "" },
  {
    dataType: "date",
    human: <>between</>,
    type: "between",
    val1: "",
    val2: ""
  },
  {
    dataType: "date",
    human: <>not between</>,
    type: "nbetween",
    val1: "",
    val2: ""
  },
  { dataType: "date", human: <>is null</>, type: "null" },
  { dataType: "date", human: <>is not null</>, type: "nnull" }
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
    case "lt":
    case "gt":
    case "lte":
    case "gte":
      return (
        <DateTimeEditor
          value={setting.val1}
          outputFormat="D.M.YYYY"
          onChange={(event, isoDay) =>
            props.onChange && props.onChange({ ...setting, val1: isoDay })
          }
          onEditorBlur={props.onBlur}
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
              props.onChange && props.onChange({ ...setting, val1: isoDay })
            }
            onEditorBlur={props.onBlur}
          />
          <DateTimeEditor
            value={setting.val2}
            outputFormat="D.M.YYYY"
            onChange={(event, isoDay) =>
              props.onChange && props.onChange({ ...setting, val2: isoDay })
            }
            onEditorBlur={props.onBlur}
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
export class FilterSettingsDate extends React.Component<{
  onTriggerApplySetting?: (setting: ISetting) => void;
}> {
  constructor(props: any) {
    super(props);
    (this.setting as any).val1 = moment().toISOString();
    (this.setting as any).val2 = moment().toISOString();
  }

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
