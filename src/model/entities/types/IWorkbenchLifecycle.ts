import { IOpenedScreen, IDialogInfo } from "./IOpenedScreen";
import { IMainMenuItemType } from "./IMainMenu";

export interface IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1;

  onMainMenuItemClick(args: { event: any; item: any }): void;
  onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen): void;
  onScreenTabCloseClick(event: any, openedScreen: IOpenedScreen): void;

  openNewForm(
    id: string,
    type: IMainMenuItemType,
    label: string,
    dontRequestData: boolean,
    dialogInfo: IDialogInfo | undefined,
    parameters: { [key: string]: any }
  ): void;
  closeForm(openedScreen: IOpenedScreen): void;

  run(): void;
  parent?: any;
}

export const isIWorkbenchLifecycle = (o: any): o is IWorkbenchLifecycle =>
  o.$type_IWorkbenchLifecycle;
