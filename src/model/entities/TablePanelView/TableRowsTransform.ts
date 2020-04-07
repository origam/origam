import { IDataRow, IGroupHeader } from "gui/Components/ScreenElements/Table/CellDrivers/types";
import { computed } from "mobx";

export class TableRowsTransform {
  constructor(
    private getRootGroupHeaders: () => IGroupHeader[],
    private getDataRows: () => any[][]
  ) {}

  get isGrouping() {
    return false;
  }

  @computed
  get tableRows() {
    const result: IDataRow[] = [];

    function recursiveGroupHeader(header: IGroupHeader) {
      result.push(header);
      for (let childGroup of header.childGroups) {
        recursiveGroupHeader(childGroup);
      }
      for (let childRow of header.childRows) {
        recursiveRow(childRow);
      }
    }

    function recursiveRow(row: any[]) {
      result.push(row);
    }

    if (this.isGrouping) {
      for (let header of this.getRootGroupHeaders()) recursiveGroupHeader(header);
    } else {
      for (let row of this.getDataRows()) recursiveRow(row);
    }

    return result;
  }
}
