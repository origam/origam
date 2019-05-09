import { IRecords } from "./IRecords";
import { IProperties } from "./IProperties";
import { IRecord } from "./IRecord";
import { IProperty } from "./IProperty";

export interface IDataTable {
  existingRecordCount: number;
  propertyCount: number;
  dirtyValues: Map<string, Map<string, any>>;
  newRecordIds: Map<string, boolean> | undefined;
  dirtyDeletedIds: Map<String, boolean>;
  getRecordByIdx(idx: number): IRecord | undefined;
  getRecordById(id: string): IRecord | undefined;
  addDirtyValues(recId: string, values: Map<string, any>): void;
  getValueByIdx(recIdx: number, propIdx: number): any;
  getValueById(recId: string, propId: string): any;
  getValue(record: IRecord, property: IProperty): any;
  getRecValueMap(id: string): Map<string, any>;
  getRecordIndexById(id: string): number | undefined;
  getRecordIdByIndex(idx: number): string | undefined;
  setRecords(records: IRecord[]): void;
  resetDirty(): void;
  removeDirtyRow(rowId: string): void;
  mutateRow(rowId: string, values: { [key: string]: any }): void;
  substRecord(rowId: string, record: IRecord): void;
  hasContent: boolean;
  records: IRecords;
  properties: IProperties
}
