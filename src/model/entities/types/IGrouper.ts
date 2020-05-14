import { IGroupTreeNode } from "gui/Components/ScreenElements/Table/TableRendering/types";
import {IDataTable} from "./IDataTable";

export interface IGrouper {
  topLevelGroups: IGroupTreeNode[];
  loadChildren(groupHeader: IGroupTreeNode): void;
  parent?: any;
  start(): void;
}
