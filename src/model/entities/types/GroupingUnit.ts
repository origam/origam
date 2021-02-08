import { T } from "utils/translation";

export enum GroupingUnit {
  Year, Month, Day, Hour, Minute
}

export function GroupingUnitToLabel(groupingUnit: GroupingUnit | undefined){
  switch(groupingUnit){
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

export function groupingUnitToString(unit: GroupingUnit | undefined){
  switch(unit){
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