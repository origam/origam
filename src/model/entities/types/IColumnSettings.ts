import { GroupingUnit } from "./IGroupingConfiguration";

export interface IColumnSettings {
  propertyId: string;
  isHidden: boolean;
  width: number;
  aggregationTypeNumber: number;
  groupingIndex: number | undefined;
  timeGroupingUnit: GroupingUnit | undefined;
}