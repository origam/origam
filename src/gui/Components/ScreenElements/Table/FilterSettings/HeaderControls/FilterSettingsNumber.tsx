import React from "react";
import { FilterSettingsComboBox, FilterSettingsComboBoxItem } from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.scss";
import { action, observable } from "mobx";
import { observer } from "mobx-react";
import produce from "immer";
import { FilterSetting } from "./FilterSetting";
import {T} from "utils/translation";

const OPERATORS = () => [
  { human: <>=</>, type: "eq" },
  { human: <>&ne;</>, type: "neq" },
  { human: <>&le;</>, type: "lte" },
  { human: <>&ge;</>, type: "gte" },
  { human: <>&#60;</>, type: "lt" },
  { human: <>&#62;</>, type: "gt" },
  { human: <>{T("between", "filter_operator_between")}</>, type: "between" },
  { human: <>{T("not between", "filter_operator_not_between")}</>, type: "nbetween" },
  { human: <>{T("is null", "filter_operator_is_null")}</>, type: "null" },
  {
    human: <>{T("is not null", "filter_operator_not_is_null")}</>,
    type: "nnull"
  }
] as any [];

const OpCombo: React.FC<{
  setting: any;
  onChange: (newSetting: any) => void;
}> = (props) => {
  return (
    <FilterSettingsComboBox
      trigger={<>{(OPERATORS().find((item) => item.type === props.setting.type) || {}).human}</>}
    >
      {OPERATORS().map((op) => (
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
  onChange: (newSetting: any) => void;
  onBlur?: (event: any) => void;
}> = (props) => {
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
            props.onChange(
              produce(setting, (draft: any) => {
                draft.val1 = event.target.value === "" ? undefined : event.target.value;
              })
            )
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
              props.onChange(
                produce(setting, (draft: any) => {
                  draft.val1 = event.target.value === "" ? undefined : event.target.value;
                })
              )
            }
            onBlur={props.onBlur}
          />
          <input
            type="number"
            className={CS.input}
            value={setting.val2}
            onChange={(event: any) =>
              props.onChange(
                produce(setting, (draft: any) => {
                  draft.val2 = event.target.value === "" ? undefined : event.target.value;
                })
              )
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
  onTriggerApplySetting?: (setting: any) => void;
  setting?: any;
}> {
  @observable.ref setting: FilterSetting = new FilterSetting(OPERATORS()[0].type, OPERATORS()[0].human);

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
    this.handleSettingChange();
  }

  @action.bound
  handleChange(newSetting: any) {
    this.setting = newSetting;
    this.handleSettingChange();
  }

  private handleSettingChange() {
    switch (this.setting.type) {
      case "eq":
      case "neq":
      case "lt":
      case "gt":
      case "lte":
      case "gte":
        this.setting.isComplete = this.setting.val1 !== undefined;
        this.setting.val2 = undefined;
        this.props.onTriggerApplySetting && this.props.onTriggerApplySetting(this.setting);
        break;
      case "between":
      case "nbetween":
        this.setting.isComplete =
          this.setting.val1 !== undefined && this.setting.val2 !== undefined;
        this.props.onTriggerApplySetting && this.props.onTriggerApplySetting(this.setting);
        break;
      default:
        this.setting.val1 = undefined;
        this.setting.val2 = undefined;
        this.setting.isComplete = this.setting.type === "null" || this.setting.type === "nnull";
        this.props.onTriggerApplySetting && this.props.onTriggerApplySetting(this.setting);
    }
  }

  render() {
    return (
      <>
        <OpCombo setting={this.setting} onChange={this.handleChange} />
        <OpEditors setting={this.setting} onChange={this.handleChange} onBlur={this.handleBlur} />

        {/*<input className={CS.input} />*/}
      </>
    );
  }
}
