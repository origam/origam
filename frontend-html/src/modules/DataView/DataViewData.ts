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

import { TypeSymbol } from "dic/Container";
import { IDataTable } from "model/entities/types/IDataTable";
import { IProperty } from "model/entities/types/IProperty";
import { onFieldChange } from "model/actions-ui/DataView/TableView/onFieldChange";
import { runInFlowWithHandler } from "utils/runInFlowWithHandler";

export class DataViewData {
  constructor(
    private dataTable: () => IDataTable,
    private propertyById: (id: string) => IProperty | undefined
  ) {
  }

  getCellValue(rowId: string, propertyId: string) {
    const dataTable = this.dataTable();
    const property = this.propertyById(propertyId);
    const row = dataTable.getRowById(rowId);
    if (property && row) {
      return dataTable.getCellValue(row, property);
    } else return null;
  }

  getCellText(propertyId: string, value: any) {
    const property = this.propertyById(propertyId);
    if (property && value) {
      return this.dataTable().resolveCellText(property, value);
    } else return null;
  }

  getIsCellTextLoading(propertyId: string, value: any): boolean {
    const property = this.propertyById(propertyId);
    if (property && value) {
      return this.dataTable().isCellTextResolving(property, value);
    } else return false;
  }

  async setNewValue(rowId: string, propertyId: string, value: any) {
    const dataTable = this.dataTable();
    const row = dataTable.getRowById(rowId);
    const property = this.propertyById(propertyId);
    if (property && row) {
      await onFieldChange(property)({
        event: undefined,
        row: row,
        property: property,
        value: value,
      });
    }
  }
}

export const IDataViewData = TypeSymbol<DataViewData>("IDataViewData");
