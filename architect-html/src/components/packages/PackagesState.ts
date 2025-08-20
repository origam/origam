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

import { observable } from 'mobx';
import { IArchitectApi, IPackage, IPackagesInfo } from 'src/API/IArchitectApi.ts';
import { ProgressBarState } from 'src/components/topBar/ProgressBarState.ts';
import { TabViewState } from 'src/components/tabView/TabViewState.ts';
import { ModelTreeState } from 'src/components/modelTree/ModelTreeState.ts';
import { UIState } from 'src/stores/UIState.ts';

export class PackagesState {
  @observable.shallow accessor packages: IPackage[] = [];
  @observable accessor activePackageId: string | undefined;
  private activePackageChanged = false;

  constructor(
    private progressBarState: ProgressBarState,
    private sideBarTabViewState: TabViewState,
    private modelTreeState: ModelTreeState,
    private uiState: UIState,
    private architectApi: IArchitectApi,
  ) {}

  *loadPackages(): Generator<Promise<IPackagesInfo>, void, IPackagesInfo> {
    const packagesInfo = yield this.architectApi.getPackages();
    this.packages = packagesInfo.packages ?? [];
    if (packagesInfo.activePackageId) {
      yield* this.setActivePackage(packagesInfo.activePackageId)() as any;
      this.activePackageChanged = true;
    }
  }

  private setActivePackage(packageId: string) {
    return function* (this: PackagesState) {
      this.progressBarState.isWorking = true;
      try {
        yield this.architectApi.setActivePackage(packageId);
        this.activePackageId = packageId;
        yield* this.modelTreeState.loadPackageNodes();
        this.sideBarTabViewState.showModelTree();
      } finally {
        this.progressBarState.isWorking = false;
      }
    }.bind(this);
  }

  setActivePackageClick(packageId: string) {
    return function* (this: PackagesState) {
      yield* this.setActivePackage(packageId)();
      if (this.activePackageChanged) {
        this.uiState.clearExpandedNodes();
      }
      this.activePackageChanged = true;
    }.bind(this);
  }
}
