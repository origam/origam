import { GroupingUnit } from "./GroupingUnit";

export interface IGroupingConfigurationData {

}

export interface IGroupingConfiguration extends IGroupingConfigurationData {
  nextColumnToGroupBy(groupColumnName: string): IGroupingSettings | undefined;
  groupingSettings: Map<string, IGroupingSettings>;
  isGrouping: boolean;
  groupingColumnCount: number;
  orderedGroupingColumnSettings: IGroupingSettings[];
  firstGroupingColumn: IGroupingSettings;

  registerGroupingOnOffHandler(handler: ()=>void): void;

  setGrouping(columnId: string, groupingUnit: GroupingUnit | undefined, groupingIndex: number): void;
  clearGrouping(): void;

  parent?: any;
}

export interface IGroupingSettings{
  columnId: string;
  groupIndex: number;
  groupingUnit: GroupingUnit | undefined;
}

