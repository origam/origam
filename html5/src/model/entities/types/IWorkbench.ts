import {  IMainMenu, IMainMenuEnvelope } from "./IMainMenu";
import { IWorkbenchLifecycle } from "./IWorkbenchLifecycle";
import { IClientFulltextSearch } from "./IClientFulltextSearch";
import { IOpenedScreens } from "./IOpenedScreens";

export interface IWorkbenchData {
  mainMenuEnvelope: IMainMenuEnvelope;
  workbenchLifecycle: IWorkbenchLifecycle;
  clientFulltextSearch: IClientFulltextSearch;
  openedScreens: IOpenedScreens;
}

export interface IWorkbench extends IWorkbenchData {
  $type_IWorkbench: 1;

  mainMenuEnvelope: IMainMenuEnvelope;  
  run(): void;

  parent?: any;
}

export const isIWorkbench = (o: any): o is IWorkbench => o.$type_IWorkbench;
