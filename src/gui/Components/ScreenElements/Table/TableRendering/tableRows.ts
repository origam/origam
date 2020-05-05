import { IGroupTreeNode, IGroupRow, ITableRow } from "./types";

export class TableGroupRow implements IGroupRow {
  constructor(public groupLevel: number, public sourceGroup: IGroupTreeNode) {}
  parent: IGroupRow | undefined;
  get columnLabel(): string {
    return this.sourceGroup.columnLabel;
  }
  get columnValue(): string {
    return this.sourceGroup.groupLabel;
  }
  get isExpanded(): boolean {
    return this.sourceGroup.isExpanded;
  }
}



export function flattenToTableRows(rootGroups: IGroupTreeNode[]) {
    const result: ITableRow[] = [];
    let level = 0;
    function recursive(group: IGroupTreeNode) {
      result.push(new TableGroupRow(level, group));
      if (!group.isExpanded) return;
      for (let g of group.childGroups) {
        level++;
        recursive(g);
        level--;
      }
      if(group.childGroups.length === 0){
          result.push(...group.childRows);
      }
    }
    for (let group of rootGroups) {
      recursive(group);
    }
    console.log(result)
    return result;
}
