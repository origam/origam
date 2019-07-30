import { IOpenedScreen } from "./IOpenedScreen";

export interface IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1;

  onMainMenuItemClick(args: { event: any; item: any }): void;
  onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen): void;
  onScreenTabCloseClick(event: any, openedScreen: IOpenedScreen): void;
  
  run(): void;
  parent?: any;
}

export const isIWorkbenchLifecycle = (o: any): o is IWorkbenchLifecycle =>
  o.$type_IWorkbenchLifecycle;
