import React from "react";
import {
  FilterSettingsComboBox,
  FilterSettingsComboBoxItem
} from "../FilterSettingsComboBox";

import CS from "./FilterSettingsCommon.module.css";
import { DateTimeEditor } from "../../../Editors/DateTimeEditor";
import moment from "moment";

export class FilterSettingsDate extends React.Component {
  render() {
    return (
      <>
        <FilterSettingsComboBox trigger={<>=</>}>
          <FilterSettingsComboBoxItem>=</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>&ne;</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>&le;</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>&ge;</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>&#60;</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>&#62;</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>between</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>not between</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>is null</FilterSettingsComboBoxItem>
          <FilterSettingsComboBoxItem>is not null</FilterSettingsComboBoxItem>
        </FilterSettingsComboBox>
        <DateTimeEditor
          value={moment().toISOString()}
          outputFormat="DD.MM.YYYY"
        />
      </>
    );
  }
}
