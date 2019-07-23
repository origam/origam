export interface IFormScreenLifecycleData {}

export interface IFormScreenLifecycle extends IFormScreenLifecycleData {
  $type_IFormScreenLifecycle: 1;

  run(): void;
  parent?: any;
}

export const isIFormScreenLifecycle = (o: any): o is IFormScreenLifecycle =>
  o.$type_IFormScreenLifecycle;
