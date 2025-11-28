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

import { ArchitectApi } from '@api/ArchitectApi';
import { IArchitectApi } from '@api/IArchitectApi';
import { EditorTabViewState } from '@components/editorTabView/EditorTabViewState';
import { ModelTreeState } from '@components/modelTree/ModelTreeState';
import { PackagesState } from '@components/packages/PackagesState';
import { PropertiesState } from '@components/properties/PropertiesState';
import { TabViewState } from '@components/tabView/TabViewState';
import { ProgressBarState } from '@components/topBar/ProgressBarState';
import { DialogStackState } from '@dialogs/DialogStackState';
import { IDialogStackState } from '@dialogs/types';
import { ErrorDialogController } from '@errors/ErrorDialog';
import { TranslationsStore } from '@stores/TranslationsStore';
import { UIState } from '@stores/UiState';
import { observable } from 'mobx';

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
  @observable public accessor output: string = '';

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
