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

import { IDataSource, IDataSourceData } from "./types/IDataSource";
import { IDataSourceField } from "./types/IDataSourceField";
import { IRowState } from "./types/IRowState";

export class DataSource implements IDataSource {
  $type_IDataSource: 1 = 1;

  constructor(data: IDataSourceData) {
    Object.assign(this, data);
    this.fields.forEach(o => (o.parent = this));
    this.rowState.parent = this;
  }

  parent?: any;

  entity: string = "";
  dataStructureEntityId: string = "";
  identifier: string = "";
  lookupCacheKey: string = "";
  fields: IDataSourceField[] = [];
  rowState: IRowState = null as any;

  getFieldByName(name: string): IDataSourceField | undefined {
    let field = this.fields.find(field => field.name === name);
    if(!field){
      const filedNames = this.fields.map(field => field.name).join(", ");
      throw new Error(`Filed named "${name}" was not found in data source fields : [${filedNames}]. Please make sure the DataBinding is set up correctly.`);
    }
    return field;
  }

  getFieldByIndex(index: number): IDataSourceField | undefined {
    return this.fields.find(field => field.index === index);
  }

  dispose(){
    this.rowState.dispose();
  }
}
