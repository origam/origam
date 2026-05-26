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

import { T } from '@/main';
import { IArchitectApi, IUpdatePropertiesResult } from '@api/IArchitectApi';
import { IEditorNode } from '@components/editorTabView/EditorTabViewState';
import { IEditorState, IValidationError } from '@components/editorTabView/IEditorState';
import { EditorProperty, toChanges } from '@editors/gridEditor/EditorProperty';
import { IPropertyManager } from '@editors/propertyEditor/IPropertyManager';
import { computed, observable } from 'mobx';

export class GridEditorState implements IEditorState, IPropertyManager {
  @observable accessor properties: EditorProperty[];
  @observable accessor isSaving = false;
  @observable accessor isActive = false;
  @observable accessor _isDirty: boolean;

  constructor(
    public editorId: string,
    protected editorNode: IEditorNode,
    properties: EditorProperty[] | undefined,
    isDirty: boolean,
    protected architectApi: IArchitectApi,
  ) {
    this._isDirty = isDirty;
    this.properties = properties ?? [];
  }

  @computed
  get isDirty() {
    let someHasError = false;
    for (const property of this.properties) {
      someHasError = someHasError || !!property.error;
    }
    return this._isDirty && !someHasError;
  }

  @computed
  get validationErrors(): IValidationError[] {
    return this.properties
      .filter(p => p.error)
      .map(p => ({ propertyName: p.name, error: p.error! }));
  }

  get label() {
    const nameValue = this.properties.find(x => x.name === 'Name')?.value;
    return typeof nameValue === 'string' ? nameValue : '';
  }

  get title(): string {
    const typeName = this.editorNode.itemTypeName;
    const label = this.label;
    if (typeName && label) {
      return T('{0}: {1}', 'grid_editor_title_full', typeName, label);
    }
    if (typeName) {
      return typeName;
    }
    return T('Editing: {0}', 'grid_editor_title', label);
  }

  *save(): Generator<Promise<any>, void, any> {
    try {
      this.isSaving = true;
      yield this.architectApi.persistChanges(this.editorNode.origamId);
      if (this.editorNode.parent) {
        yield* this.editorNode.parent.loadChildren();
      }
      this._isDirty = false;
    } finally {
      this.isSaving = false;
    }
  }

  *onPropertyUpdated(
    property: EditorProperty,
    value: any,
  ): Generator<Promise<IUpdatePropertiesResult>, void, IUpdatePropertiesResult> {
    if (property.name === 'Name') {
      const oldName = property.value;
      for (const mappedName of ['MappedObjectName', 'MappedColumnName']) {
        const mapped = this.properties.find(p => p.name === mappedName);
        if (
          mapped &&
          !mapped.readOnly &&
          (mapped.value === null ||
            mapped.value === undefined ||
            mapped.value === '' ||
            mapped.value === oldName)
        ) {
          mapped.value = value;
        }
      }
    }
    property.value = value;
    const changes = toChanges(this.properties);
    const updateResult = (yield this.architectApi.updateProperties(
      this.editorNode.origamId,
      changes,
    )) as IUpdatePropertiesResult;
    for (const property of this.properties) {
      const propertyUpdate = updateResult.propertyUpdates.find(
        update => property.name === update.propertyName,
      );
      property.update(propertyUpdate);
    }
    this._isDirty = updateResult.isDirty;
  }
}
