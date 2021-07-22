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

import {
  IComponentBinding,
  IComponentBindingData,
  IComponentBindingPair,
  IComponentBindingPairData
} from "./types/IComponentBinding";
import {computed} from "mobx";
import {IDataView} from "./types/IDataView";
import {getFormScreen} from "../selectors/FormScreen/getFormScreen";
import {getDataTable} from "../selectors/DataView/getDataTable";

export class ComponentBindingPair implements IComponentBindingPair {
  constructor(data: IComponentBindingPairData) {
    Object.assign(this, data);
  }

  parent?: any;
  parentPropertyId: string = "";
  childPropertyId: string = "";
}

export class ComponentBinding implements IComponentBinding {
  $type_IComponentBinding: 1 = 1;
  
  constructor(data: IComponentBindingData) {
    Object.assign(this, data);
    this.bindingPairs.forEach(o => (o.parent = this));
  }

  parentId: string = "";
  parentEntity: string = "";
  childId: string = "";
  childEntity: string = "";
  childPropertyType: string = "";
  bindingPairs: IComponentBindingPair[] = [];

  @computed get parentDataView(): IDataView {
    const screen = getFormScreen(this);
    return screen.getDataViewByModelInstanceId(this.parentId)!;
  }

  @computed get childDataView(): IDataView {
    const screen = getFormScreen(this);
    return screen.getDataViewByModelInstanceId(this.childId)!;
  }

  @computed get bindingController() {
    const c: Array<[string, any]> = [];
    const parentDataTable = getDataTable(this.parentDataView);
    for (let pair of this.bindingPairs) {
      const row = this.parentDataView.selectedRow;
      const property = parentDataTable.getPropertyById(pair.parentPropertyId);
      if (row && property) {
        c.push([
          pair.childPropertyId,
          parentDataTable.getCellValue(row, property)
        ]);
      }
    }
    return c;
  }

  @computed get isBindingControllerValid() {
    return this.bindingController.every(pair => pair[0] && pair[1])
  }

  parent?: any;
}
