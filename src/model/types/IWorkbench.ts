import { ILoadingMainMenu, IMainMenu } from "./IMainMenu";
export const CWorkbench = "CWorkbench";

export interface IWorkbenchData {}

export interface IWorkbench extends IWorkbenchData {
  CWorkbench: typeof CWorkbench;

  mainMenu?: ILoadingMainMenu | IMainMenu;

  parent?: any;
}
