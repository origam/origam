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

import { IFilter } from "model/entities/types/IFilter";
import { FilterSetting } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSetting";
import { filterTypeFromNumber } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import { IProperty } from "model/entities/types/IProperty";
import { FilterGroupManager } from "model/entities/FilterGroupManager";
import { LookupFilterSetting } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsLookup";
import { IFilterGroup } from "model/entities/types/IFilterGroup";

function filterJsonToFilterGroup(
    filterJson: any, 
    properties: IProperty[], 
    selectionMember: string | null | undefined
) {
  const filters: IFilter[] = filterJson.details.map((detail: any) => {
    const property = properties.find((prop) => prop.id === detail.property);
    if (!property) {
      return undefined;
    }
    const setting = property.column === "ComboBox"
      ? new LookupFilterSetting(
        filterTypeFromNumber(detail.operator, property.column),
        true,
        detail.value1,
        detail.value2)
      : new FilterSetting(
        filterTypeFromNumber(detail.operator, property.column),
        true,
        detail.value1,
        detail.value2)

    return {
      propertyId: detail.property,
      dataType: property.column,
      setting: setting,
    };
  }).filter((detail:any) => !!detail);
  const selectionCheckboxFilterGroup = 
    filterJson.details.find((detail: any) => detail.property === selectionMember);
  return {
    filters: filters,
    id: filterJson.id,
    isGlobal: filterJson.isGlobal,
    name: filterJson.name,
    selectionCheckboxFilter: selectionCheckboxFilterGroup?.value1
  };
}

export function cloneFilterGroup(group: IFilterGroup | undefined) {
  if (!group) {
    return undefined;
  }
  const filters = group.filters.map(filter => {
      return {
        propertyId: filter.propertyId,
        dataType: filter.dataType,
        setting: filter.dataType === "ComboBox"
          ? new LookupFilterSetting(
            filter.setting.type,
            filter.setting.isComplete,
            filter.setting.val1,
            filter.setting.val2)
          : new FilterSetting(
            filter.setting.type,
            filter.setting.isComplete,
            filter.setting.val1,
            filter.setting.val2)
      }
    }
  );
  return {
    filters: filters,
    id: group.id,
    isGlobal: group.isGlobal,
    name: group.name,
    selectionCheckboxFilter: group.selectionCheckboxFilter
  };
}

export function addFilterGroups(
  filterGroupManager: FilterGroupManager,
  properties: IProperty[],
  panelConfigurationJson: any,
  selectionMember: string | null | undefined 
) {

  filterGroupManager.filterGroups = panelConfigurationJson.filters
    .map((filterJson: any) => filterJsonToFilterGroup(
      filterJson, properties, selectionMember
    ))
    .filter((group: any) => group);

  if (panelConfigurationJson.initialFilter) {
    filterGroupManager.defaultFilter = filterJsonToFilterGroup(
      panelConfigurationJson.initialFilter,
      properties,
      selectionMember
    );
  }
}