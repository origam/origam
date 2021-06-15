import {ISortingConfig} from "../../entities/types/ISortingConfig";
import {getWorkbenchLifecycle} from "../getWorkbenchLifecycle";
import {latinize} from "../../../utils/string";

function getSortingConfig(ctx: any): ISortingConfig {
  return getWorkbenchLifecycle(ctx).portalSettings?.sortingConfig ?? {
    caseSensitive: false,
    accentSensitive: true
  };
}

export function prepareForSortAndFilter(ctx: any, text: string | undefined | null){
  if(text === undefined || text === null){
    return text;
  }
  const sortingConfig = getSortingConfig(ctx);
  if (!sortingConfig.caseSensitive) {
    text = text.toLowerCase();
  }
  if (!sortingConfig.accentSensitive) {
    text = latinize(text);
  }
  return text;
}

export function prepareAnyForSortAndFilter(ctx: any, value: any): any {

  if (typeof value === 'string' || value instanceof String){
    return prepareForSortAndFilter(ctx, value as string);
  }
  if(Array.isArray(value)){
    return value.map(val =>  prepareAnyForSortAndFilter(ctx, val))
  }
  return value;
}

