import { ILoadingMainMenu, IMainMenu } from "./IMainMenu";
import { IWorkbenchLifecycle } from "./IWorkbenchLifecycle";
import { IClientFulltextSearch } from "./IClientFulltextSearch";

export interface IWorkbenchData {
  workbenchLifecycle: IWorkbenchLifecycle;
  clientFulltextSearch: IClientFulltextSearch;
}

export interface IWorkbench extends IWorkbenchData {
  $type_IWorkbench: 1;

  mainMenu?: ILoadingMainMenu | IMainMenu;
  

  run(): void;
  setMainMenu(mainMenu: IMainMenu | ILoadingMainMenu): void;

  parent?: any;
}

export const isIWorkbench = (o: any): o is IWorkbench => o.$type_IWorkbench;
