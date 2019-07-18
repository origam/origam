export interface IScreenLifecycleData {}

export interface IScreenLifecycle extends IScreenLifecycleData {
  $type_IScreenLifecycle: 1;

  parent?: any;
}

export const isIScreenLifecycle = (o: any): o is IScreenLifecycle =>
  o.$type_IScreenLifecycle;
