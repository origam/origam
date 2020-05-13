import { IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
import {IDataTable} from "./IDataTable";

export interface IGrouper {
  getTopLevelGroups(): IGroupTreeNode[];
  apply(firstGroupingColumn: string): void;
  loadChildren(groupHeader: IGroupTreeNode): void;
  parent?: any;
  sortingFunction: ((dataTable: IDataTable) => (row1: any[], row2: any[]) => number) | undefined;
}
