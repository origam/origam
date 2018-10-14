import { computed } from "mobx";
import * as uuid from "uuid";

import {
  IDataTableState,
  ILookupResolverProvider,
  IDataTableSelectors,
  IDataTableRecord,
  IDataTableField,
  IDataTableFieldStruct,
  ICellValue,
  IFieldType
} from "./types";
import { DataTableRecord } from "./DataTableState";

export class DataTableSelectors implements IDataTableSelectors {
  constructor(
    public state: IDataTableState,
    public lookupResolverProvider: ILookupResolverProvider,
    public tableId: string
  ) {}

  @computed
  get records() {
    return this.state.records.filter(o => !o.dirtyDeleted);
  }

  @computed
  get recordCount() {
    return this.records.length;
  }

  @computed
  get fullRecords() {
    return this.state.records;
  }

  @computed
  get fullRecordCount() {
    return this.fullRecords.length;
  }

  @computed
  get fields() {
    return this.state.fields;
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

  public getRecordById(recordId: string): IDataTableRecord | undefined {
    // TODO: Use mapping id => array index to speedup this lookup
    return this.fullRecords.find(o => o.id === recordId);
  }

  public getRecordByRecordIndex(idx: number) {
    return this.records[idx];
  }

  public getFieldById(fieldId: string): IDataTableField | undefined {
    if (fieldId === "id") {
      return "ID";
    }
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
    if (field === "ID") {
      return record.id;
    }
    return record.values[field.dataIndex];
  }

  public getOriginalValue(
    record: IDataTableRecord,
    field: IDataTableField
  ): ICellValue | undefined {
    if (field === "ID") {
      return record.id;
    }
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
      field.lookupResultTableId &&
      field.lookupResultFieldId
    ) {
      value = this.lookupResolverProvider
        .get(field.lookupResultTableId, field.lookupResultFieldId)
        .getLookedUpValue(value);
    }
    return value;
  }

  public getDefaultValue(field: IDataTableFieldStruct) {
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
    const record = new DataTableRecord(
      uuid.v4(),
      this.fields.map(o => this.getDefaultValue(o))
    );
    record.dirtyNew = true;
    return record;
  }
}
