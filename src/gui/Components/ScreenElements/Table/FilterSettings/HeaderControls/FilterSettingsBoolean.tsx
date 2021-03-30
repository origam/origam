/* eslint-disable no-whitespace-before-property */
import React from "react";
import { FilterSettingsComboBox, FilterSettingsComboBoxItem } from "../FilterSettingsComboBox";

import { Checkbox } from "gui/Components/CheckBox/Checkbox";
import { observer } from "mobx-react";
import { action} from "mobx";
import { FilterSetting } from "./FilterSetting";
import { Operator } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";

const OPERATORS: Operator[] = [Operator.equals];

@observer
export class FilterSettingsBoolean extends React.Component<{
  setting?: any;
}> {

  static get defaultSettings(){
    return new FilterSetting(OPERATORS[0].type)
  }

  @action.bound handleValueClick(event: any) {
      const setting = this.props.setting;
      if (setting.val1 === undefined) {
        setting.val1 = false;
        setting.isComplete = true;
      } else if (setting .val1 === false) {
        setting.val1 = true;
        setting.isComplete = true;
      } else if (setting .val1 === true) {
        setting.val1 = undefined;
        setting.isComplete = false;
    }
  }

  render() {
    return (
      <>
        <FilterSettingsComboBox trigger={<>=</>}>
          <FilterSettingsComboBoxItem>=</FilterSettingsComboBoxItem>
        </FilterSettingsComboBox>
        <Checkbox
          indeterminate={this.props.setting.val1 === undefined}
          checked={this.props.setting.val1}
          onClick={this.handleValueClick}
        />
      </>
    );
  }
}
