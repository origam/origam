import { observable, action } from "mobx";
import {
  IDataTableState,
  IDataTableField,
  IDataTableRecord,
  IFieldId,
  ITableId,
  IDataTableFieldStruct,
  IFieldType
} from "./types";

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

export class DataTableField implements IDataTableFieldStruct {
  @observable
  public formOrder: number;

  @observable 
  public gridVisible: boolean;
  
  @observable
  public formVisible: boolean;
  
  public type: IFieldType;
  
  public lookupResultTableId?: string | undefined;

  @observable
  public label: string;

  @observable
  public dataIndex: number;

  public id: IFieldId;

  public isLookedUp: boolean;

  public lookupResultFieldId: IFieldId;

  constructor({
    id,
    label,
    type,
    dataIndex,
    isLookedUp,
    lookupResultFieldId,
    lookupResultTableId
  }: {
    id: IFieldId;
    label: string;
    type: IFieldType;
    dataIndex: number;
    isLookedUp: boolean;
    lookupResultFieldId?: IFieldId;
    lookupResultTableId?: ITableId;
  }) {
    Object.assign(this, {
      id,
      label,
      type,
      dataIndex,
      isLookedUp,
      lookupResultFieldId,
      lookupResultTableId
    });
  }
}

export class DataTableState implements IDataTableState {
  @observable
  public records: DataTableRecord[] = [];

  @observable
  public fields: IDataTableFieldStruct[] = [];
}
