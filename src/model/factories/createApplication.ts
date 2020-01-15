import { ErrorDialogController } from "model/entities/ErrorDialog";
import { Application } from "../entities/Application";
import { ApplicationLifecycle } from "../entities/ApplicationLifecycle";
import { DialogStack } from "../entities/DialogStack";
import { OrigamAPI } from "../entities/OrigamAPI";
import { IApplication } from "../entities/types/IApplication";


export function createApplication(): IApplication {
  return new Application({
    api: new OrigamAPI(),
    applicationLifecycle: new ApplicationLifecycle(),
    dialogStack: new DialogStack(),
    errorDialogController: new ErrorDialogController()
  });
}
