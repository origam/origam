import { IRecord } from "./IRecord";
import { IRecordId } from "../../values/types/IRecordId";

export interface IRecords {
  visibleCount: number;
  byId(id: IRecordId): IRecord | undefined;
  byIndex(idx: number): IRecord | undefined;
  getIndex(record: IRecord): number | undefined;
  index2Id(idx: number): IRecordId | undefined;
  id2Index(id: IRecordId): number | undefined;

  deleteRecord(record: IRecord): void;
}