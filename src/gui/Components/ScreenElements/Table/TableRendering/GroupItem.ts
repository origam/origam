
import { observable } from "mobx";
import { IGroupItem } from "./types";

export class GroupItem implements IGroupItem {
  constructor(
    public childGroups: GroupItem[],
    public childRows: any[][],
    public columnLabel: string,
    public groupLabel: string
  ) {}

  @observable isExpanded = true;
}