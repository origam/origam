import {ErrorDialogController} from "model/entities/ErrorDialog";
import {Application} from "../entities/Application";
import {ApplicationLifecycle} from "../entities/ApplicationLifecycle";
import {DialogStack} from "../entities/DialogStack";
import {OrigamAPI} from "../entities/OrigamAPI";
import {IApplication} from "../entities/types/IApplication";
import {handleError} from "../actions/handleError";
import {flow} from "mobx";


export function createApplication(): IApplication {
  const applicationLifecycle = new ApplicationLifecycle();

  const apiErrorHandler = (error: any) => {
    flow(function* apiErrorHandler() {
      yield* handleError(applicationLifecycle)(error);
    })();
  };

  return new Application({
    api: new OrigamAPI(apiErrorHandler),
    applicationLifecycle: applicationLifecycle,
    dialogStack: new DialogStack(),
    errorDialogController: new ErrorDialogController()
  });
}
