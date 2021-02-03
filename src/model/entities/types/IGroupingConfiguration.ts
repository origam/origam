
export interface IGroupingConfigurationData {

}

export interface IGroupingConfiguration extends IGroupingConfigurationData {
  nextColumnToGroupBy(groupColumnName: string): string | undefined;
  groupingSettings: Map<string, IGroupingSettings>;
  isGrouping: boolean;
  groupingColumnCount: number;
  orderedGroupingColumnIds: string[];
  firstGroupingColumn: string;

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

export enum GroupingUnit {
  Year, Month, Day, Hour, Minute
}