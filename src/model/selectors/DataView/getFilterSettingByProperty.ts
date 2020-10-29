import {getFilterConfiguration} from "./getFilterConfiguration";

export function getFilterSettingByProperty(ctx: any, prop: string) {
  const filter = getFilterConfiguration(ctx).getSettingByPropertyId(prop);
  return filter ? filter.setting : undefined;
}
