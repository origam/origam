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

import { flow } from "mobx";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import {
  getSelectionMember
} from "model/selectors/DataView/getSelectionMember";
import {
  getDataSourceFieldByName
} from "model/selectors/DataSources/getDataSourceFieldByName";
import {
  getFormScreenLifecycle
} from "model/selectors/FormScreen/getFormScreenLifecycle";
import { setSelectedStateRowId } from "model/actions-tree/selectionCheckboxes";
import { handleError } from "model/actions/handleError";
import { getRowStates } from "model/selectors/RowState/getRowStates";
import { getDataView } from "model/selectors/DataView/getDataView";

export function setAllSelectionStates(ctx: any, selectionState: boolean) {
  flow(function* () {
    try {
      yield* updateRowStates(ctx);
      const dataTable = getDataTable(ctx);
      const selectionMember = getSelectionMember(ctx);
      if (!!selectionMember) {
        for (let row of dataTable.rows) {
          const dsField = getDataSourceFieldByName(ctx, selectionMember);
          if (dsField) {
            dataTable.setDirtyValue(row, selectionMember, selectionState);
          }
        }
        yield* getFormScreenLifecycle(ctx).onFlushData();
        for (let row of dataTable.rows) {
          const dataSourceField = getDataSourceFieldByName(ctx, selectionMember)!;
          const newSelectionState = dataTable.getCellValueByDataSourceField(row, dataSourceField);
          const rowId = dataTable.getRowId(row);
          yield* setSelectedStateRowId(ctx)(rowId, newSelectionState);
        }
      } else {
        for (let row of dataTable.rows) {
          const rowId = dataTable.getRowId(row);
          yield* setSelectedStateRowId(ctx)(rowId, selectionState);
        }
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  })();
}

function* updateRowStates(ctx: any) {
  const dataView = getDataView(ctx);
  const rowIds = dataView.dataTable.rows.map(row => dataView.dataTable.getRowId(row));
  yield* getRowStates(ctx).loadValues(rowIds);
}