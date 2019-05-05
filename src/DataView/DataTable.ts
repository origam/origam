import { action, observable, computed } from "mobx";
import { L, ML } from "../utils/types";
import { unpack } from "../utils/objects";

import { IProperty } from "./types/IProperty";
import { IRecords } from "./types/IRecords";
import { IProperties } from "./types/IProperties";
import { Property } from "./Property";
import { IDataTable } from "./types/IDataTable";
import { IRecord } from "./types/IRecord";




export class Properties implements IProperties {
  
  constructor(public P: { items: ML<IProperty[]> }) {}

  @observable.shallow items: IProperty[] = unpack(this.P.items);

  @computed get count(): number {
    return this.items.length;
  }

  @computed get ids(): string[] {
    return this.items.map(item => item.id);
  }

  @computed get itemsById() {
    return new Map(
      this.items.map(item => [item.id, item] as [string, IProperty])
    );
  }

  getByIndex(idx: number): IProperty | undefined {
    return this.items[idx];
  }

  getById(id: string): IProperty | undefined {
    return this.items.find(item => item.id === id);
  }

  getIdByIndex(idx: number): string | undefined {
    const prop = this.getByIndex(idx);
    return prop ? prop.id : undefined;
  }

  getIndexById(id: string): number | undefined {
    const idx = this.items.findIndex(item => item.id === id);
    return idx > -1 ? idx : undefined;
  }

  getIdAfterId(id: string): string | undefined {
    const idx = this.getIndexById(id);
    const newIdx = idx !== undefined ? idx + 1 : undefined;
    const newId = newIdx !== undefined ? this.getIdByIndex(newIdx) : undefined;
    return newId;
  }

  getIdBeforeId(id: string): string | undefined {
    const idx = this.getIndexById(id);
    const newIdx = idx !== undefined ? idx - 1 : undefined;
    const newId = newIdx !== undefined ? this.getIdByIndex(newIdx) : undefined;
    return newId;
  }
}

export class Records implements IRecords {

  @observable.shallow items: Array<Array<any>> = [];
  @observable deletedRecordIds: Map<string, boolean> | undefined;

  @computed get count(): number {
    return this.items.length;
  }

  @computed get existingCount(): number {
    return this.existingItems.length;
  }

  @computed get existingItems() {
    const { deletedRecordIds } = this;
    if (!deletedRecordIds) {
      return this.items;
    }
    return this.items.filter(item => !deletedRecordIds.has(item[0]));
  }

  @action.bound markDeleted(recId: string) {
    if (!this.deletedRecordIds) {
      this.deletedRecordIds = new Map();
    }
    this.deletedRecordIds.set(recId, true);
  }

  @action.bound
  setRecords(records: IRecord[]): void {
    this.items = records;
  }

  getByIndex(idx: number): IRecord | undefined {
    return this.existingItems[idx];
  }

  getById(id: string): IRecord | undefined {
    // TODO: Not finding by id...?
    return this.items.find(item => item[0] === id);
  }

  getIdByIndex(idx: number): string | undefined {
    const rec = this.getByIndex(idx);
    return rec ? rec[0] : undefined;
  }

  getIndexById(id: string): number | undefined {
    const idx = this.existingItems.findIndex(item => item[0] === id);
    return idx > -1 ? idx : undefined;
  }

  getIdAfterId(id: string): string | undefined {
    const idx = this.getIndexById(id);
    const newIdx = idx !== undefined ? idx + 1 : undefined;
    const newId = newIdx !== undefined ? this.getIdByIndex(newIdx) : undefined;
    return newId;
  }

  getIdBeforeId(id: string): string | undefined {
    const idx = this.getIndexById(id);
    const newIdx = idx !== undefined ? idx - 1 : undefined;
    const newId = newIdx !== undefined ? this.getIdByIndex(newIdx) : undefined;
    return newId;
  }
}

export class DataTable implements IDataTable {



  constructor(
    public P: {
      records: L<IRecords>;
      properties: L<IProperties>;
    }
  ) {}

  @computed get existingRecordCount(): number {
    return this.records.existingCount;
  }

  @computed get propertyCount(): number {
    return this.properties.count;
  }

  @observable dirtyValues:
    | Map<string, Map<string, any>>
    | undefined = undefined;

  @observable newRecordIds: Map<string, boolean> | undefined;

  @action.bound
  setRecords(records: IRecord[]): void {
    this.records.setRecords(records);
  }

  @action.bound
  resetDirty(): void {
    this.dirtyValues = undefined;
  }

  @action.bound
  addDirtyValues(recId: string, values: Map<string, any>) {
    if (!this.dirtyValues) {
      this.dirtyValues = new Map();
    }
    if (!this.dirtyValues.get(recId)) {
      this.dirtyValues.set(recId, new Map());
    }
    for (let [propId, value] of values) {
      this.dirtyValues.get(recId)!.set(propId, value);
    }
  }

  getRecordByIdx(idx: number): IRecord | undefined {
    return this.records.getByIndex(idx);
  }

  getRecordById(id: string): IRecord | undefined {
    return this.records.getById(id);
  }


  getValueByIdx(recIdx: number, propIdx: number): any {
    const record = this.getRecordByIdx(recIdx);
    const property = this.properties.getByIndex(propIdx);
    if (record && property) {
      return this.getValue(record, property);
    }
  }

  getValueById(recId: string, propId: string): any {
    const record = this.records.getById(recId);
    const property = this.properties.getById(propId);
    if (record && property) {
      return this.getValue(record, property);
    }
  }

  getValue(record: IRecord, property: IProperty) {
    if (
      this.dirtyValues &&
      this.dirtyValues.has(record[0]) &&
      this.dirtyValues.get(record[0])!.has(property.id)
    ) {
      return this.dirtyValues.get(record[0])!.get(property.id);
    }
    return record[property.dataIndex];
  }


  getRecValueMap(id: string): Map<string, any> {
    const record = this.records.getById(id);
    const result = new Map();
    if (record) {
      for (let prop of this.properties.items) {
        result.set(prop.id, this.getValue(record, prop));
      }
    }
    return result;
  }

  getRecordIndexById(id: string): number | undefined {
    return this.records.getIndexById(id);
  }

  @computed get records() {
    return this.P.records();
  }

  @computed get properties() {
    return this.P.properties();
  }
}
