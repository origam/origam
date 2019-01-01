import { observable, action, computed } from "mobx";
import { IDataTableRecord, IDataTableField, IDataTable } from './types2';
import * as uuid from "uuid";
import { DataTableRecord } from "./DataTableRecord";
import { IFieldType, IDataTableFieldStruct, IRecordId, ILookupResolverProvider, ICellValue } from "./types";
import { EventObserver } from "src/utils/events";

export class DataTable implements IDataTable {
  constructor(
    public lookupResolverProvider: ILookupResolverProvider,
    public tableId: string
  ) {}

  @observable
  public sRecords: IDataTableRecord[] = [];

  @observable
  public sFields: IDataTableField[] = [];


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
      const pivotIndex = this.getFullRecordIndexById(afterId);
      this.records.splice(pivotIndex + 1, 0, record);
      console.log("inserting");
      this.onDataCommitted.trigger();
    } else if (beforeId) {
      const pivotIndex = this.getFullRecordIndexById(beforeId);
      this.records.splice(pivotIndex, 0, record);
      console.log("inserting");
      this.onDataCommitted.trigger();
    } else {
      throw new Error("Before or after...?");
    }
  }

  @action.bound
  public deleteRecord(record: IDataTableRecord) {
    const recordIndex = this.getFullRecordIndexById(record.id);
    if (record.dirtyNew) {
      this.records.splice(recordIndex, 1);
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
    record.setDirtyValue(field.id, value);
    this.onDataCommitted.trigger();
  }

  public putRecord(
    record: IDataTableRecord,
    where: { before?: IRecordId; after?: IRecordId }
  ): void {
    if (where.before) {
      const recordIndex = this.getFullRecordIndexById(where.before);
      this.records.splice(recordIndex, 0, record);
      this.onDataCommitted.trigger();
    } else if (where.after) {
      const recordIndex = this.getFullRecordIndexById(where.after);
      this.records.splice(recordIndex + 1, 0, record);
      this.onDataCommitted.trigger();
    } else {
      this.records.push(record);
    }
  }

  @action.bound
  public replaceUpdatedRecord(updatedRecord: IDataTableRecord): void {
    const index = this.getFullRecordIndexById(updatedRecord.id);
    if (index > -1) {
      this.records.splice(index, 1, updatedRecord);
    } else {
      this.records.push(updatedRecord)
    }
  }

  @action.bound
  public replaceCreatedRecord(createdRecord: IDataTableRecord): void {
    const index = this.getFullRecordIndexById(createdRecord.id);
    if (index > -1) {
      this.records.splice(index, 1, createdRecord);
    }
  }

  @action.bound
  public deleteDeletedRecord(recordId: string): void {
    const index = this.getFullRecordIndexById(recordId);
    if (index > -1) {
      this.records.splice(index, 1);
    }
  }

  @action.bound
  public setDirtyValue(recordId: string, fieldId: string, value: string) {
    const record = this.getRecordById(recordId);
    record!.setDirtyValue(fieldId, value);
  }

  @action.bound
  public setRecords(records: IDataTableRecord[]) {
    this.sRecords = records;
  }

  @action.bound
  public appendRecords(records: IDataTableRecord[]) {
    this.records.push(...records);
  }

  @action.bound
  public prependRecords(records: IDataTableRecord[]) {
    this.records.unshift(...records);
  }

  @action.bound
  public setFields(fields: IDataTableFieldStruct[]) {
    this.sFields = fields;
  }

  @action.bound
  public trimHead(deleteCount: number) {
    this.records.splice(0, deleteCount);
  }

  @action.bound
  public trimTail(deleteCount: number) {
    this.records.splice(-deleteCount, deleteCount);
  }

  @action.bound public clearAll() {
    this.records.length = 0;
  }

  @computed
  get records() {
    return this.sRecords.filter(o => !o.dirtyDeleted);
  }

  @computed
  get recordCount() {
    return this.records.length;
  }

  @computed
  get fullRecords() {
    return this.sRecords;
  }

  @computed
  get fullRecordCount() {
    return this.fullRecords.length;
  }

  @computed
  get fields() {
    return this.sFields;
  }

  @computed
  get fieldCount() {
    return this.fields.length;
  }

  @computed
  get firstFullRecord() {
    return this.fullRecords[0];
  }

  @computed
  get lastFullRecord() {
    return this.fullRecords.slice(-1)[0];
  }

  public recordExistsById(recordId: string): boolean {
    return this.getRecordById(recordId) !== undefined;
  }

  public fieldExistsById(fieldId: string): boolean {
    return this.getFieldById(fieldId) !== undefined;
  }

  public getRecordById(recordId: string): IDataTableRecord | undefined {
    // TODO: Use mapping id => array index to speedup this lookup
    return this.fullRecords.find(o => o.id === recordId);
  }

  public getRecordByRecordIndex(idx: number) {
    return this.records[idx];
  }

  public getFieldById(fieldId: string): IDataTableField | undefined {
    return this.fields.find(o => o.id === fieldId);
  }

  public getFieldByFieldIndex(idx: number) {
    if (idx >= this.fields.length || idx < 0) {
      return;
    }
    return this.fields[idx];
  }

  public getFullRecordIndexById(recordId: string) {
    return this.fullRecords.findIndex(o => o.id === recordId);
  }

  public getRecordIndexById(recordId: string) {
    return this.records.findIndex(o => o.id === recordId);
  }

  public getFieldIndexById(fieldId: string) {
    return this.fields.findIndex(o => (o as any).id === fieldId);
  }

  public getResetValue(record: IDataTableRecord, field: IDataTableField) {
    return record.values[field.dataIndex];
  }

  public getOriginalValue(
    record: IDataTableRecord,
    field: IDataTableField
  ): ICellValue | undefined {
    let value;
    if (record.dirtyValues && record.dirtyValues.has(field.id)) {
      value = record.dirtyValues.get(field.id);
    } else {
      value = this.getResetValue(record, field);
    }
    return value;
  }

  public getValue(record: IDataTableRecord, field: IDataTableFieldStruct) {
    let value = this.getOriginalValue(record, field);
    if (
      field.isLookedUp &&
      field.lookupId &&
      field.lookupIdentifier
    ) {
      value = this.lookupResolverProvider
        .get(field.lookupId)
        .getLookedUpValue(value);
    }
    return value;
  }

  public getDefaultValue(field: IDataTableFieldStruct, id: IRecordId) {
    if (field.isPrimaryKey) {
      return id;
    }
    switch (field.type) {
      case IFieldType.integer:
        return 0;
      case IFieldType.string:
      case IFieldType.color:
      case IFieldType.date:
        return "";
      case IFieldType.boolean:
        return 0;
      default:
        throw new Error("No field type given.");
    }
  }

  public newRecord(): IDataTableRecord {
    const id = uuid.v4();
    const record = new DataTableRecord(
      id,
      this.fields.map(o => this.getDefaultValue(o, id))
    );
    record.dirtyNew = true;
    return record;
  }
}