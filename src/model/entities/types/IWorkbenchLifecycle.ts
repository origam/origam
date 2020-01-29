import { IOpenedScreen, IDialogInfo } from "./IOpenedScreen";
import { IMainMenuItemType } from "./IMainMenu";

export interface IWorkbenchLifecycle {
  $type_IWorkbenchLifecycle: 1;

  onMainMenuItemClick(args: { event: any; item: any }): Generator;
  onWorkQueueListItemClick(event: any, item: any): Generator;
  onScreenTabHandleClick(event: any, openedScreen: IOpenedScreen): Generator;

  openNewForm(
    id: string,
    type: IMainMenuItemType,
    label: string,
    dontRequestData: boolean,
    dialogInfo: IDialogInfo | undefined,
    parameters: { [key: string]: any }
  ): Generator;

  openNewUrl(url: string, title: string): Generator;

  closeForm(openedScreen: IOpenedScreen): Generator;

  run(): Generator;
  parent?: any;
}

export const isIWorkbenchLifecycle = (o: any): o is IWorkbenchLifecycle =>
  o.$type_IWorkbenchLifecycle;
