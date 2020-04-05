import { IDataOrdering } from "../OrderingConfiguration";

export interface IGroupingConfigurationData {

}

export interface IGroupingConfiguration extends IGroupingConfigurationData {
  groupingIndices: Map<string, number>;

  setGrouping(columnId: string, groupingIndex: number): void;
  clearGrouping(): void;
  applyGrouping(): void;

  generatedOrderingTerms: IDataOrdering[];

  parent?: any;
}