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
  IScreenEditorData,
  IScreenEditorModel,
  IToolBoxItem,
} from 'src/API/IArchitectApi.ts';
import { ToolboxState } from 'src/components/editors/designerEditor/common/ToolboxState.tsx';

export class ScreenToolboxState {
  toolboxState: ToolboxState;
  sections: IToolBoxItem[];
  widgets: IToolBoxItem[];

  constructor(
    screenEditorData: IScreenEditorData,
    id: string,
    private architectApi: IArchitectApi,
  ) {
    this.sections = screenEditorData.sections;
    this.widgets = screenEditorData.widgets;
    this.toolboxState = new ToolboxState(
      screenEditorData.dataSources,
      screenEditorData.name,
      screenEditorData.schemaExtensionId,
      screenEditorData.selectedDataSourceId,
      id,
      this.updateTopProperties.bind(this),
    );
  }

  private updateTopProperties() {
    return function* (
      this: ScreenToolboxState,
    ): Generator<Promise<IScreenEditorModel>, void, IScreenEditorModel> {
      const updateResult = yield this.architectApi.updateScreenEditor({
        schemaItemId: this.toolboxState.id,
        name: this.toolboxState.name,
        selectedDataSourceId: this.toolboxState.selectedDataSourceId,
        modelChanges: [],
      });
      const newData = updateResult.data;
      this.toolboxState.name = newData.name;
      this.toolboxState.selectedDataSourceId = newData.selectedDataSourceId;
    }.bind(this);
  }
}
