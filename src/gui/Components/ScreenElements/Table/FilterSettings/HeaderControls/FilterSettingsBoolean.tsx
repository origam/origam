import React from "react";
import { FilterSettingsComboBox, FilterSettingsComboBoxItem } from "../FilterSettingsComboBox";

import { Checkbox } from "../../../../Checkbox";
import { observer } from "mobx-react";
import { action, observable } from "mobx";
import { IFilterSetting } from "../../../../../../model/entities/types/IFilterSetting";
import { FilterSetting } from "./FilterSetting";
import { Operator } from "./Operatots";

const OPERATORS: Operator[] = [Operator.equals];

@observer
export class FilterSettingsBoolean extends React.Component<{
  onTriggerApplySetting?: (setting: any) => void;
  setting?: any;
}> {
  @observable.ref setting: FilterSetting = this.props.setting
    ? this.props.setting
    : new FilterSetting(OPERATORS[0].type);

  @action.bound handleValueClick(event: any) {
      if (this.setting .val1 === undefined) {
        this.setting .val1 = false;
        this.setting .isComplete = true;
      } else if (this.setting .val1 === false) {
        this.setting .val1 = true;
        this.setting .isComplete = true;
      } else if (this.setting .val1 === true) {
        this.setting .val1 = undefined;
        this.setting .isComplete = false;
    };

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
