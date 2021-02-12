import {ICellOffset, IGroupTreeNode} from "gui/Components/ScreenElements/Table/TableRendering/types";
import { IProperty } from "./IProperty";

export interface IGrouper {
  getTotalRowCount(rowId: string): number | undefined;
  getRowIndex(rowId: string): number | undefined;
  topLevelGroups: IGroupTreeNode[];
  allGroups: IGroupTreeNode[];
  getRowById(id: string): any[] | undefined;
  loadChildren(groupHeader: IGroupTreeNode): Generator;
  notifyGroupClosed(group: IGroupTreeNode): void;
  getCellOffset(rowId: string): ICellOffset;
  substituteRecord(row: any[]): void;
  getNextRowId(rowId: string): string;
  getPreviousRowId(selectedRowId: string): string;
  parent?: any;
  start(): void;
}
