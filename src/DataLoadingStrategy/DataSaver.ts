import { action } from "mobx";
import { IDataTableRecord, IRecordId } from "src/DataTable/types";

export class DataSaver {
  constructor() {return}

  @action.bound public updateRecords(records: IDataTableRecord[]): Promise<IDataTableRecord[]> {
    throw new Error();
  }

  @action.bound public deleteRecords(recordIds: IRecordId[]) {
    throw new Error();
  }

  @action.bound public createRecords(records: IDataTableRecord[]) {
    throw new Error();
  }
}