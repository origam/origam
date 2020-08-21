export interface IDataViewLifecycleData {}

export interface IDataViewLifecycle extends IDataViewLifecycleData {
  $type_IDataViewLifecycle: 1;
  isWorking: boolean;
  changeMasterRow(): Generator;
  navigateChildren(): Generator;
  navigateAsChild(): Generator;
  start(): void;

  startSelectedRowReaction(fireImmediatelly?: boolean): void;
  stopSelectedRowReaction(): void;
  runRecordChangedReaction(action: ()=>Generator): any;

  parent?: any;
}

export const isIDataViewLifecycle = (o: any): o is IDataViewLifecycle =>
  o.$type_IDataViewLifecycle;
