
import { observable } from "mobx";
import { IGroupTreeNode } from "./types";
import {IHeaderContainer} from "../../../../Workbench/ScreenArea/TableView/TableView";

export interface IGroupItemData{
  childGroups: IGroupTreeNode[];
  childRows: any[][];
  columnId: string;
  columnValue: string ;
  groupLabel: string;
  parent: IGroupTreeNode | undefined;
  rowCount: number;
}

export class GroupItem implements IGroupTreeNode {
  constructor(data: IGroupItemData) {
    Object.assign(this, data);
  }
  childGroups: IGroupTreeNode[] = null as any;
  childRows: any[][] = null as any;
  columnId: string = null as any;
  columnValue: string = null as any;
  groupLabel: string = null as any;
  parent: IGroupTreeNode | undefined = null as any;
  rowCount: number = null as any;

  @observable isExpanded = false;
}