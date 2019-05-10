import { IRecord } from "./IRecord";

export interface IRecords {
  count: number;
  existingCount: number;
  getByIndex(idx: number): IRecord | undefined;
  getById(id: string): IRecord | undefined;
  getIdByIndex(idx: number): string | undefined;
  getIndexById(id: string): number | undefined;
  getIdAfterId(id: string): string | undefined;
  getIdBeforeId(id: string): string | undefined;
  setRecords(records: IRecord[]): void;
  substRecord(rowId: string, record: IRecord): void;
  markDeleted(rowId: string): void;
  removeDirtyDeleted(rowId: string): void;
  removeRow(rowId: string): void;
  deletedRecordIds: Map<string, boolean> | undefined;
}
