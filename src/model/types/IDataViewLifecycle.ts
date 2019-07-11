import { IProperty } from './IProperty';
export const CDataViewLifecycle = "CDataViewLifecycle";

export interface IDataViewLifecycleData {}

export interface IDataViewLifecycle extends IDataViewLifecycleData {
  $type: typeof CDataViewLifecycle;

  isWorking: boolean;

  loadFresh(): void;
  requestFlushData(row: any[], property: IProperty): void;
  run(): void;
  parent?: any;
}
