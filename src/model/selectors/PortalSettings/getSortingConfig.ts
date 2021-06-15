import {ISortingConfig} from "../../entities/types/ISortingConfig";
import {getWorkbenchLifecycle} from "../getWorkbenchLifecycle";
import {latinize} from "../../../utils/string";

function getSortingConfig(ctx: any): ISortingConfig {
  return getWorkbenchLifecycle(ctx).portalSettings?.sortingConfig ?? {
    caseSensitive: false,
    accentSensitive: true
  };
}

export function prepareForSortAndFilter(ctx: any, text: string){
  const sortingConfig = getSortingConfig(ctx);
  if (!sortingConfig.caseSensitive) {
    text = text.toLowerCase();
  }
  if (!sortingConfig.accentSensitive) {
    text = latinize(text);
  }
  return text;
}

