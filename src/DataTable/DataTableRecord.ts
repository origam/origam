import { observable, action } from "mobx";
import { ICellValue } from "./types";
import { IDataTableRecord } from "./types2";


export class DataTableRecord implements IDataTableRecord {

  @observable.ref
  public values: ICellValue[] = [];

  @observable
  public dirtyValues: Map<string, ICellValue> | undefined;

  @observable
  public dirtyNew: boolean = false;

  @observable
  public dirtyDeleted = false;

  constructor(public id: string, values: ICellValue[]) {
    this.values = values;
  }

  public get isDirtyChanged(): boolean {
    return this.dirtyValues !== undefined && this.dirtyValues.size > 0
  }

  public get isDirtyDeleted(): boolean {
    return this.dirtyDeleted;
  }

  public get isDirtyNew(): boolean {
    return this.dirtyNew;
  }

  @action.bound
  public setDirtyDeleted(state: boolean) {
    this.dirtyDeleted = state;
  }

  @action.bound
  public setDirtyValue(fieldId: string, value: string) {
    if (!this.dirtyValues) {
      this.dirtyValues = new Map();
    }
    this.dirtyValues.set(fieldId, value);
  }

  @action.bound
  public setDirtyNew(state: boolean) {
    this.dirtyNew = state;
  }
}