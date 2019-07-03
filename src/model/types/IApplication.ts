import { IWorkbench } from "./IWorkbench";
import { IApi } from "./IApi";
import { IApplicationLifecycle } from "./IApplicationLifecycle";

export interface IApplicationData {
  api: IApi;
  applicationLifecycle: IApplicationLifecycle;
}

export interface IApplication extends IApplicationData {
  workbench?: IWorkbench;
  
  parent?: any;

  resetWorkbench(): void;
  setWorkbench(workbench: IWorkbench): void;

}
