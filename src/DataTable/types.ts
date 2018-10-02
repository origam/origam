export type ICellValue = string;
export type IFieldId = string;
export type IRecordId = string;
export type ITableId = string;

export interface IDataTableState {
  records: IDataTableRecord[];
  fields: IDataTableFieldStruct[];
}

export interface IDataTableSelectors {
  tableId: ITableId;
  fullRecords: IDataTableRecord[];
  fullRecordCount: number;
  recordCount: number;
  fieldCount: number;
  lastFullRecord: IDataTableRecord;
  firstFullRecord: IDataTableRecord;

  getFullRecordIndexById(afterId: string): number;
  getRecordById(recordId: string): IDataTableRecord | undefined;
  getFieldById(fieldId: string): IDataTableField | undefined;
  getRecordByRecordIndex(recordIndex: number): IDataTableRecord | undefined;
  getFieldByFieldIndex(fieldIndex: number): IDataTableFieldStruct | undefined;
  getRecordIndexById(recordId: IRecordId): number;
  getFieldIndexById(columnId: IFieldId): number;
  getValue(
    record: IDataTableRecord | undefined,
    field: IDataTableField | undefined
  ): ICellValue | undefined;
  getResetValue(
    record: IDataTableRecord,
    field: IDataTableField
  ): ICellValue;
}

export interface IDataTableActions {
  appendRecords(records: IDataTableRecord[]): void;
  prependRecords(records: IDataTableRecord[]): void;
  setRecords(records: IDataTableRecord[]): void;
  trimTail(cnt: number): void;
  trimHead(cnt: number): void;
  setDirtyCellValue(record: IDataTableRecord, field: IDataTableField, value: ICellValue): void;
  deleteRecord(record: IDataTableRecord): void;
  putNewRecord(record: IDataTableRecord): void;
}

export interface IDataTableRecord {
  values: ICellValue[];
  dirtyValues: Map<string, ICellValue> | undefined;
  dirtyNew: boolean;
  dirtyDeleted: boolean;
  id: string;

  setDirtyDeleted(state: boolean): void;
  setDirtyValue(fieldId: string, value: ICellValue): void;
  setDirtyNew(state: boolean): void;
}

export type IDataTableField = "ID" | IDataTableFieldStruct;

export interface IDataTableFieldStruct {
  id: string;
  label: string;
  dataIndex: number;
  isLookedUp: boolean;
  lookupResultFieldId?: IFieldId;
  lookupResultTableId?: ITableId;
}

export interface ILookupResolver {
  getLookedUpValue(value: ICellValue | undefined): string;
}

export interface ILookupResolverProvider {
  get(tableId: ITableId, fieldId: IFieldId): ILookupResolver;
}
