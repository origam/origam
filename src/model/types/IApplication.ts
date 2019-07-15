import { IWorkbench } from "./IWorkbench";
import { IApi } from "./IApi";
import { IApplicationLifecycle } from "./IApplicationLifecycle";
import { IOpenedScreens } from "./IOpenedScreens";

export interface IApplicationData {
  api: IApi;
  applicationLifecycle: IApplicationLifecycle;
  openedScreens: IOpenedScreens;
}

export interface IApplication extends IApplicationData {
  $type_IApplication: 1;

  workbench?: IWorkbench;

  parent?: any;

  resetWorkbench(): void;
  setWorkbench(workbench: IWorkbench): void;
  run(): void;
}

export const isIApplication = (o: any): o is IApplication =>
  o.$type_IApplication;
