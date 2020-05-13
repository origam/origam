
export interface IGroupingConfigurationData {

}

export interface IGroupingConfiguration extends IGroupingConfigurationData {
  nextColumnToGroupBy(groupColumnName: string): string | undefined;
  groupingIndices: Map<string, number>;
  isGrouping: boolean;
  groupingColumnCount: number;
  orderedGroupingColumnIds: string[];
  firstGroupingColumn: string;

  setGrouping(columnId: string, groupingIndex: number): void;
  clearGrouping(): void;
  // applyGrouping(): void;

  parent?: any;
}