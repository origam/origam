export const CDataViewLifecycle = "CDataViewLifecycle";

export interface IDataViewLifecycleData {}

export interface IDataViewLifecycle extends IDataViewLifecycleData {
  $type: typeof CDataViewLifecycle;

  isWorking: boolean;

  loadFresh(): void;
  run(): void;
  parent?: any;
}
