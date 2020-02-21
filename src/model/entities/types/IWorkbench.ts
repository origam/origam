import {  IMainMenu, IMainMenuEnvelope } from "./IMainMenu";
import { IWorkbenchLifecycle } from "./IWorkbenchLifecycle";
import { IClientFulltextSearch } from "./IClientFulltextSearch";
import { IOpenedScreens } from "./IOpenedScreens";
import { IWorkQueues } from "./IWorkQueues";
import { IRecordInfo } from "./IRecordInfo";

export interface IWorkbenchData {
  mainMenuEnvelope: IMainMenuEnvelope;
  workbenchLifecycle: IWorkbenchLifecycle;
  clientFulltextSearch: IClientFulltextSearch;
  openedScreens: IOpenedScreens;
  openedDialogScreens: IOpenedScreens;
  workQueues: IWorkQueues;
  recordInfo: IRecordInfo;
}

export interface IWorkbench extends IWorkbenchData {
  $type_IWorkbench: 1;
  isFullScreen: boolean;

  // loggedUserName: any
  mainMenuEnvelope: IMainMenuEnvelope;  
  run(): Generator;
  setFullscreen(state: boolean): void;

  openedScreenIdSet: Set<string>;

  parent?: any;
}

export const isIWorkbench = (o: any): o is IWorkbench => o.$type_IWorkbench;
