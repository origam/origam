import { IApi } from "./IApi";
import { IApplicationLifecycle } from "./IApplicationLifecycle";
import { IDialogStack } from "./IDialogStack";
import { IWorkbench } from "./IWorkbench";

export interface IApplicationData {
  api: IApi;
  applicationLifecycle: IApplicationLifecycle;
  dialogStack: IDialogStack;
}

export interface IApplication extends IApplicationData {
  $type_IApplication: 1;

  workbench?: IWorkbench;

  parent?: any;

  resetWorkbench(): void;
  setWorkbench(workbench: IWorkbench): void;
  run(): Generator;
}

export const isIApplication = (o: any): o is IApplication =>
  o.$type_IApplication;
