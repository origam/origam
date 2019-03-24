import { IRecords } from "./types/IRecords";
import { IRecord } from "./types/IRecord";
import { IRecordId } from "../values/types/IRecordId";
import { computed, observable, action } from "mobx";

export class Records implements IRecords {

  items: IRecord[] = [];
  @observable dirtyDeletedRecords: Map<string, IRecord> = new Map();

  @computed get visibleItems() {
    return this.items.filter(item => !this.dirtyDeletedRecords.has(item.id));
  }

  @computed
  get visibleCount(): number {
    return this.visibleItems.length;
  }

  byId(id: IRecordId): IRecord | undefined {
    return this.items.find(item => item.id === id);
  }

  byIndex(idx: number): IRecord | undefined {
    return this.visibleItems[idx];
  }

  getIndex(record: IRecord): number | undefined {
    const index = this.visibleItems.findIndex(item => item.id === record.id);
    return index > -1 ? index : undefined;
  }

  index2Id(idx: number): IRecordId | undefined {
    const record = this.byIndex(idx);
    return record ? record.id : undefined;
  }

  id2Index(id: IRecordId): number | undefined {
    const record = this.byId(id);
    return record ? this.getIndex(record) : undefined;
  }

  @action.bound
  deleteRecord(record: IRecord): void {
    this.dirtyDeletedRecords.set(record.id, record);
  }
}
