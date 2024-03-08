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

import { IApi } from "./IApi";
import { IApplicationLifecycle } from "./IApplicationLifecycle";
import { IDialogStack } from "./IDialogStack";
import { IWorkbench } from "./IWorkbench";
import { IErrorDialogController } from "./IErrorDialog";
import { MobileState } from "model/entities/MobileState/MobileState";

export interface IApplicationData {
  api: IApi;
  applicationLifecycle: IApplicationLifecycle;
  dialogStack: IDialogStack;
  errorDialogController: IErrorDialogController;
}

export interface IApplication extends IApplicationData {
  $type_IApplication: 1;

  workbench?: IWorkbench;

  mobileState: MobileState;

  parent?: any;

  layout: Layout;

  resetWorkbench(): void;

  setWorkbench(workbench: IWorkbench): void;

  run(): Generator;
}

export enum Layout {
  Phone = "phone",
  Tablet = "tablet",
  Desktop = "desktop",
}

export const isIApplication = (o: any): o is IApplication =>
  o.$type_IApplication;

