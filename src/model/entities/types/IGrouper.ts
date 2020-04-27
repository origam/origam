import { IGroupRow } from "gui/Components/ScreenElements/Table/TableRendering/types";

export interface IGrouper {
  getTopLevelGroups(): IGroupRow[];
  apply(firstGroupingColumn: string): void;
  loadChildren(groupHeader: IGroupRow): void;
  parent?: any;
}
