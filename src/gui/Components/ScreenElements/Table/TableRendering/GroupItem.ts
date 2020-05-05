
import { observable } from "mobx";
import { IGroupTreeNode } from "./types";

export class GroupItem implements IGroupTreeNode {
  constructor(
    public childGroups: GroupItem[],
    public childRows: any[][],
    public columnLabel: string,
    public groupLabel: string,
    public rowCount: number,
    public parent: IGroupTreeNode | undefined,
    public columnValue: string
  ) {}

  @observable isExpanded = false;
}