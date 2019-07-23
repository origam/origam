export interface IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1;

  run(): void;
  parent?: any;
}

export const isIWorkbenchLifecycle = (o: any): o is IWorkbenchLifecycle =>
  o.$type_IWorkbenchLifecycle;
