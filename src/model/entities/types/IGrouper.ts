import {IGroupTreeNode} from "gui/Components/ScreenElements/Table/TableRendering/types";
import { IProperty } from "./IProperty";

export interface IGrouper {
  getAllValuesOfProp(property: IProperty): Promise<Set<any>>
  topLevelGroups: IGroupTreeNode[];
  allGroups: IGroupTreeNode[];
  loadChildren(groupHeader: IGroupTreeNode): Generator;
  notifyGroupClosed(group: IGroupTreeNode): void;
  parent?: any;
  start(): void;
}
