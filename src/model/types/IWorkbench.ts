import { ILoadingMainMenu, IMainMenu } from "./IMainMenu";
import { IWorkbenchLifecycle } from "./IWorkbenchLifecycle";
export const CWorkbench = "CWorkbench";

export interface IWorkbenchData {
  workbenchLifecycle: IWorkbenchLifecycle;
}

export interface IWorkbench extends IWorkbenchData {
  $type: typeof CWorkbench;

  mainMenu?: ILoadingMainMenu | IMainMenu;

  run(): void;
  setMainMenu(mainMenu: IMainMenu | ILoadingMainMenu): void;

  parent?: any;
}
