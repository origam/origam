import React from "react";
import { FilterSettingsComboBox, FilterSettingsComboBoxItem } from "../FilterSettingsComboBox";

import { Checkbox } from "../../../../Checkbox";
import { observer } from "mobx-react";
import { action, observable } from "mobx";
import produce from "immer";
import { FilterSetting } from "./FilterSettingsNumber";
import {IFilterSetting} from "../../../../../../model/entities/types/IFilterSetting";

const OPERATORS: any[] = [{ dataType: "boolean", human: <>=</>, type: "eq", val1: undefined }];

@observer
export class FilterSettingsBoolean extends React.Component<{
  onTriggerApplySetting?: (setting: any) => void;
  setting?: any;
}> {
  @observable.ref setting: FilterSetting = new FilterSetting(OPERATORS[0].type, OPERATORS[0].human);

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
    this.setting = produce(this.setting, (draft: IFilterSetting) => {
      if (draft.val1 === undefined) {
        draft.val1 = 0;
        draft.isComplete = true;
      } else if (draft.val1 === 0) {
        draft.val1 = 1;
        draft.isComplete = true;
      } else if (draft.val1 === 1) {
        draft.val1 = undefined;
        draft.isComplete = false;
      }
    });

    this.props.onTriggerApplySetting && this.props.onTriggerApplySetting(this.setting);
  }

  render() {
    return (
      <>
        <FilterSettingsComboBox trigger={<>=</>}>
          <FilterSettingsComboBoxItem>=</FilterSettingsComboBoxItem>
        </FilterSettingsComboBox>
        <Checkbox
          indeterminate={this.setting.val1 === undefined}
          checked={this.setting.val1}
          onClick={this.handleValueClick}
        />
      </>
    );
  }
}
