import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.css";
import { Checkbox } from "../../../../Checkbox";
import { observer } from "mobx-react";
import { observable, action } from "mobx";

interface IOp1 {
  type: "eq";
  human: React.ReactNode;
  val1: boolean | null;
}

export type ISetting = (IOp1) & { dataType: "boolean" };

const OPERATORS: ISetting[] = [
  { dataType: "boolean", human: <>=</>, type: "eq", val1: null }
];

@observer
export class FilterSettingsBoolean extends React.Component<{
  onTriggerApplySetting?: (setting: ISetting) => void;
  setting?: ISetting;
}> {
  @observable setting: ISetting = OPERATORS[0];

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

  @action.bound handleValueClick(event: any) {
    if (this.setting.val1 === null) {
      this.setting.val1 = false;
    } else if (this.setting.val1 === false) {
      this.setting.val1 = true;
    } else if (this.setting.val1 === true) {
      this.setting.val1 = null;
    }
    this.props.onTriggerApplySetting &&
      this.props.onTriggerApplySetting(this.setting);
  }

  render() {
    return (
      <>
        <FilterSettingsComboBox trigger={<>=</>}>
          <FilterSettingsComboBoxItem>=</FilterSettingsComboBoxItem>
        </FilterSettingsComboBox>
        <Checkbox
          indeterminate={this.setting.val1 === null}
          checked={this.setting.val1 !== null ? this.setting.val1 : undefined}
          onClick={this.handleValueClick}
        />
      </>
    );
  }
}
