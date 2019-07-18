import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from './FilterSettingsCommon.module.css';

export class FilterSettingsString extends React.Component {
  render() {
    return (
      <>
        <FilterSettingsComboBox trigger={<>=</>}>
          <FilterSettingsComboBoxItem>=</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>&ne;</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>begins with</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>
            not begins with
          </FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>ends with</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>not ends with</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>contains</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>not contains</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>is null</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>is not null</FilterSettingsComboBoxItem>
        </FilterSettingsComboBox>
        <input className={CS.input} />
      </>
    );
  }
}
