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
    filter.setting.filterValue1,
    filter.setting.filterValue2
  );
}
export function toFilterItem(columnId: string, operator: string, val1?: any, val2?: string) {
  const values = [Array.isArray(val1) ? val1 : [val1],
                  Array.isArray(val2) ? val2 : [val2]]
    .flat()
    .filter((x) => !!x)
    .map((x) => toFilterValueForm(x));
  const items = [columnId, operator]
    .map((x) => `"${x}"`)
    .concat(values)
    .join(", ");
  return `[${items}]`;
}

function toFilterValueForm(value: any) {
  return isNaN(value) ? '"' + value + '"' : value;
}
