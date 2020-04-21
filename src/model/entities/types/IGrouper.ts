import { IRowGroup } from "./IRowGroup";
export interface IGrouper {
  getTopLevelGroups(): IRowGroup[];
  apply(firstGroupingColumn: string): void;
  loadChildren(groupHeader: IRowGroup): void;
}
