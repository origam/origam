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
function arrayToString(array: any[]){
  return `[${array.join(", ")}]`;
}

function valesToRightHandSide(val1?: any, val2?: any){
  const val1IsArray = Array.isArray(val1);
  const val2IsArray = Array.isArray(val2);

  if(val1 !== undefined && !val1IsArray && val2 !== undefined && !val2IsArray){
    return arrayToString([ toFilterValueForm(val1),  toFilterValueForm(val2)]);
  }
  if(val1IsArray && val2 === undefined){
    return arrayToString(val1.map((x: any) => toFilterValueForm(x)));
  }
  else if(!val1IsArray && val2 === undefined){
    return  toFilterValueForm(val1);
  }
  else if(val1 === undefined && !val2IsArray){
    return  toFilterValueForm(val2);
  }
  throw new Error(`Cannot convert values "${val1}" and "${val2}" to a right hand side`)
}

export function toFilterItem(columnId: string, operator: string, val1?: any, val2?: any) {
  return `["${columnId}", "${operator}", ${valesToRightHandSide(val1, val2)}]`;
}

function toFilterValueForm(value: any) {
  return isNaN(value) ? '"' + value + '"' : value;
}
