import { DateTimeEditor } from "gui/Components/ScreenElements/Editors/DateTimeEditor";
import { action, observable } from "mobx";
import { observer } from "mobx-react";
import moment from "moment";
import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";
import produce from "immer";

const OPERATORS: any[] = [
  { human: <>=</>, type: "eq" },
  { human: <>&ne;</>, type: "neq" },
  { human: <>&le;</>, type: "lte" },
  { human: <>&ge;</>, type: "gte" },
  { human: <>&#60;</>, type: "lt" },
  { human: <>&#62;</>, type: "gt" },
  { human: <>between</>, type: "between" },
  { human: <>not between</>, type: "nbetween" },
  { human: <>is null</>, type: "null" },
  { human: <>is not null</>, type: "nnull" }
];

const OpCombo: React.FC<{
  setting: any;
  onChange: (newSetting: any) => void;
}> = props => {
  return (
    <FilterSettingsComboBox
      trigger={
        <>
          {
            (OPERATORS.find(item => item.type === props.setting.type) || {})
              .human
          }
        </>
      }
    >
      {OPERATORS.map(op => (
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
            props.onChange &&
            props.onChange(
              produce(setting, (draft: any) => {
                draft.val1 = isoDay;
              })
            )
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
              props.onChange &&
              props.onChange(
                produce(setting, (draft: any) => {
                  draft.val1 = isoDay;
                })
              )
            }
            onEditorBlur={props.onBlur}
          />
          <DateTimeEditor
            value={setting.val2}
            outputFormat="D.M.YYYY"
            onChange={(event, isoDay) =>
              props.onChange &&
              props.onChange(
                produce(setting, (draft: any) => {
                  draft.val2 = isoDay;
                })
              )
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
  onTriggerApplySetting?: (setting: any) => void;
  setting?: any;
}> {
  constructor(props: any) {
    super(props);
    (this.setting as any).val1 = undefined; //moment().toISOString();
    (this.setting as any).val2 = undefined; //moment().toISOString();
  }

  @observable.ref setting: any = OPERATORS[0];

  componentDidMount() {
    this.takeSettingFromProps();
  }

  componentDidUpdate() {
    this.takeSettingFromProps();
  }

  @action.bound takeSettingFromProps() {
    if (this.props.setting) {
      this.setting = this.props.setting;
    }
  }

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
  handleChange(newSetting: any) {
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
