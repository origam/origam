import {getFilterConfiguration} from "./getFilterConfiguration";

export function getFilterSettingByProperty(ctx: any, prop: string) {
  const setting = getFilterConfiguration(ctx).getSettingByPropertyId(prop);
  return setting ? setting.setting : undefined;
}
