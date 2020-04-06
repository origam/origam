
export interface IGroupingConfigurationData {

}

export interface IGroupingConfiguration extends IGroupingConfigurationData {
  groupingIndices: Map<string, number>;

  setGrouping(columnId: string, groupingIndex: number): void;
  clearGrouping(): void;
  applyGrouping(): void;


  parent?: any;
}