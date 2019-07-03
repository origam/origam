import { IWorkbench, IWorkbenchData } from "./types/IWorkbench";
import { ILoadingMainMenu, IMainMenu } from "./types/IMainMenu";

export class Workbench implements IWorkbench {
  CWorkbench: "CWorkbench" = "CWorkbench";

  constructor(data: IWorkbenchData) {
    Object.assign(this, data);
  }

  mainMenu?: ILoadingMainMenu | IMainMenu | undefined;

  parent?: any;
}
