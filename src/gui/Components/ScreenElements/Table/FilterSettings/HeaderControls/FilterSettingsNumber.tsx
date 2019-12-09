import React, { useImperativeHandle } from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.css";
import { observable, computed, action } from "mobx";
import { observer, PropTypes } from "mobx-react";
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
            (OPERATORS.find(item => item.typ === props.setting.type) || {})
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
  onChange: (newSetting: any) => void;
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
            props.onChange(
              produce(setting, (draft: any) => {
                draft.val1 = event.target.value;
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
                  draft.val1 = event.target.value;
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
                  draft.val2 = event.target.value;
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
