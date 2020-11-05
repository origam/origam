import React, { useContext } from "react";
import { FilterSettingsBoolean } from "./HeaderControls/FilterSettingsBoolean";
import { IProperty } from "../../../../../model/entities/types/IProperty";
import { FilterSettingsString } from "./HeaderControls/FilterSettingsString";
import { FilterSettingsDate } from "./HeaderControls/FilterSettingsDate";
import { observer } from "mobx-react-lite";
import { FilterSettingsNumber } from "./HeaderControls/FilterSettingsNumber";
import {FilterSettingsLookup, LookupFilterSetting} from "./HeaderControls/FilterSettingsLookup";
import { flow } from "mobx";
import { MobXProviderContext } from "mobx-react";
import { onApplyFilterSetting } from "../../../../../model/actions-ui/DataView/TableView/onApplyFilterSetting";
import { getFilterSettingByProperty } from "model/selectors/DataView/getFilterSettingByProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataView } from "model/selectors/DataView/getDataView";
import { isInfiniteScrollLoader } from "gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";
import { Operator } from "./HeaderControls/Operatots";
import {IFilterSetting} from "model/entities/types/IFilterSetting";

export const FilterSettings: React.FC = observer((props) => {
  const property = useContext(MobXProviderContext).property as IProperty;
  const dataTable = getDataTable(property);
  const dataView = getDataView(property);

  function getSettings(defaultValue: IFilterSetting){
    let setting = getFilterSettingByProperty(property, property.id);
    if(!setting){
      setting = defaultValue;
      onApplyFilterSetting(property)(setting);
    }
    return setting;
  }

  switch (property.column) {
    case "Text":
      return (
        <FilterSettingsString
          setting={getSettings(FilterSettingsString.defaultSettings)}
        />
      );
    case "CheckBox":
      return (
        <FilterSettingsBoolean
          setting={getSettings(FilterSettingsBoolean.defaultSettings)}
        />
      );
    case "Date":
      return (
        <FilterSettingsDate
          setting={getSettings(FilterSettingsDate.defaultSettings)}
        />
      );
    case "Number":
      return (
        <FilterSettingsNumber
          setting={getSettings(FilterSettingsNumber.defaultSettings)}
        />
      );
    case "ComboBox":
      return (
        <FilterSettingsLookup
          setting={getSettings(FilterSettingsLookup.defaultSettings)}
          property={property}
          lookup={property.lookup!}
          getOptions={flow(function* (searchTerm: string) {
            const allIds = isInfiniteScrollLoader(dataView.infiniteScrollLoader)
              ? yield dataView.infiniteScrollLoader.getAllValuesOfProp(property)
              :  Array.from(new Set(dataTable.getAllValuesOfProp(property)).values());
            const lookupMap = yield property.lookupEngine?.lookupResolver.resolveList(allIds);

            return Array.from(allIds.values())
              .map(id => [ id, lookupMap.get(id)])
              .filter(
                (array) =>
                  array[1] &&
                  array[1].toLocaleLowerCase().includes((searchTerm || "").toLocaleLowerCase())
              );
          })}
        />
      );
    default:
      return <>{property.column}</>;
  }
});
