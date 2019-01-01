import { action } from "mobx";
import {
  IDataTableRecord,
  IRecordId,
  IDataTableActions,
  IDataTableSelectors
} from "src/DataTable/types";
import axios from "axios";
import { getToken } from "./api";
import { IDataLoader, IDataSaver } from "./types";
import { DataTableRecord } from "src/DataTable/DataTableRecord";

export class DataSaver implements IDataSaver {
  constructor(
    public tableName: string,
    public dataTableActions: IDataTableActions,
    public dataTableSelectors: IDataTableSelectors,
    public menuItemId: string
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
        },
        menuId: this.menuItemId
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
        rowIdToDelete: recordId,
        menuId: this.menuItemId
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
        },
        menuId: this.menuItemId
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
