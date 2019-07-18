import { IWorkbench } from "./IWorkbench";
import { IApi } from "./IApi";
import { IApplicationLifecycle } from "./IApplicationLifecycle";
import { IOpenedScreens } from "./IOpenedScreens";
import { IDialogStack } from "./IDialogStack";

export interface IApplicationData {
  api: IApi;
  applicationLifecycle: IApplicationLifecycle;
  openedScreens: IOpenedScreens;
  dialogStack: IDialogStack;
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
