import { IApplication, IApplicationData } from "./types/IApplication";
import { IWorkbench } from "./types/IWorkbench";
import { ApplicationLifecycle } from "./ApplicationLifecycle";
import { IApplicationLifecycle } from "./types/IApplicationLifecycle";
import { OrigamAPI } from "./OrigamAPI";
import { IApi } from "./types/IApi";
import { action } from "mobx";

export class Application implements IApplication {


  parent?: any;

  constructor(data: IApplicationData) {
    Object.assign(this, data);
    this.applicationLifecycle.parent = this;
  }

  applicationLifecycle: IApplicationLifecycle = null as any;
  api: IApi = null as any;

  workbench?: IWorkbench;

  resetWorkbench(): void {
    this.workbench = undefined;
  }

  setWorkbench(workbench: IWorkbench): void {
    this.workbench = workbench;
  }

  @action.bound
  run(): void {
    this.applicationLifecycle.run();
  }
}
