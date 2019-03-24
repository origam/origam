import { IRecord } from "./types/IRecord";
import { ICellValue } from "./types/ICellValue";
import { action, observable } from "mobx";
import { IPropertyId } from "../values/types/IPropertyId";

interface IRecordParam {
  id: string;
  values: ICellValue[];
  dirtyValues: Map<string, any>;
}

export class Record implements IRecord {
  constructor(param: IRecordParam) {
    this.id = param.id;
    this.values = param.values;
    this.dirtyValues = param.dirtyValues;
  }

  id: string;
  @observable values: ICellValue[];
  @observable dirtyValues: Map<string, any>;
  @observable dirtyDeleted: boolean = false;

  getValueByIndex(idx: number) {
    return this.values[idx];
  }

  getDirtyValueByKey(key: string) {
    return this.dirtyValues.get(key);
  }

  hasDirtyValue(key: IPropertyId): boolean {
    return this.dirtyValues ? this.dirtyValues.has(key) : false;
  }

  @action.bound
  setValues(values: ICellValue[]): void {
    this.values = values;
  }

  @action.bound
  setDirtyValue(key: IPropertyId, value: ICellValue): void {
    if (!this.dirtyValues) {
      this.dirtyValues = new Map();
    }
    this.dirtyValues.set(key, value);
  }

  @action.bound setDirtyDeleted(state: boolean) {
    this.dirtyDeleted = state;
  }
}
