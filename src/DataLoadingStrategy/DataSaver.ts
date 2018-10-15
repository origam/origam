import { action } from "mobx";
import {
  IDataTableRecord,
  IRecordId,
  IDataTableActions,
  IDataTableSelectors
} from "src/DataTable/types";
import axios from "axios";
import { DataTableRecord } from "src/DataTable/DataTableState";



export class DataSaver {
  constructor(
    public tableName: string,
    public dataTableActions: IDataTableActions,
    public dataTableSelectors: IDataTableSelectors
  ) {
    return;
  }

  @action.bound
  public updateRecords(
    records: IDataTableRecord[]
  ): Promise<IDataTableRecord[]> {
    return Promise.all(
      records.map(record => {
        return axios
          .put(`http://127.0.0.1:8080/api/${this.tableName}/${record.id}`, {
            ...this.serializeRecord(record)
          })
          .then(result => {
            const updatedRecord = this.deserializeRecord(result.data.record);
            // TODO: What if server returns nothing?
            this.dataTableActions.replaceUpdatedRecord(updatedRecord);
            return updatedRecord;
          });
      })
    );
  }

  @action.bound
  public deleteRecords(recordIds: IRecordId[]): Promise<any> {
    return Promise.all(recordIds.map(recordId => {
      return axios.delete(`http://127.0.0.1:8080/api/${this.tableName}/${recordId}`)
      .then(result => {
        this.dataTableActions.deleteDeletedRecord(recordId);
      })
    }))
  }

  @action.bound
  public createRecords(records: IDataTableRecord[]): Promise<any> {
    return Promise.all(records.map(record => {
      return axios.post(`http://127.0.0.1:8080/api/${this.tableName}`, {
        ...this.serializeRecord(record)
      })
      .then(result => {
        const createdRecord = this.deserializeRecord(result.data.record);
        // TODO: What if server returns nothing?
        this.dataTableActions.replaceCreatedRecord(createdRecord);
      })
    }))
  }

  public serializeRecord(record: IDataTableRecord): any {
    const result = {};
    for (const field of this.dataTableSelectors.fields) {
      result[field.id] = this.dataTableSelectors.getOriginalValue(
        record,
        field
      );
    }
    const ID = "id";
    result[ID] = record.id;
    return result;
  }

  public deserializeRecord(data: any): DataTableRecord {
    const values = new Array(this.dataTableSelectors.fields.length);
    for (const field of this.dataTableSelectors.fields) {
      values[field.dataIndex] = data[field.id];
    }
    const record = new DataTableRecord(data.id, values);
    return record;
  }
}
