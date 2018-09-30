import { observable, action } from "mobx";
import { IDataTableState, IDataTableField, IDataTableRecord } from "./types";

export class DataTableRecord implements IDataTableRecord {
  @observable.ref
  public values: any[] = [];

  @observable
  public dirtyValues: Map<string, any> | undefined;

  @observable
  public dirtyNew: boolean = false;

  @observable
  public dirtyDeleted = false;

  constructor(public id: string, values: any[]) {
    this.values = values;
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

export class DataTableField implements IDataTableField {
  @observable
  public label: string;

  @observable
  public dataIndex: number;

  constructor(
    public id: string,
    label: string,
    dataIndex: number,
    public isLookedUp: boolean
  ) {
    Object.assign(this, { label, dataIndex });
  }
}

export class DataTableState implements IDataTableState {
  @observable
  public records: DataTableRecord[] = [];

  @observable
  public fields: DataTableField[] = [];
}
