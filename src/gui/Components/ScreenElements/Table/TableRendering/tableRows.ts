import { computed, IObservable, IObservableArray, IComputedValue } from "mobx";

import { IGroupItem, IGroupRow, ITableRow } from "./types";

export class TableGroupRow implements IGroupRow {
  constructor(public groupLevel: number, public sourceGroup: IGroupItem) {}
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



export function tableRows(rootGroups: IComputedValue<IGroupItem[]>) {
  return computed<ITableRow[]>(() => {
    const result: ITableRow[] = [];
    let level = 0;
    function recursive(group: IGroupItem) {
      result.push(new TableGroupRow(level, group));
      if (!group.isExpanded) return;
      for (let g of group.childGroups) {
        level++;
        recursive(g);
        level--;
      }
      result.push(...group.childRows);
    }
    for (let group of rootGroups.get()) {
      recursive(group);
    }
    console.log(result)
    return result;
  });
}
