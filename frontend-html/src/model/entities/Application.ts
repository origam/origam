import {handleError} from "model/actions/handleError";
import {IApi} from "./types/IApi";
import {IApplication, IApplicationData} from "./types/IApplication";
import {IApplicationLifecycle} from "./types/IApplicationLifecycle";
import {IDialogStack} from "./types/IDialogStack";
import {IErrorDialogController} from "./types/IErrorDialog";
import {IWorkbench} from "./types/IWorkbench";

export class Application implements IApplication {
  
  $type_IApplication: 1 = 1;

  constructor(data: IApplicationData) {
    Object.assign(this, data);
    this.applicationLifecycle.parent = this;
    this.dialogStack.parent = this;
    this.errorDialogController.parent = this;
  }

  errorDialogController: IErrorDialogController = null as any;
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
