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
import { getFilterSettingByProperty } from "model/selectors/DataView/getFilterSettingByProperty";

export const FilterSettings: React.FC = observer(props => {
  const property = useContext(MobXProviderContext).property as IProperty;
  const setting = getFilterSettingByProperty(property, property.id);
  const handleApplyFilterSetting = onApplyFilterSetting(property);
  console.log(setting);

  switch (property.column) {
    case "Text":
      return (
        <FilterSettingsString
          onTriggerApplySetting={handleApplyFilterSetting}
          setting={setting as any}
        />
      );
    case "CheckBox":
      return (
        <FilterSettingsBoolean
          onTriggerApplySetting={handleApplyFilterSetting}
          setting={setting as any}
        />
      );
    case "Date":
      return (
        <FilterSettingsDate
          onTriggerApplySetting={handleApplyFilterSetting}
          setting={setting as any}
        />
      );
    case "Number":
      return (
        <FilterSettingsNumber
          onTriggerApplySetting={handleApplyFilterSetting}
          setting={setting as any}
        />
      );
    case "ComboBox":
      return <FilterSettingsLookup />;
    default:
      return <>{property.column}</>;
  }
});
