/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { handleError } from "model/actions/handleError";
import { IApi } from "./types/IApi";
import { IApplication, IApplicationData } from "./types/IApplication";
import { IApplicationLifecycle } from "./types/IApplicationLifecycle";
import { IDialogStack } from "./types/IDialogStack";
import { IErrorDialogController } from "./types/IErrorDialog";
import { IWorkbench } from "./types/IWorkbench";
import { MobileState } from "model/entities/MobileState/MobileState";
import { observable } from "mobx";

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
  mobileState = new MobileState();

  resetWorkbench(): void {
    this.workbench = undefined;
  }

  setWorkbench(workbench: IWorkbench): void {
    this.workbench = workbench;
    workbench.parent = this;
    this.mobileState.initialize(workbench);
  }

  *run() {
    try {
      yield*this.applicationLifecycle.run();
    } catch (e) {
      yield*handleError(this)(e);
      throw e;
    }
  }

  parent?: any;

  @observable
  breakpoint = "";
}
