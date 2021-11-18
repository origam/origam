import {getFilterConfiguration} from "./getFilterConfiguration";
import {filterToFilterItem, joinWithAND} from "../../entities/OrigamApiHelpers";
import {getDataView} from "./getDataView";

export function getUserFilters(args:{ctx: any, excludePropertyId?: string}){
  const dataView = getDataView(args.ctx);
  const filterConfiguration = getFilterConfiguration(dataView);
  const filterList = filterConfiguration.activeFilters
    .filter(filter => !args.excludePropertyId || args.excludePropertyId !== filter.propertyId)
    .filter(filter => filter.setting.isComplete)
    .map(filter => filterToFilterItem(filter));
  return joinWithAND(filterList);
}