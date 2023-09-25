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

import { IDataSourceField } from "./IDataSourceField";
import { IRowState } from "./IRowState";

export interface IDataSourceData {
  entity: string;
  identifier: string;
  lookupCacheKey: string;
  fields: IDataSourceField[];
  dataStructureEntityId: string;
  rowState: IRowState;
}

export interface IDataSource extends IDataSourceData {
  $type_IDataSource: 1;

  getFieldByName(name: string): IDataSourceField | undefined;

  getFieldByIndex(idex: number): IDataSourceField | undefined;

  parent?: any;

  dispose(): void;
}

export const isIDataSource = (o: any): o is IDataSource => o.$type_IDataSource;
