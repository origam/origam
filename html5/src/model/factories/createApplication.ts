import { Application } from "../entities/Application";
import { OrigamAPI } from "../entities/OrigamAPI";
import { ApplicationLifecycle } from "../entities/ApplicationLifecycle";
import { IApplication } from "../entities/types/IApplication";
import { OpenedScreens } from '../entities/OpenedScreens';
import { DialogStack } from "../entities/DialogStack";


export function createApplication(): IApplication {
  return new Application({
    api: new OrigamAPI(),
    applicationLifecycle: new ApplicationLifecycle(),
    dialogStack: new DialogStack()
  });
}
