import { toJS } from "mobx";
import { getProperty } from "model/selectors/DataView/getProperty";
import { getFilterConfiguration } from "../../../selectors/DataView/getFilterConfiguration";

export function onApplyFilterSetting(ctx: any) {
  const prop = getProperty(ctx);
  return function onApplyFilterSetting(setting: any) {
    console.log("apply filter:", prop, toJS(setting));
    getFilterConfiguration(ctx).setFilter({ propertyId: prop.id, setting });
  };
}
