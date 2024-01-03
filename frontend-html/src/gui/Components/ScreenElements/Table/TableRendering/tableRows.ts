/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { IGroupRow, IGroupTreeNode, ITableRow } from "./types";

export class TableGroupRow implements IGroupRow {
  constructor(public groupLevel: number, public sourceGroup: IGroupTreeNode) {
  }

  parent: IGroupRow | undefined;

  get columnLabel(): string {
    return this.sourceGroup.groupLabel;
  }

  get columnValue(): string {
    return this.sourceGroup.getColumnDisplayValue();
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
    if (group.childGroups.length === 0) {
      result.push(...group.childRows);
    }
  }

  for (let group of rootGroups) {
    recursive(group);
  }
  return result;
}
