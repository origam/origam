import { action } from "mobx";
import {
  IDataTableState,
  IDataTableSelectors,
  IDataTableRecord,
  IDataTableField,
  IDataTableActions,
  IDataTableFieldStruct,
  IRecordId
} from "./types";
import { EventObserver } from "src/utils/events";

export class DataTableActions implements IDataTableActions {
  constructor(
    public state: IDataTableState,
    public selectors: IDataTableSelectors
  ) {}

  public onDataCommitted = EventObserver();

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
      console.log("inserting");
      this.onDataCommitted.trigger();
    } else if (beforeId) {
      const pivotIndex = this.selectors.getFullRecordIndexById(beforeId);
      this.state.records.splice(pivotIndex, 0, record);
      console.log("inserting");
      this.onDataCommitted.trigger();
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
      console.log("deleting");
      this.onDataCommitted.trigger();
    }
  }

  @action.bound
  public setDirtyCellValue(
    record: IDataTableRecord,
    field: IDataTableField,
    value: string
  ): void {
    if (field === "ID") {
      return;
    }
    record.setDirtyValue(field.id, value);
    this.onDataCommitted.trigger();
  }

  public putRecord(
    record: IDataTableRecord,
    where: { before?: IRecordId; after?: IRecordId }
  ): void {
    if (where.before) {
      const recordIndex = this.selectors.getFullRecordIndexById(where.before);
      this.state.records.splice(recordIndex, 0, record);
      this.onDataCommitted.trigger();
    } else if (where.after) {
      const recordIndex = this.selectors.getFullRecordIndexById(where.after);
      this.state.records.splice(recordIndex + 1, 0, record);
      this.onDataCommitted.trigger();
    } else {
      this.state.records.push(record);
    }
  }

  @action.bound
  public replaceUpdatedRecord(updatedRecord: IDataTableRecord): void {
    const index = this.selectors.getFullRecordIndexById(updatedRecord.id);
    if (index > -1) {
      this.state.records.splice(index, 1, updatedRecord);
    }
  }

  @action.bound
  public replaceCreatedRecord(createdRecord: IDataTableRecord): void {
    const index = this.selectors.getFullRecordIndexById(createdRecord.id);
    if (index > -1) {
      this.state.records.splice(index, 1, createdRecord);
    }
  }

  @action.bound
  public deleteDeletedRecord(recordId: string): void {
    const index = this.selectors.getFullRecordIndexById(recordId);
    if (index > -1) {
      this.state.records.splice(index, 1);
    }
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
