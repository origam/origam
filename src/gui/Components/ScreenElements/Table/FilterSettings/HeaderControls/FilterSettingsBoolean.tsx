import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from './FilterSettingsCommon.module.css';
import { Checkbox } from '../../../../Checkbox';

export class FilterSettingsBoolean extends React.Component {
  render() {
    return (
      <>
        <FilterSettingsComboBox trigger={<>=</>}>
          <FilterSettingsComboBoxItem>=</FilterSettingsComboBoxItem>
        </FilterSettingsComboBox>
        <Checkbox indeterminate={true} />
      </>
    );
  }
}
