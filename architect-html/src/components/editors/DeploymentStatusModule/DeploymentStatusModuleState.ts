/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

import { IEditorState } from '@/components/editorTabView/IEditorState';
import { T } from '@/main';
import { IArchitectApi, IDeploymentStatusResponse } from '@api/IArchitectApi';
import { UIState } from '@stores/UiState';
import { flow, observable } from 'mobx';

export default class DeploymentStatusModuleState implements IEditorState {
  @observable accessor response: IDeploymentStatusResponse;
  @observable accessor isActive = false;
  @observable accessor expandedPackages: Set<string> = new Set();

  isDirty = false;
  label = T('Deployment Status', 'editor_DeploymentStatus_TabLabel');

  constructor(
    public editorId: string,
    response: IDeploymentStatusResponse,
    protected architectApi: IArchitectApi,
    protected uiState: UIState,
  ) {
    this.response = response;
    this.expandedPackages = new Set(uiState.getDeploymentStatusState().expandedPackageIds);
  }

  isExpanded(packageId: string): boolean {
    return this.expandedPackages.has(packageId);
  }

  toggleExpanded(packageId: string) {
    const next = new Set(this.expandedPackages);
    if (next.has(packageId)) {
      next.delete(packageId);
    } else {
      next.add(packageId);
    }
    this.expandedPackages = next;
    this.uiState.setDeploymentStatusState({ expandedPackageIds: Array.from(next) });
  }

  save(): Generator<Promise<any>, void, any> {
    throw new Error('Method is unnecessary.');
  }

  reload = flow(function* (this: DeploymentStatusModuleState) {
    this.response = yield this.architectApi.fetchDeploymentStatus();
  });
}
