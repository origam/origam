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

import {IDataView} from "./IDataView";

export interface IComponentBindingData {
  parentId: string;
  parentEntity: string;
  childId: string;
  childEntity: string;
  childPropertyType: string;
  bindingPairs: IComponentBindingPair[];
}

export interface IComponentBinding extends IComponentBindingData {
  $type_IComponentBinding: 1;

  bindingController: Array<[string, any]>;
  parentDataView: IDataView;
  childDataView: IDataView;
  isBindingControllerValid: boolean;
  parent?: any;
}

export interface IComponentBindingPairData {
  parentPropertyId: string;
  childPropertyId: string;
}

export interface IComponentBindingPair extends IComponentBindingPairData {
  parent?: any;
}

export const isIComponentBinding = (o: any): o is IComponentBinding =>
  o.$type_IComponentBinding;
