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

import { IDataSource } from '@api/IArchitectApi';
import { TabViewState } from '@components/tabView/TabViewState';
import { observable } from 'mobx';

export class ToolboxState {
  @observable accessor name: string;
  @observable accessor selectedDataSourceId: string;
  @observable accessor isDirty: boolean = false;
  tabViewState: TabViewState = new TabViewState();
  onNameTopPropertiesChanged: ((warnings: string[]) => void) | undefined = undefined;

  constructor(
    public dataSources: IDataSource[],
    name: string,
    public schemaExtensionId: string,
    selectedDataSourceId: string,
    public id: string,
    private updateTopProperties: () => () => Generator<Promise<any>, string[], any>,
  ) {
    this.name = name;
    this.selectedDataSourceId = selectedDataSourceId;
  }

  selectedDataSourceIdChanged(value: string) {
    return function* (this: ToolboxState) {
      this.selectedDataSourceId = value;
      const warnings = yield* this.updateTopProperties()();
      if (this.onNameTopPropertiesChanged) {
        this.onNameTopPropertiesChanged(warnings);
      }
    }.bind(this);
  }

  nameChanged(value: string) {
    return function* (this: ToolboxState) {
      this.name = value;
      const warnings = yield* this.updateTopProperties()();
      if (this.onNameTopPropertiesChanged) {
        this.onNameTopPropertiesChanged(warnings);
      }
    }.bind(this);
  }
}
