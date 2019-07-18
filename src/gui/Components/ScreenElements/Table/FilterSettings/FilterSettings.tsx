import React from "react";
import { FilterSettingsBoolean } from "./HeaderControls/FilterSettingsBoolean";
import { IProperty } from "../../../../../model/entities/types/IProperty";
import { FilterSettingsString } from "./HeaderControls/FilterSettingsString";
import { FilterSettingsDate } from "./HeaderControls/FilterSettingsDate";

export class FilterSettings extends React.Component<{
  propertyColumn: string;
}> {
  render() {
    switch (this.props.propertyColumn) {
      case "Text":
        return <FilterSettingsString />;
      case "CheckBox":
        return <FilterSettingsBoolean />;
      case "Date":
        return <FilterSettingsDate />;
      default:
        return this.props.propertyColumn;
    }
  }
}
