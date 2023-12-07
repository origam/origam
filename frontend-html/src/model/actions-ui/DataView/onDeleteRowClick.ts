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

import { getDataView } from "model/selectors/DataView/getDataView";
import { getSelectedRow } from "model/selectors/DataView/getSelectedRow";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { flow } from "mobx";
import { handleError } from "model/actions/handleError";
import { getTablePanelView } from "../../selectors/TablePanelView/getTablePanelView";

export function onDeleteRowClick(ctx: any) {
  return flow(function*onDeleteRowClick(event: any, doNotAskForConfirmation? : boolean) {
    try {
      const dataView = getDataView(ctx);
      getTablePanelView(dataView)?.setEditing(false);
      const selectedRow = getSelectedRow(ctx);
      if (selectedRow) {
        const dataTable = getDataTable(ctx);
        const entity = dataView.entity;
        const formScreenLifecycle = getFormScreenLifecycle(ctx);
        yield*formScreenLifecycle.onDeleteRow(
          entity,
          dataTable.getRowId(selectedRow),
          dataView,
          doNotAskForConfirmation
        );
        getTablePanelView(ctx)?.triggerOnFocusTable();
      }
    } catch (e) {
      yield*handleError(ctx)(e);
      throw e;
    }
  });
}
