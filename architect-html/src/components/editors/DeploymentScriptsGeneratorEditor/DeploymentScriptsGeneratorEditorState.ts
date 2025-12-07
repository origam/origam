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

import { IEditorState } from '@/components/editorTabView/IEditorState';
import { ModelTreeState } from '@/components/modelTree/ModelTreeState';
import { IArchitectApi, IDatabaseResult, IDeploymentVersion } from '@api/IArchitectApi';
import { flow, observable } from 'mobx';

export default class DeploymentScriptsGeneratorEditorState implements IEditorState {
  @observable accessor results: IDatabaseResult[];
  @observable accessor isSaving = false;
  @observable accessor isActive = false;
  @observable accessor selectedItems: Set<string> = new Set();
  @observable accessor possibleDeploymentVersions: IDeploymentVersion[];
  @observable accessor currentDeploymentVersionId: string | null;

  label = 'Deployment Scripts Generator';
  isDirty = false;

  constructor(
    public editorId: string,
    results: IDatabaseResult[],
    possibleDeploymentVersions: IDeploymentVersion[],
    currentDeploymentVersionId: string | null,
    protected architectApi: IArchitectApi,
    protected modelTreeState: ModelTreeState,
  ) {
    this.results = results ?? [];
    this.possibleDeploymentVersions = possibleDeploymentVersions ?? [];
    this.currentDeploymentVersionId = currentDeploymentVersionId;
  }

  save(): Generator<Promise<any>, void, any> {
    throw new Error('Method not implemented.');
  }

  toggleSelection(schemaItemId: string) {
    if (this.selectedItems.has(schemaItemId)) {
      this.selectedItems.delete(schemaItemId);
    } else {
      this.selectedItems.add(schemaItemId);
    }
  }

  selectAll() {
    this.selectedItems = new Set(
      this.results.filter(item => item.schemaItemId).map(item => item.schemaItemId!),
    );
  }

  clearSelection() {
    this.selectedItems = new Set();
  }

  isSelected(schemaItemId: string): boolean {
    return this.selectedItems.has(schemaItemId);
  }

  getSelectedPlatform(): string | null {
    const selectedResults = this.results.filter(r => this.selectedItems.has(r.schemaItemId));
    if (selectedResults.length === 0) {
      return null;
    }

    const platforms = new Set(selectedResults.map(r => r.platformName));
    if (platforms.size !== 1) {
      return null;
    }

    return selectedResults[0].platformName;
  }

  addToDeployment = flow(function* (this: DeploymentScriptsGeneratorEditorState) {
    const platform = this.getSelectedPlatform();
    if (!platform || !this.currentDeploymentVersionId) {
      return;
    }

    yield this.architectApi.addToDeployment({
      platform,
      deploymentVersionId: this.currentDeploymentVersionId,
      schemaItemIds: Array.from(this.selectedItems),
    });

    yield* this.modelTreeState.loadPackageNodes();
  });
}
