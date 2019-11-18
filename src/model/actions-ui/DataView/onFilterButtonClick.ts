import { getFilterConfiguration } from "model/selectors/DataView/getFilterConfiguration";

export function onFilterButtonClick(ctx: any) {
  return function onFilterButtonClick(event: any) {
    getFilterConfiguration(ctx).onFilterDisplayClick(event);
  };
}
