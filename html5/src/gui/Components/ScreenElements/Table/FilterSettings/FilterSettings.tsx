import React from "react";
import { FilterSettingsBoolean } from "./HeaderControls/FilterSettingsBoolean";
import { IProperty } from "../../../../../model/entities/types/IProperty";
import { FilterSettingsString } from "./HeaderControls/FilterSettingsString";
import { FilterSettingsDate } from "./HeaderControls/FilterSettingsDate";
import { observer } from "mobx-react-lite";
import { FilterSettingsNumber } from "./HeaderControls/FilterSettingsNumber";
import { FilterSettingsLookup } from "./HeaderControls/FilterSettingsLookup";
import { toJS } from "mobx";
import { useContext } from "react";
import { MobXProviderContext } from "mobx-react";
import { onApplyFilterSetting } from "../../../../../model/actions/DataView/TableView/onApplyFilterSetting";

export const FilterSettings: React.FC = observer(props => {
  const property = useContext(MobXProviderContext).property as IProperty;
  const handleApplyFilterSetting = onApplyFilterSetting(property);
  switch (property.column) {
    case "Text":
      return (
        <FilterSettingsString
          onTriggerApplySetting={handleApplyFilterSetting}
        />
      );
    case "CheckBox":
      return <FilterSettingsBoolean />;
    case "Date":
      return (
        <FilterSettingsDate onTriggerApplySetting={handleApplyFilterSetting} />
      );
    case "Number":
      return (
        <FilterSettingsNumber
          onTriggerApplySetting={handleApplyFilterSetting}
        />
      );
    case "ComboBox":
      return <FilterSettingsLookup />;
    default:
      return <>{property.column}</>;
  }
});
