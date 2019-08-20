import React from "react";
import { FilterSettingsBoolean } from "./HeaderControls/FilterSettingsBoolean";
import { IProperty } from "../../../../../model/entities/types/IProperty";
import { FilterSettingsString } from "./HeaderControls/FilterSettingsString";
import { FilterSettingsDate } from "./HeaderControls/FilterSettingsDate";
import { observer } from "mobx-react-lite";
import { FilterSettingsNumber } from "./HeaderControls/FilterSettingsNumber";
import { FilterSettingsLookup } from "./HeaderControls/FilterSettingsLookup";

export const FilterSettings: React.FC<{ propertyColumn: string }> = observer(
  props => {
    switch (props.propertyColumn) {
      case "Text":
        return <FilterSettingsString />;
      case "CheckBox":
        return <FilterSettingsBoolean />;
      case "Date":
        return <FilterSettingsDate />;
      case "Number":
        return <FilterSettingsNumber />;
      case "ComboBox":
        return <FilterSettingsLookup />;
      default:
        return <>{props.propertyColumn}</>;
    }
  }
);
