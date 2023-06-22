/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

/* eslint-disable no-whitespace-before-property */
import React from "react";
import { FilterSettingsComboBox, FilterSettingsComboBoxItem } from "../FilterSettingsComboBox";

import { Checkbox } from "gui/Components/CheckBox/Checkbox";
import { observer } from "mobx-react";
import { action } from "mobx";
import { FilterSetting } from "./FilterSetting";
import { Operator } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import { MobileBooleanInput } from "gui/connections/MobileComponents/Form/MobileBooleanInput";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import S from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsBoolean.module.scss";

const OPERATORS: Operator[] = [Operator.equals];

@observer
export class FilterSettingsBoolean extends React.Component<{
  setting?: any;
  id: string;
  ctx: any;
}> {

  static get defaultSettings() {
    return new FilterSetting(OPERATORS[0].type)
  }

  @action.bound handleValueClick(event: any) {
    const setting = this.props.setting;
    if (setting.val1 === undefined) {
      setting.val1 = false;
      setting.isComplete = true;
    } else if (setting.val1 === false) {
      setting.val1 = true;
      setting.isComplete = true;
    } else if (setting.val1 === true) {
      setting.val1 = undefined;
      setting.isComplete = false;
    }
  }

  render() {
    return (
      <>
        <FilterSettingsComboBox
          id={"combo_" + this.props.id}
          trigger={<>=</>}
        >
          <FilterSettingsComboBoxItem>=</FilterSettingsComboBoxItem>
        </FilterSettingsComboBox>
        {isMobileLayoutActive(this.props.ctx)
          ? <div className={S.mobileInputContainer}>
            <MobileBooleanInput
              checked={this.props.setting.val1}
              onChange={this.handleValueClick}
            />
          </div>
          : <Checkbox
            id={"input_" + this.props.id}
            indeterminate={this.props.setting.val1 === undefined}
            checked={this.props.setting.val1}
            onClick={this.handleValueClick}
          />
        }
      </>
    );
  }
}
