import { IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
import {IDataTable} from "./IDataTable";

export interface IGrouper {
  topLevelGroups: IGroupTreeNode[];
  loadChildren(groupHeader: IGroupTreeNode): void;
  parent?: any;
  sortingFunction: ((dataTable: IDataTable) => (row1: any[], row2: any[]) => number) | undefined;
  start(): void;
}
