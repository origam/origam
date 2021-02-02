import { IFilter } from "model/entities/types/IFilter";
import { FilterSetting } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSetting";
import { filterTypeFromNumber } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/Operator";
import { IProperty } from "model/entities/types/IProperty";
import { FilterGroupManager } from "model/entities/FilterGroupManager";
import { LookupFilterSetting } from "gui/Components/ScreenElements/Table/FilterSettings/HeaderControls/FilterSettingsLookup";
import { IFilterGroup } from "model/entities/types/IFilterGroup";

function filterJsonToFilterGroup(filterJson: any, properties: IProperty[]) {
  const filters: IFilter[] = filterJson.details.map((detail: any) => {
    const property = properties.find((prop) => prop.id === detail.property)!;
    const setting = property.column === "ComboBox"
      ? new LookupFilterSetting(
          filterTypeFromNumber(detail.operator, property.column),
          true,
          detail.value1,
          detail.value2)
        :new FilterSetting(
          filterTypeFromNumber(detail.operator, property.column),
          true,
          detail.value1,
          detail.value2)
    
    return {
      propertyId: detail.property,
      dataType: property.column,
      setting: setting,
    };
  });
  return {
    filters: filters,
    id: filterJson.id,
    isGlobal: filterJson.isGlobal,
    name: filterJson.name,
  };
}

export function cloneFilterGroup(group: IFilterGroup | undefined){
  if(!group){
    return undefined;
  }
  const filters = group.filters.map(filter => 
    {
      return {
        propertyId: filter.propertyId,
        dataType: filter.dataType,
        setting: filter.dataType === "ComboBox"
          ? new LookupFilterSetting(
            filter.setting.type,
            filter.setting.isComplete,
            filter.setting.val1,
            filter.setting.val2)
          :new FilterSetting(
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
  };
}

export function addFilterGroups(
  filterGroupManager: FilterGroupManager,
  properties: IProperty[],
  panelConfigurationJson: any
) {

  filterGroupManager.filterGroups = panelConfigurationJson.filters
    .map((filterJson: any) => filterJsonToFilterGroup(filterJson, properties))

  if (panelConfigurationJson.initialFilter) {
    filterGroupManager.defaultFilter = filterJsonToFilterGroup(
      panelConfigurationJson.initialFilter,
      properties
    );
  }
}