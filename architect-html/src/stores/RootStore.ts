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

import { ArchitectApi } from "src/API/ArchitectApi.ts";
import { IArchitectApi } from "src/API/IArchitectApi.ts";
import {
  EditorTabViewState
} from "src/components/editorTabView/EditorTabViewState.ts";
import { TabViewState } from "src/components/tabView/TabViewState.ts";
import { UiState } from "src/stores/UiState.ts";
import { PackagesState } from "src/components/packages/PackagesState.ts";
import { ModelTreeState } from "src/components/modelTree/ModelTreeState.ts";

export class RootStore {
  public projectState: ProjectState;

  constructor() {
    const architectApi = new ArchitectApi();
    this.projectState = new ProjectState(architectApi);
  }
}


export class ProjectState {
  private editorTabViewState: EditorTabViewState;
  private sideBarTabViewState = new TabViewState();
  private uiState = new UiState();
  private packagesState: PackagesState;
  private modelTreeState: ModelTreeState;

  public getEditorTabViewState() {
    return this.editorTabViewState;
  }
  public getUiState() {
    return this.uiState;
  }
  public getSideBarTabViewState() {
    return this.sideBarTabViewState;
  }
  public getPackagesState() {
    return this.packagesState;
  }

  public getModelTreeState() {
    return this.modelTreeState;
  }

  constructor(private architectApi: IArchitectApi) {
    this.packagesState = new PackagesState(architectApi);
    this.editorTabViewState = new EditorTabViewState(architectApi, this);
    this.modelTreeState = new ModelTreeState(architectApi, this);
  }

  showModelTree() {
    this.sideBarTabViewState.activeTabIndex = 1;
  }
}



