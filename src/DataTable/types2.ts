import { IFieldId, IFieldType, IDropdownColumn, ITableId, ILookupResolverProvider, IRecordId, ICellValue } from "./types";

export interface IDataTableRecord {
  values: ICellValue[];
  dirtyValues: Map<string, ICellValue> | undefined;
  dirtyNew: boolean;
  dirtyDeleted: boolean;
  id: string;
  isDirtyChanged: boolean;
  isDirtyDeleted: boolean;
  isDirtyNew: boolean;
  setDirtyDeleted(state: boolean): void;
  setDirtyValue(fieldId: string, value: ICellValue): void;
  setDirtyNew(state: boolean): void;
}

export interface IDataTableField {
  id: IFieldId;
  label: string;
  type: IFieldType;
  dataIndex: number;
  recvDataIndex: number;
  isLookedUp: boolean;
  isPrimaryKey: boolean;
  lookupId: string;
  lookupIdentifier: string;
  dropdownColumns: IDropdownColumn[];
  formOrder: number;
  gridVisible: boolean;
  formVisible: boolean;
}

export interface IDataTable {
  tableId: ITableId;
  fullRecords: IDataTableRecord[];
  fullRecordCount: number;
  recordCount: number;
  fieldCount: number;
  fields: IDataTableField[];
  lastFullRecord: IDataTableRecord;
  firstFullRecord: IDataTableRecord;
  lookupResolverProvider: ILookupResolverProvider;

  newRecord(): IDataTableRecord;
  recordExistsById(recordId: IRecordId): boolean;
  fieldExistsById(fieldId: IFieldId): boolean;
  getFullRecordIndexById(recordId: string): number;
  getRecordById(recordId: string): IDataTableRecord | undefined;
  getFieldById(fieldId: string): IDataTableField | undefined;
  getRecordByRecordIndex(recordIndex: number): IDataTableRecord | undefined;
  getFieldByFieldIndex(fieldIndex: number): IDataTableField | undefined;
  getRecordIndexById(recordId: IRecordId): number;
  getFieldIndexById(columnId: IFieldId): number;
  getValue(
    record: IDataTableRecord | undefined,
    field: IDataTableField | undefined
  ): ICellValue | undefined;
  getOriginalValue(
    record: IDataTableRecord,
    field: IDataTableField
  ): ICellValue | undefined;
  getResetValue(
    record: IDataTableRecord,
    field: IDataTableField
  ): ICellValue | undefined;
}
