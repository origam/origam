import { getDataView } from "./getDataView";
import { getFilterConfiguration } from "./getFilterConfiguration";

export function getUserFilterLookups(ctx: any): { [key: string]: string } | undefined {
  const dataView = getDataView(ctx);
  const filterConfiguration = getFilterConfiguration(dataView);
  const lookupMap = filterConfiguration.activeFilters
    .filter((filter) => filter.setting.isComplete && filter.setting.lookupId)
    .reduce(function (lookupMap: {[key: string]: string }, filter) {
      lookupMap[filter.propertyId] = filter.setting.lookupId!;
      return lookupMap;
    }, {});

  return Object.keys(lookupMap).length !== 0 ? lookupMap : undefined;
}
