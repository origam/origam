export interface IDataViewLifecycleData {}

export interface IDataViewLifecycle extends IDataViewLifecycleData {
  $type_IDataViewLifecycle: 1;
  isWorking: boolean;
  navigateAsChild(): Generator;
  start(): void;
  parent?: any;
}

export const isIDataViewLifecycle = (o: any): o is IDataViewLifecycle =>
  o.$type_IDataViewLifecycle;
