import { IApplication, IApplicationData } from "./types/IApplication";
import { IWorkbench } from "./types/IWorkbench";
import { ApplicationLifecycle } from "./ApplicationLifecycle";
import { IApplicationLifecycle } from "./types/IApplicationLifecycle";
import { OrigamAPI } from "./OrigamAPI";
import { IApi } from "./types/IApi";
import { action } from "mobx";
import { IOpenedScreens } from "./types/IOpenedScreens";
import { IDialogStack } from "./types/IDialogStack";
import { handleError } from "model/actions/handleError";

export class Application implements IApplication {
  $type_IApplication: 1 = 1;

  constructor(data: IApplicationData) {
    Object.assign(this, data);
    this.applicationLifecycle.parent = this;
    this.dialogStack.parent = this;
  }

  applicationLifecycle: IApplicationLifecycle = null as any;
  api: IApi = null as any;
  dialogStack: IDialogStack = null as any;

  workbench?: IWorkbench;

  resetWorkbench(): void {
    this.workbench = undefined;
  }

  setWorkbench(workbench: IWorkbench): void {
    this.workbench = workbench;
    workbench.parent = this;
  }

  *run() {
    try {
      yield* this.applicationLifecycle.run();
    } catch (e) {
      yield* handleError(this)(e);
      throw e;
    }
  }

  parent?: any;
}
