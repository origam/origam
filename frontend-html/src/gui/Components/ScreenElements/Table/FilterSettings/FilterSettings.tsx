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

import React, { useContext } from "react";
import { FilterSettingsBoolean } from "./HeaderControls/FilterSettingsBoolean";
import { IProperty } from "model/entities/types/IProperty";
import { FilterSettingsString } from "./HeaderControls/FilterSettingsString";
import { FilterSettingsDate } from "./HeaderControls/FilterSettingsDate";
import { observer } from "mobx-react-lite";
import { FilterSettingsNumber } from "./HeaderControls/FilterSettingsNumber";
import { FilterSettingsLookup } from "./HeaderControls/FilterSettingsLookup";
import { flow } from "mobx";
import { MobXProviderContext } from "mobx-react";
import { onApplyFilterSetting } from "model/actions-ui/DataView/TableView/onApplyFilterSetting";
import { getFilterSettingByProperty } from "model/selectors/DataView/getFilterSettingByProperty";
import { IFilterSetting } from "model/entities/types/IFilterSetting";
import { getAllLookupIds } from "model/entities/getAllLookupIds";
import { FilterSettingsTagInput } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsTagInput";
import { getGridFocusManager } from "../../../../../model/entities/GridFocusManager";

export const FilterSettings: React.FC<{ autoFocus: boolean, ctx: any }> = observer((props) => {
  const property = useContext(MobXProviderContext).property as IProperty;

  function getSettings(defaultValue: IFilterSetting) {
    let setting = getFilterSettingByProperty(property, property.id);
    if (!setting) {
      setting = defaultValue;
      onApplyFilterSetting(property)(setting);
    }
    return setting;
  }

  function onFilterValueChange() {
    getGridFocusManager(props.ctx).focusTableOnReload = false;
  }

  switch (property.column) {
    case "Text":
      return <FilterSettingsString
        id={property.modelInstanceId}
        setting={getSettings(FilterSettingsString.defaultSettings)}
        onChange={onFilterValueChange}
        autoFocus={props.autoFocus}/>;
    case "CheckBox":
      return <FilterSettingsBoolean
        id={property.modelInstanceId}
        setting={getSettings(FilterSettingsBoolean.defaultSettings)}
        ctx={property}
      />;
    case "Date":
      return <FilterSettingsDate
        id={property.modelInstanceId}
        setting={getSettings(FilterSettingsDate.defaultSettings)}
        autoFocus={props.autoFocus}
        property={property}
      />;
    case "Number":
      return <FilterSettingsNumber
        id={property.modelInstanceId}
        allowDecimalSeparator={property.entity !== "Integer" && property.entity !== "Long"}
        setting={getSettings(FilterSettingsNumber.defaultSettings)}
        onChange={onFilterValueChange}
        autoFocus={props.autoFocus}/>;
    case "ComboBox":
      const setting = getSettings(FilterSettingsLookup.defaultSettings);
      setting.lookupId = property.lookupId;
      return (
        <FilterSettingsLookup
          id={property.modelInstanceId}
          setting={setting}
          property={property}
          lookup={property.lookup!}
          autoFocus={props.autoFocus}
          getOptions={flow(function*(searchTerm: string) {
            let allLookupIds = yield*getAllLookupIds(property);
            const lookupMap = yield property.lookupEngine?.lookupResolver.resolveList(allLookupIds);

            return Array.from(allLookupIds.values())
              .map((id) => [id, lookupMap.get(id)])
              .filter
              (
                (array) =>
                  array[1] &&
                  array[1].toLocaleLowerCase().includes((searchTerm || "").toLocaleLowerCase())
              )
              .sort((x, y) => x[1] > y[1] ? 1 : -1);
          })}
        />
      );
    case "Checklist":
    case "TagInput":
      return (
        <FilterSettingsTagInput
          id={property.modelInstanceId}
          setting={getSettings(FilterSettingsTagInput.defaultSettings)}
          property={property}
          lookup={property.lookup!}
          autoFocus={props.autoFocus}
          getOptions={flow(function*(searchTerm: string) {
            let allLookupIds = yield*getAllLookupIds(property);
            const lookupMap: Map<any, any> =
              yield property.lookupEngine?.lookupResolver.resolveList(allLookupIds);
            return Array.from(lookupMap.entries())
              .filter
              (
                (array) =>
                  array[1] &&
                  array[1].toLocaleLowerCase().includes((searchTerm || "").toLocaleLowerCase())
              )
              .sort((x, y) => x[1] > y[1] ? 1 : -1);
          })}
        />
      );
    default:
      return <>{property.column}</>;
  }
});
