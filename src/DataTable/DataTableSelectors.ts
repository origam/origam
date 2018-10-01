import { computed } from "mobx";
import {
  IDataTableState,
  ILookupResolverProvider,
  IDataTableSelectors,
  IDataTableRecord,
  IDataTableField
} from "./types";

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

  public getFieldById(fieldId: string): IDataTableField | undefined | "ID" {
    if (fieldId === "id") {
      return "ID";
    }
    return this.fields.find(o => o.id === fieldId);
  }

  public getFieldByFieldIndex(idx: number) {
    return this.fields[idx];
  }

  public getFullRecordIndexById(recordId: string) {
    return this.fullRecords.findIndex(o => o.id === recordId);
  }

  public getRecordIndexById(recordId: string) {
    return this.records.findIndex(o => o.id === recordId);
  }

  public getFieldIndexById(fieldId: string) {
    return this.fields.findIndex(o => o.id === fieldId);
  }

  public getResetValue(
    record: IDataTableRecord,
    field: IDataTableField | string
  ) {
    if (field === "ID") {
      return record.id;
    }
    return record.values[(field as IDataTableField).dataIndex];
  }

  public getValue(record: IDataTableRecord, field: IDataTableField) {
    let value;
    if (record.dirtyValues && record.dirtyValues.has(field.id)) {
      value = record.dirtyValues.get(field.id);
    } else {
      value = this.getResetValue(record, field);
    }
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
}
