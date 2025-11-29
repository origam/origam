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
import { IArchitectApi, IDatabaseResult } from '@api/IArchitectApi';
import { observable } from 'mobx';

export default class DeploymentScriptsGeneratorEditorState implements IEditorState {
  @observable accessor results: IDatabaseResult[];
  @observable accessor isSaving = false;
  @observable accessor isActive = false;
  @observable accessor selectedItems: Set<string> = new Set();

  label = 'Deployment Scripts Generator';
  isDirty = false;

  constructor(
    public editorId: string,
    results: IDatabaseResult[],
    protected architectApi: IArchitectApi,
  ) {
    this.results = results ?? [];
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
}
