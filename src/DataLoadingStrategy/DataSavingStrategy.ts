import { IDataTableSelectors, IDataTableActions } from "src/DataTable/types";
import { action } from "mobx";
import { IDataSaver, IDataLoader, IDataLoadingStrategyActions } from "./types";

export class DataSavingStrategy {
  constructor(
    public dataTableSelectors: IDataTableSelectors,
    public dataTableActions: IDataTableActions,
    public dataSaver: IDataSaver,
    public dataLoadingStrategyActions: IDataLoadingStrategyActions
  ) {
    this.dataTableActions.onDataCommitted(this.handleDataTableCommit);
  }

  private isFlowRunning = false;
  private flowNeedsRerun = false;

  @action.bound
  public async handleDataTableCommit() {
    if (this.isFlowRunning) {
      this.flowNeedsRerun = true;
      return;
    }
    do {
      try {
        this.flowNeedsRerun = false;
        this.isFlowRunning = true;
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

        for (const record of toDelete) {
          await this.dataSaver.deleteRecord(record.id);
          this.dataTableActions.deleteDeletedRecord(record.id);
        }

        for (const record of toCreate) {
          await this.dataSaver.createRecord(record);
          await this.dataLoadingStrategyActions.reloadRow(record.id);
        }

        for (const record of toModify) {
          await this.dataSaver.updateRecord(record);
          await this.dataLoadingStrategyActions.reloadRow(record.id);
        }
      } finally {
        this.isFlowRunning = false;
      }
    } while (this.flowNeedsRerun);
  }
}
