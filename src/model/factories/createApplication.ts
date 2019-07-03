import { Application } from "../Application";
import { OrigamAPI } from "../OrigamAPI";
import { ApplicationLifecycle } from "../ApplicationLifecycle";
import { IApplication } from "../types/IApplication";

export function createApplication(): IApplication {
  return new Application({
    api: new OrigamAPI(),
    applicationLifecycle: new ApplicationLifecycle()
  });
}
