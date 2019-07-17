import { IProperty } from "./IProperty";

export interface IDataViewLifecycleData {}

export interface IDataViewLifecycle extends IDataViewLifecycleData {
  $type_IDataViewLifecycle: 1;

  isWorking: boolean;

  onDeleteRowClicked(): void;
  loadFresh(): void;
  requestFlushData(row: any[], property: IProperty): void;
  run(): void;
  parent?: any;
}

export const isIDataViewLifecycle = (o: any): o is IDataViewLifecycle =>
  o.$type_IDataViewLifecycle;
