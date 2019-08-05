import {  IMainMenu, IMainMenuEnvelope } from "./IMainMenu";
import { IWorkbenchLifecycle } from "./IWorkbenchLifecycle";
import { IClientFulltextSearch } from "./IClientFulltextSearch";

export interface IWorkbenchData {
  mainMenuEnvelope: IMainMenuEnvelope;
  workbenchLifecycle: IWorkbenchLifecycle;
  clientFulltextSearch: IClientFulltextSearch;
}

export interface IWorkbench extends IWorkbenchData {
  $type_IWorkbench: 1;

  mainMenuEnvelope: IMainMenuEnvelope;  
  run(): void;

  parent?: any;
}

export const isIWorkbench = (o: any): o is IWorkbench => o.$type_IWorkbench;
