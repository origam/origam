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

import { IArchitectApi } from "src/API/IArchitectApi.ts";
import {
  EditorTabViewState
} from "src/components/editorTabView/EditorTabViewState.ts";
import { TabViewState } from "src/components/tabView/TabViewState.ts";
import { UiState } from "src/stores/UiState.ts";
import { PackagesState } from "src/components/packages/PackagesState.ts";
import { ModelTreeState } from "src/components/modelTree/ModelTreeState.ts";
import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { DialogStackState } from "src/dialog/DialogStackState.tsx";
import { IDialogStackState } from "src/dialog/types.ts";
import { ErrorDialogController } from "src/errorHandling/ErrorDialog.tsx";

export class RootStore {
  public editorTabViewState: EditorTabViewState;
  public sideBarTabViewState = new TabViewState();
  public uiState = new UiState();
  public packagesState: PackagesState;
  public modelTreeState: ModelTreeState;
  public architectApi: IArchitectApi = new ArchitectApi();
  public dialogStack: IDialogStackState = new DialogStackState();
  public errorDialogController: ErrorDialogController;

  constructor() {
    this.errorDialogController = new ErrorDialogController(this.dialogStack);
    this.packagesState = new PackagesState(this.architectApi);
    this.editorTabViewState = new EditorTabViewState(this);
    this.modelTreeState = new ModelTreeState(this);
  }
}



