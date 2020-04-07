import { computed } from "mobx";
import { IColumnDriver, ITableViewInfo, IColumnDriverFactories } from "./types";

export class DataRowColumnDriversCollection {
  constructor(
    private tableViewInfo: ITableViewInfo,
    private columnDriverFactories: IColumnDriverFactories
  ) {}

  @computed get drivers() {
    const result: IColumnDriver[] = [];

    if (this.tableViewInfo.isCheckboxes) {
      result.push(this.columnDriverFactories.newCheckboxColumnDriver());
    }
    if (this.tableViewInfo.isGrouping) {
      for (let i = 0; i < this.tableViewInfo.groupingColumnsCount; i++) {
        const columnId = this.tableViewInfo.getGroupedColumnIdByLevel(i + 1);
        result.push(this.columnDriverFactories.newGroupHeaderColumnDriver(columnId));
      }
    }
    for (let i = 0; i < this.tableViewInfo.dataColumnsCount; i++) {
      const columnId = this.tableViewInfo.getDataColumnIdByIndex(i);
      result.push(this.columnDriverFactories.newDataColumnDriver(columnId));
    }

    return result;
  }
}
