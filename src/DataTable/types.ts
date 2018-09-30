export type ICellValue = string;
export type IFieldId = string;
export type ITableId = string;

export interface IDataTableState {
  records: IDataTableRecord[];
  fields: IDataTableField[];
}

export interface IDataTableSelectors {
  tableId: ITableId;
  fullRecords: IDataTableRecord[];
  fullRecordCount: number;
  fieldCount: number;
  lastFullRecord: IDataTableRecord;
  firstFullRecord: IDataTableRecord;

  getFullRecordIndexById(afterId: string): number;
  getRecordById(recordId: string): IDataTableRecord | undefined;
  getFieldById(fieldId: string): IDataTableField | undefined | "ID";
  getResetValue(
    record: IDataTableRecord,
    field: IDataTableField | "ID"
  ): ICellValue;
}

export interface IDataTableActions {
  appendRecords(records: IDataTableRecord[]): void;
  prependRecords(records: IDataTableRecord[]): void;
  setRecords(records: IDataTableRecord[]): void;
  trimTail(cnt: number): void;
  trimHead(cnt: number): void;
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

export interface IDataTableField {
  id: string;
  label: string;
  dataIndex: number;
  isLookedUp: boolean;
}

export interface ILookupResolver {
  getLookedUpValue(value: ICellValue | undefined): string;
}

export interface ILookupResolverProvider {
  get(tableId: ITableId, fieldId: IFieldId): ILookupResolver;
}
