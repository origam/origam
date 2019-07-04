import { Application } from "../Application";
import { OrigamAPI } from "../OrigamAPI";
import { ApplicationLifecycle } from "../ApplicationLifecycle";
import { IApplication } from "../types/IApplication";
import { OpenedScreens } from '../OpenedScreens';

export function createApplication(): IApplication {
  return new Application({
    api: new OrigamAPI(),
    applicationLifecycle: new ApplicationLifecycle(),
    openedScreens: new OpenedScreens()
  });
}
