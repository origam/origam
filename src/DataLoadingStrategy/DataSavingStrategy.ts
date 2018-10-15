import { IDataTableSelectors, IDataTableActions } from "src/DataTable/types";
import { action } from "mobx";
import { IDataSaver } from "./types";

export class DataSavingStrategy {
  constructor(
    public dataTableSelectors: IDataTableSelectors,
    public dataTableActions: IDataTableActions,
    public dataSaver: IDataSaver
  ) {
    this.dataTableActions.onDataCommitted(this.handleDataTableCommit);
  }

  @action.bound
  public handleDataTableCommit() {
    console.log("Handling data table commit.");
    const toCreate = [];
    const toDelete = [];
    const toModify = [];
    for (const record of this.dataTableSelectors.fullRecords) {
      if (record.isDirtyDeleted) {
        toDelete.push(record);
      } else if (record.isDirtyNew) {
        toCreate.push(record);
      } else if (record.isDirtyChanged) {
        toModify.push(record);
      }
    }
    console.log("DELETE", toDelete);
    console.log("CREATE", toCreate);
    console.log("MODIFY", toModify);
    if (toDelete.length > 0) {
      this.dataSaver.deleteRecords(toDelete.map(record => record.id));
    }
    if (toCreate.length > 0) {
      this.dataSaver.createRecords(toCreate);
    }
    if (toModify.length > 0) {
      this.dataSaver.updateRecords(toModify);
    }
  }
}
