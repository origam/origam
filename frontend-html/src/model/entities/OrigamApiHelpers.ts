/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import moment, { Moment } from "moment";
import { IFilter } from "./types/IFilter";
import { toOrigamServerString } from "utils/moment";

export function joinWithAND(filterItems: string[]) {
  if (filterItems.length === 0) return "";
  if (filterItems.length === 1) return filterItems[0];
  return '["$AND", ' + filterItems.join(", ") + "]";
}

export function joinWithOR(filterItems: string[]) {
  if (filterItems.length === 0) return "";
  if (filterItems.length === 1) return filterItems[0];
  return '["$OR", ' + filterItems.join(", ") + "]";
}

export function filterToFilterItem(filter: IFilter) {
  return toFilterItem(
    filter.propertyId,
    filter.dataType,
    filter.setting.type,
    filter.setting.filterValue1,
    filter.setting.filterValue2
  );
}

function arrayToString(array: any[]) {
  return `[${array.join(", ")}]`;
}

function valuesToRightHandSide(val1?: any, val2?: any) {
  const val1IsArray = Array.isArray(val1);
  const val2IsArray = Array.isArray(val2);

  if (val1 !== undefined && !val1IsArray && val2 !== undefined && !val2IsArray) {
    return arrayToString([toFilterValueForm(val1), toFilterValueForm(val2)]);
  }
  if (val1IsArray && val2 === undefined) {
    return arrayToString(val1.map((x: any) => toFilterValueForm(x)));
  } else if (!val1IsArray && val2 === undefined) {
    return toFilterValueForm(val1);
  } else if (val1 === undefined && !val2IsArray) {
    return toFilterValueForm(val2);
  }
  throw new Error(`Cannot convert values "${val1}" and "${val2}" to a right hand side`);
}

export function toFilterItem(
  columnId: string,
  dataType: string | null,
  operator: string,
  val1?: any,
  val2?: any
) {
  if (
    dataType === "Date" &&
    (operator === "eq" || operator === "neq") &&
    val1 !== undefined &&
    val1 !== "" &&
    val1 !== null &&
    val1.endsWith("T00:00:00.000")
  ) {
    const upperLimit = new Date(val1);
    upperLimit.setDate(upperLimit.getDate() + 1);
    const dateTimeFormat = new Intl.DateTimeFormat("en", {
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    });
    const [{value: month}, , {value: day}, , {value: year}] = dateTimeFormat.formatToParts(
      upperLimit
    );
    const upperLimitString = year.concat("-", month, "-", day, "T00:00:00.000");
    const substitutedOperator = operator === "eq" ? "between" : "nbetween";
    return `["${columnId}", "${substitutedOperator}", ${valuesToRightHandSide(
      val1,
      upperLimitString
    )}]`;
  }
  const val1Formatted = moment.isMoment(val1)
    ? toOrigamServerString(val1 as Moment)
    : val1;
  return `["${columnId}", "${operator}", ${valuesToRightHandSide(val1Formatted, val2)}]`;
}

function toFilterValueForm(value: any) {
  if (value === undefined) {
    return null;
  }
  return isNaN(value) ? '"' + value + '"' : value;
}
