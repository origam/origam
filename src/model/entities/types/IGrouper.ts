import {IGroupTreeNode} from "gui/Components/ScreenElements/Table/TableRendering/types";
import { IProperty } from "./IProperty";

export interface IGrouper {
  getRowIndex(rowId: string): number | undefined;
  getAllValuesOfProp(property: IProperty): Promise<Set<any>>
  topLevelGroups: IGroupTreeNode[];
  allGroups: IGroupTreeNode[];
  getRowById(id: string): any[] | undefined;
  loadChildren(groupHeader: IGroupTreeNode): Generator;
  notifyGroupClosed(group: IGroupTreeNode): void;
  parent?: any;
  start(): void;
}
