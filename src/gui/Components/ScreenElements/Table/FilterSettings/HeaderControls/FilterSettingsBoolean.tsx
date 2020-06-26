import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.scss";
import { Checkbox } from "../../../../Checkbox";
import { observer } from "mobx-react";
import { observable, action } from "mobx";
import produce from "immer";

const OPERATORS: any[] = [
  { dataType: "boolean", human: <>=</>, type: "eq", val1: null }
];

@observer
export class FilterSettingsBoolean extends React.Component<{
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

  @action.bound handleValueClick(event: any) {
    this.setting = produce(this.setting, (draft: any) => {
      if (draft.val1 === null) {
        draft.val1 = false;
      } else if (draft.val1 === false) {
        draft.val1 = true;
      } else if (draft.val1 === true) {
        draft.val1 = null;
      }
    });

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
