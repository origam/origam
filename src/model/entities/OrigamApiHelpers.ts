import { IFilter } from "./types/IFilter";

export function joinWithAND(filterItems: string[]) {
  if (filterItems.length === 0) return "";
  if (filterItems.length === 1) return filterItems[0];
  return '["$AND", ' + filterItems.join(", ") + "]";
}

export function filterToFilterItem(filter: IFilter) {
  return toFilterItem(
    filter.propertyId,
    filter.setting.type,
    filter.setting.val1,
    filter.setting.val2
  );
}
export function toFilterItem(columnId: string, operator: string, val1: any, val2?: string) {
  return val2
    ? `["${columnId}", "${operator}", ${toFilterValueForm(val1)}, ${toFilterValueForm(val2)}]`
    : `["${columnId}", "${operator}", ${toFilterValueForm(val1)}]`;
}

function toFilterValueForm(value: any) {
  return typeof value === "string" ? '"' + value + '"' : value;
}
