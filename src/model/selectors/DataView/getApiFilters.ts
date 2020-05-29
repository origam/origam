import {getFilterConfiguration} from "./getFilterConfiguration";
import {joinWithAND, toFilterItem} from "../../entities/OrigamApiHelpers";
import {getDataView} from "./getDataView";

export function getApiFilters(ctx: any){
  const dataView = getDataView(ctx);
  const filterConfiguration = getFilterConfiguration(dataView);
  const filterList = filterConfiguration.filtering.map(filterItem =>{
    return toFilterItem(filterItem.propertyId, filterItem.setting.type, filterItem.setting.val1);
  });
  return joinWithAND(filterList);
}