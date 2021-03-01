import {getFilterConfiguration} from "./getFilterConfiguration";
import {filterToFilterItem, joinWithAND} from "../../entities/OrigamApiHelpers";
import {getDataView} from "./getDataView";

export function getUserFilters(ctx: any, excludePropertyId?: string){
  const dataView = getDataView(ctx);
  const filterConfiguration = getFilterConfiguration(dataView);
  const filterList = filterConfiguration.activeFilters
    .filter(filter => !excludePropertyId || excludePropertyId !== filter.propertyId)
    .filter(filter => filter.setting.isComplete)
    .map(filter => filterToFilterItem(filter));
  return joinWithAND(filterList);
}