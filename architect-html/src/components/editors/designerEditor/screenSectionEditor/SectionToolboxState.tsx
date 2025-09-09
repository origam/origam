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

import {
  IArchitectApi,
  IEditorField,
  ISectionEditorData,
  ISectionEditorModel,
} from '@api/IArchitectApi';
import { ToolboxState } from '@editors/designerEditor/common/ToolboxState';
import { observable } from 'mobx';

export class SectionToolboxState {
  toolboxState: ToolboxState;
  @observable accessor fields: IEditorField[];
  @observable accessor selectedFieldName: string | undefined;

  constructor(
    sectionEditorData: ISectionEditorData,
    id: string,
    private architectApi: IArchitectApi,
  ) {
    this.toolboxState = new ToolboxState(
      sectionEditorData.dataSources,
      sectionEditorData.name,
      sectionEditorData.schemaExtensionId,
      sectionEditorData.selectedDataSourceId,
      id,
      this.updateTopProperties.bind(this),
    );
    this.fields = sectionEditorData.fields;
  }

  private updateTopProperties() {
    return function* (
      this: SectionToolboxState,
    ): Generator<Promise<ISectionEditorModel>, void, ISectionEditorModel> {
      const updateResult = yield this.architectApi.updateSectionEditor({
        schemaItemId: this.toolboxState.id,
        name: this.toolboxState.name,
        selectedDataSourceId: this.toolboxState.selectedDataSourceId,
        modelChanges: [],
      });
      const newData = updateResult.data;
      this.toolboxState.name = newData.name;
      this.toolboxState.selectedDataSourceId = newData.selectedDataSourceId;
      this.fields = newData.fields;
    }.bind(this);
  }
}
