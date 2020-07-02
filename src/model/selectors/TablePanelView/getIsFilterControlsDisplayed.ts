import {getFilterConfiguration} from "../DataView/getFilterConfiguration";

export function getIsFilterControlsDisplayed(ctx: any) {
  return getFilterConfiguration(ctx).isFilterControlsDisplayed;
}