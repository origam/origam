import { IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";

export interface IGrouper {
  getTopLevelGroups(): IGroupTreeNode[];
  apply(firstGroupingColumn: string): void;
  loadChildren(groupHeader: IGroupTreeNode): void;
  parent?: any;
}
