import { action } from "mobx";
import {
  IDataTableState,
  IDataTableSelectors,
  IDataTableRecord,
  IDataTableField,
  IDataTableActions,
  IDataTableFieldStruct
} from "./types";

export class DataTableActions implements IDataTableActions {

  constructor(
    public state: IDataTableState,
    public selectors: IDataTableSelectors
  ) {}

  @action.bound
  public insertRecord({
    afterId,
    beforeId,
    record
  }: {
    afterId?: string;
    beforeId?: string;
    record: IDataTableRecord;
  }) {
    if (afterId) {
      const pivotIndex = this.selectors.getFullRecordIndexById(afterId);
      this.state.records.splice(pivotIndex + 1, 0, record);
    } else if (beforeId) {
      const pivotIndex = this.selectors.getFullRecordIndexById(beforeId);
      this.state.records.splice(pivotIndex, 0, record);
    } else {
      throw new Error("Before or after...?");
    }
  }

  @action.bound
  public deleteRecord(record: IDataTableRecord) {
    const recordIndex = this.selectors.getFullRecordIndexById(record.id);
    if (record.dirtyNew) {
      this.state.records.splice(recordIndex, 1);
    } else {
      record.setDirtyDeleted(true);
    }
  }

  @action.bound
  public setDirtyCellValue(record: IDataTableRecord, field: IDataTableField, value: string): void {
    if(field === 'ID') {
      return;
    }
    record.setDirtyValue(field.id, value);
  }

  public putNewRecord(record: IDataTableRecord): void {
    throw new Error("Method not implemented.");
  }

  @action.bound
  public setDirtyValue(recordId: string, fieldId: string, value: string) {
    const record = this.selectors.getRecordById(recordId);
    record!.setDirtyValue(fieldId, value);
  }

  @action.bound
  public setRecords(records: IDataTableRecord[]) {
    this.state.records = records;
  }

  @action.bound
  public appendRecords(records: IDataTableRecord[]) {
    this.state.records.push(...records);
  }

  @action.bound
  public prependRecords(records: IDataTableRecord[]) {
    this.state.records.unshift(...records);
  }

  @action.bound
  public setFields(fields: IDataTableFieldStruct[]) {
    this.state.fields = fields;
  }

  @action.bound
  public trimHead(deleteCount: number) {
    this.state.records.splice(0, deleteCount);
  }

  @action.bound
  public trimTail(deleteCount: number) {
    this.state.records.splice(-deleteCount, deleteCount);
  }
}
