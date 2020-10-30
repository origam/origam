import React from "react";
import { FilterSettingsComboBox, FilterSettingsComboBoxItem } from "../FilterSettingsComboBox";

import { Checkbox } from "../../../../Checkbox";
import { observer } from "mobx-react";
import { action, observable } from "mobx";
import produce from "immer";
import { IFilterSetting } from "../../../../../../model/entities/types/IFilterSetting";
import { FilterSetting } from "./FilterSetting";
import { Operator } from "./Operatots";

const OPERATORS: Operator[] = [Operator.equals];

@observer
export class FilterSettingsBoolean extends React.Component<{
  onTriggerApplySetting?: (setting: any) => void;
  setting?: any;
}> {
  @observable.ref setting: FilterSetting = new FilterSetting(OPERATORS[0].type);

  componentDidMount() {
    this.takeSettingFromProps();
  }

  componentDidUpdate() {
    this.takeSettingFromProps();
  }

  @action.bound takeSettingFromProps() {
    if (this.props.setting) {
      this.setting = this.props.setting;
      return;
    }
    if (!this.setting) {
      this.setting = new FilterSetting(OPERATORS[0].type);
      return;
    }
    if (
      this.setting.val1 !== undefined ||
      this.setting.val2 !== undefined ||
      this.setting.type !== OPERATORS[0].type ||
      this.setting.isComplete !== false ||
      this.setting.lookupId !== undefined
    ) {
      this.setting = new FilterSetting(OPERATORS[0].type);
    }
  }

  @action.bound handleValueClick(event: any) {
    this.setting = produce(this.setting, (draft: IFilterSetting) => {
      if (draft.val1 === undefined) {
        draft.val1 = false;
        draft.isComplete = true;
      } else if (draft.val1 === false) {
        draft.val1 = true;
        draft.isComplete = true;
      } else if (draft.val1 === true) {
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
