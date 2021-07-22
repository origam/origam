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

import {flow} from "mobx";
import {IAction, IActionMode} from "model/entities/types/IAction";
import {getEntity} from "model/selectors/DataView/getEntity";
import {getGridId} from "model/selectors/DataView/getGridId";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";
import {getSelectedRowId} from "../../selectors/TablePanelView/getSelectedRowId";
import {handleError} from "model/actions/handleError";
import { getDataView } from "model/selectors/DataView/getDataView";

let isRunning = false;

export function onSelectionDialogActionButtonClick(ctx: any) {
  return flow(function*(event: any, action: IAction) {
    try {
      // TODO: Block "re-submission" for all ui actions
      if (isRunning) return;
      try {
        isRunning = true;

        // TODO: Wait for other async operations to finish successfully

        const lifecycle = getFormScreenLifecycle(ctx);
        const gridId = getGridId(ctx);
        const entity = getEntity(ctx);
        const rowId = getSelectedRowId(ctx);
        const dataView = getDataView(ctx);
        if (rowId) {
          yield* lifecycle.onFlushData();
          const selectedItems: string[] = action.mode === IActionMode.MultipleCheckboxes 
            ? Array.from(dataView.selectedRowIds) 
            : [rowId];
          yield* lifecycle.onExecuteAction(
            gridId,
            entity,
            action,
            selectedItems
          );
        }
      } finally {
        isRunning = false;
      }
    } catch (e) {
      yield* handleError(ctx)(e);
      throw e;
    }
  });
}
