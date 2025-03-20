/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

import { IDataTable } from "model/entities/types/IDataTable";
import {
  getDataSourceFieldByIndex
} from "model/selectors/DataSources/getDataSourceFieldByIndex";
import { IDataView } from "model/entities/types/IDataView";

export function getChangedColumns(dataView: IDataView, dirtyValueRows: any[][]){
  return dirtyValueRows.reduce(
    (result, row) => [...result, ...changedColumns(dataView.dataTable, row)],
    []);
}

function changedColumns(dataTable: IDataTable, oldRow: any[]) {
  const rowId = dataTable.getRowId(oldRow);
  const newRow = dataTable.getRowById(rowId)!;
  const changedColumnNames = [];

  for (let i = 0; i < oldRow.length; i++) {
    const oldValue = oldRow[i];
    const newValue = newRow[i];

    let isChanged = false;

    if (Array.isArray(oldValue) && Array.isArray(newValue)) {
      isChanged = !areArraysEqual(oldValue, newValue);
    } else if (oldValue !== newValue) {
      isChanged = true;
    }

    if (isChanged) {
      const columnName = getDataSourceFieldByIndex(dataTable, i)!;
      changedColumnNames.push(columnName.name);
    }
  }
  return changedColumnNames;
}

function areArraysEqual(arr1: any[], arr2: any[]): boolean {
  if (arr1.length !== arr2.length) return false;
  return arr1.every((value, index) => value === arr2[index]);
}
