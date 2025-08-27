/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

import { ArchitectApi } from '@api/ArchitectApi.ts';
import { IArchitectApi } from '@api/IArchitectApi.ts';
import { EditorTabViewState } from '@components/editorTabView/EditorTabViewState.ts';
import { ModelTreeState } from '@components/modelTree/ModelTreeState.ts';
import { PackagesState } from '@components/packages/PackagesState.ts';
import { PropertiesState } from '@components/properties/PropertiesState';
import { TabViewState } from '@components/tabView/TabViewState.ts';
import { ProgressBarState } from '@components/topBar/ProgressBarState.ts';
import { DialogStackState } from '@dialogs/DialogStackState.tsx';
import { IDialogStackState } from '@dialogs/types.ts';
import { ErrorDialogController } from '@errors/ErrorDialog.tsx';
import { TranslationsStore } from '@stores/TranslationsStore.tsx';
import { UIState } from '@stores/UiState.ts';

export class RootStore {
  public editorTabViewState: EditorTabViewState;
  public sideBarTabViewState = new TabViewState();
  public uiState = new UIState();
  public packagesState: PackagesState;
  public modelTreeState: ModelTreeState;
  public architectApi: IArchitectApi = new ArchitectApi();
  public dialogStack: IDialogStackState = new DialogStackState();
  public errorDialogController: ErrorDialogController;
  public progressBarState = new ProgressBarState();
  public propertiesState = new PropertiesState();
  public translations = new TranslationsStore();

  constructor() {
    this.errorDialogController = new ErrorDialogController(this.dialogStack);
    this.editorTabViewState = new EditorTabViewState(this);
    this.modelTreeState = new ModelTreeState(this);
    this.packagesState = new PackagesState(
      this.progressBarState,
      this.sideBarTabViewState,
      this.modelTreeState,
      this.uiState,
      this.architectApi,
    );
  }
}
