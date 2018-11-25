import { action } from "mobx";
import {
  IDataTableRecord,
  IRecordId,
  IDataTableActions,
  IDataTableSelectors
} from "src/DataTable/types";
import axios from "axios";
import { DataTableRecord } from "src/DataTable/DataTableState";
import { getToken } from "./api";
import { IDataLoader, IDataSaver } from "./types";

export class DataSaver implements IDataSaver {
  constructor(
    public tableName: string,
    public dataTableActions: IDataTableActions,
    public dataTableSelectors: IDataTableSelectors
  ) {
    return;
  }

  @action.bound
  public updateRecord(record: IDataTableRecord): Promise<any> {
    return axios.put(
      `/api/Data/Entities`,
      {
        dataStructureEntityId: this.tableName,
        rowId: record.id,
        newValues: {
          ...this.serializeRecord(record)
        }
      },
      {
        headers: { Authorization: `Bearer ${getToken()}` }
      }
    );
  }

  @action.bound
  public deleteRecord(recordId: IRecordId): Promise<any> {
    return axios.post(
      `/api/Data/EntityDelete`,
      {
        dataStructureEntityId: this.tableName,
        rowIdToDelete: recordId
      },
      {
        headers: { Authorization: `Bearer ${getToken()}` }
      }
    );
  }

  @action.bound
  public createRecord(record: IDataTableRecord): Promise<any> {
    return axios.post(
      `/api/Data/Entities`,
      {
        dataStructureEntityId: this.tableName,
        newValues: {
          ...this.serializeRecord(record)
        }
      },
      {
        headers: { Authorization: `Bearer ${getToken()}` }
      }
    );
  }

  public serializeRecord(record: IDataTableRecord): any {
    const result = {};
    for (const field of this.dataTableSelectors.fields) {
      result[field.id] = this.dataTableSelectors.getOriginalValue(
        record,
        field
      );
    }
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
