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

import { T } from "utils/translation";

export enum GroupingUnit {
  Year, Month, Day, Hour, Minute
}

export function GroupingUnitToLabel(groupingUnit: GroupingUnit | undefined) {
  switch (groupingUnit) {
    case GroupingUnit.Year:
      return T("Year", "group_by_year");
    case GroupingUnit.Month:
      return T("Month", "group_by_month");
    case GroupingUnit.Day:
      return T("Day", "group_by_day");
    case GroupingUnit.Hour:
      return T("Hour", "group_by_hour");
    case GroupingUnit.Minute:
      return T("Minute", "group_by_minute");
    default:
      return "";
  }
}

export function groupingUnitToString(unit: GroupingUnit | undefined) {
  switch (unit) {
    case GroupingUnit.Year:
      return "year";
    case GroupingUnit.Month:
      return "month";
    case GroupingUnit.Day:
      return "day";
    case GroupingUnit.Hour:
      return "hour";
    case GroupingUnit.Minute:
      return "minute";
    case undefined:
      return undefined;
    default:
      throw new Error("GroupingUnitToString not implemented for value:" + unit)
  }
}