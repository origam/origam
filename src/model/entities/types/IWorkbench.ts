import {  IMainMenu, IMainMenuEnvelope } from "./IMainMenu";
import { IWorkbenchLifecycle } from "./IWorkbenchLifecycle";
import { IClientFulltextSearch } from "./IClientFulltextSearch";
import { IOpenedScreens } from "./IOpenedScreens";

export interface IWorkbenchData {
  mainMenuEnvelope: IMainMenuEnvelope;
  workbenchLifecycle: IWorkbenchLifecycle;
  clientFulltextSearch: IClientFulltextSearch;
  openedScreens: IOpenedScreens;
  openedDialogScreens: IOpenedScreens;
}

export interface IWorkbench extends IWorkbenchData {
  $type_IWorkbench: 1;
  isFullScreen: boolean;

  // loggedUserName: any
  mainMenuEnvelope: IMainMenuEnvelope;  
  run(): Generator;
  setFullscreen(state: boolean): void;

  parent?: any;
}

export const isIWorkbench = (o: any): o is IWorkbench => o.$type_IWorkbench;
