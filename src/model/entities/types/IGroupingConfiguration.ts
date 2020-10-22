
export interface IGroupingConfigurationData {

}

export interface IGroupingConfiguration extends IGroupingConfigurationData {
  nextColumnToGroupBy(groupColumnName: string): string | undefined;
  groupingIndices: Map<string, number>;
  isGrouping: boolean;
  groupingColumnCount: number;
  orderedGroupingColumnIds: string[];
  firstGroupingColumn: string;

  registerGroupingOnOffHandler(handler: ()=>void): void;

  setGrouping(columnId: string, groupingIndex: number): void;
  clearGrouping(): void;

  parent?: any;
}